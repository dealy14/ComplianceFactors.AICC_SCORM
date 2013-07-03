using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using EntityFrameworkLayer;

namespace AICC_CMI
{
    public abstract class BaseLogic
    {
        private delegate void FieldDelegate(string key, Dictionary<string, string> json_values);

        private Dictionary<string, string> m_json_values = null;
        private Dictionary<string, object> m_values = new Dictionary<string, object>();
        private Dictionary<string, FieldDelegate> m_delegates = new Dictionary<string, FieldDelegate>();
        private Dictionary<string, string> m_dbmap = new Dictionary<string,string>();
        protected Dictionary<string, string> m_map_lesson_status = new Dictionary<string,string>();
        protected List<string> m_lesson_statuses_completed = new List<string>();

        public BaseLogic()
        {
            InitDelegates();
            InitLessonStatusMap();
            InitLessonStatusesCompleted();
        }

        public BaseLogic(Dictionary<string, string> json_values)
        {
            InitDelegates();
            InitLessonStatusMap();
            InitLessonStatusesCompleted();
            m_json_values = json_values;
        }

        private void InitDelegates()
        {
            m_delegates["cmi.core.credit"] = CreditDelegate;
            m_delegates["cmi.core.exit"] = ExitDelegate;
            m_delegates["cmi.core.entry"] = EntryDelegate;
            m_delegates["cmi.core.lesson_location"] = LessonLocationDelegate;
            m_delegates["cmi.core.lesson_mode"] = LessonModeDelegate;
            m_delegates["cmi.core.lesson_status"] = LessonStatusDelegate;
            m_delegates["cmi.student_data.mastery_score"] = MasteryScoreDelegate;
            m_delegates["cmi.core.score.raw"] = ScoreRawDelegate;
            m_delegates["cmi.core.score.min"] = ScoreMinDelegate;
            m_delegates["cmi.core.score.max"] = ScoreMaxDelegate;
            m_delegates["cmi.core.session_time"] = SessionTimeDelegate;
            m_delegates["cmi.suspend_data"] = SuspendDataDelegate;
            m_delegates["cmi.terminate"] = TerminateDelegate;
            m_delegates["cmi.comments"] = CommentsDelegate;

        }

        private void InitDbMap()
        {
            m_dbmap["cmi.core.lesson_location"] = "e_tb_enrollments.e_enroll_lesson_location";
            m_dbmap["cmi.core.credit"] = "e_tb_enrollments.e_enroll_credit";
            m_dbmap["cmi.core.lesson_mode"] = "e_tb_enrollments.e_enroll_lesson_mode";
            m_dbmap["cmi.core.lesson_status"] = "e_tb_enrollments.e_enroll_lesson_status";
            m_dbmap["cmi.core.score.raw"] = "e_tb_enrollments.e_enroll_score";
            m_dbmap["cmi.core.score.min"] = "e_tb_enrollments.e_enroll_score_min";
            m_dbmap["cmi.core.score.max"] = "e_tb_enrollments.e_enroll_score_max";
//            m_dbmap["cmi.core.session_time"] = "sent to API only.add value to e_enroll_time_spent";
            m_dbmap["cmi.core.total_time"] = "e_tb_enrollments.e_enroll_time_spent";
            m_dbmap["cmi.suspend_data "] = "e_tb_enrollments.e_enroll_suspend_data";
            m_dbmap["cmi.launch_data"] = "e_tb_enrollments.e_enroll_launch_data";
            m_dbmap["cmi.core.exit"] = "e_tb_enrollments.e_enroll_exit";
            m_dbmap["cmi.core.entry"] = "e_tb_enrollments.e_enroll_entry";
        }

        protected void InitLessonStatusMap()
        {
            m_map_lesson_status.Add("a", "attempted");
            m_map_lesson_status.Add("b", "browsed");
            m_map_lesson_status.Add("c", "completed");
            m_map_lesson_status.Add("f", "failed");
            m_map_lesson_status.Add("i", "incomplete");
            m_map_lesson_status.Add("p", "passed");
        }

        protected void InitLessonStatusesCompleted()
        {
            m_lesson_statuses_completed.Add("completed");
            m_lesson_statuses_completed.Add("passed");
            m_lesson_statuses_completed.Add("failed");
            m_lesson_statuses_completed.Add("browsed");
        }

        public void ConsumeJSObj(Dictionary<string, string> json_values)
        {
            Dictionary<string, string>.KeyCollection keys = json_values.Keys;
            foreach (string key in keys)
            {
                if (m_delegates.ContainsKey(key))
                {
                    FieldDelegate del = m_delegates[key];
                    del(key, json_values);
                }
            }
        }

        public void Persist(string enrollment_id)
        {
            // for now use a constant
            //enrollment_id = "";

            using (var context = new ComplianceFactorsEntities())
            {
                bool terminate = false;

                var enroll = (from e in context.e_tb_enrollments
                              where e.e_enroll_system_id_pk == new Guid(enrollment_id)
                              select e).FirstOrDefault();

                if (null == enroll) //not found
                {
                    throw new ApplicationException("No valid enrollment ID supplied.");
                }

                // loop through m_values and write to context; then save to EF/DB
                foreach (KeyValuePair<string, object> pair in m_values)
                {
                    switch (pair.Key)
                    {
                        case "cmi.core.lesson_status":
                            enroll.e_enroll_lesson_status = (string)pair.Value;
                            break;
                        case "cmi.core.credit":
                            enroll.e_enroll_credit = (bool)pair.Value;
                            break;
                        case "cmi.core.exit":
                            enroll.e_enroll_exit = (string)pair.Value;
                            break;
                        case "cmi.core.entry":
                            enroll.e_enroll_entry = (bool)pair.Value;
                            break;
                        case "cmi.core.lesson_location":
                            enroll.e_enroll_lesson_location = (string)pair.Value;
                            break;
                        case "cmi.core.lesson_mode":
                            enroll.e_enroll_lesson_mode = (string)pair.Value;
                            break;
                        case "cmi.student_data.mastery_score":
                            //enroll.e_tb_users_lesson_data.e_mastery_score = Convert.ToDecimal(pair.Value);
                            break;
                        case "cmi.core.score.raw":
                            enroll.e_enroll_score = Convert.ToDecimal(pair.Value);
                            break;
                        case "cmi.core.score.min":
                            enroll.e_enroll_score_min = Convert.ToDecimal(pair.Value);
                            break;
                        case "cmi.core.score.max":
                            enroll.e_enroll_score_max = Convert.ToDecimal(pair.Value);
                            break;
                        case "cmi.core.session_time":
                            // total_time += session_time (session_time will be zero until 'terminate' event)
                            enroll.e_enroll_time_spent += (int)pair.Value;
                            break;
                        case "cmi.suspend_data":
                            enroll.e_enroll_suspend_data = (string)pair.Value;
                            break;
                        case "cmi.comments":
                            enroll.e_enroll_student_comments = (string)pair.Value; //student comments come *from* the course to the LMS
                            break;
                        case "cmi.terminate":
                            terminate = (bool)pair.Value;//("true" == (string) pair.Value);
                            break;
                        default:
                            break;
                    }
                }
                context.SaveChanges();

                //Was LMSFinish / Terminate called?
                if (terminate && LessonCompleted(enroll.e_enroll_lesson_status))
                {
                    // do completion process; create transcript record; disable enrollment so as not to show in 'my courses'

                    enroll.e_enroll_completion_date = DateTime.Now;
                    enroll.e_enroll_active_flag = false;

                    t_tb_transcripts tx = t_tb_transcripts.Createt_tb_transcripts(Guid.NewGuid(), 
                                        t_transcript_user_id_fk: enroll.e_enroll_user_id_fk, 
                                        t_transcript_course_id_fk: enroll.e_enroll_course_id_fk, 
                                        t_transcript_delivery_id_fk: enroll.e_enroll_delivery_id_fk, 
                                        t_transcript_attendance_id_fk: "cd8a0438-0631-4996-8bc0-5b9609e70cb6",//"OLT Player",
                                        t_transcript_passing_status_id_fk: enroll.e_enroll_lesson_status, 
                                        t_transcript_completion_date_time: DateTime.Now, 
                                        t_transcript_completion_type_id_fk: "cd8a0438-0631-4996-8bc0-5b9609e70cb6",//"OLT Player",
                                        t_transcript_marked_by_user_id_fk: new Guid(),// all zeroes
                                        t_transcript_actual_date: DateTime.Now

                        );
                    tx.t_transcript_target_due_date = enroll.e_enroll_target_due_date;
                    tx.t_transcript_status_id_fk = enroll.e_enroll_status_id_fk;
                    tx.t_transcript_score = enroll.e_enroll_score;

                    //TODO: score min/max must be added to transcripts table

                    tx.t_transcript_credits = enroll.c_tb_deliveries_master.c_delivery_credit_units;

                    tx.t_transcript_hours = enroll.c_tb_deliveries_master.c_delivery_credit_hours;
                    tx.t_transcript_time_spent = enroll.e_enroll_time_spent;
                    tx.t_transcript_completion_score = CalcCompletionScore(enrollment_id);

                    tx.t_transcript_active_flag = true;

                    context.SaveChanges();
                }
            }
        }

        private string CalcCompletionScore(string eid)
        {
            string score;

            /*
             * TODO: Implement!
             * 
             * Lookup the Grading Scheme equivalent value by ID [s_tb_grading_schemes_values] (s_grading_scheme_system_id_fk) 
             *      WHERE {Score} IS GREATER OR EQUAL THAN (s_grading_scheme_value_min_score) 
             *          AND LESS THAN OR EQUAL (s_grading_scheme_value_max_score) and take the value in (s_grading_scheme_value_grade) 
             *          and (s_grading_scheme_value_pass_status_id_fk) to store in the Transcript table.
             *        
             *      [c_tb_deliveries_master].c_delivery_system_id_pk  ==    [e_tb_enrollments].e_enroll_delivery_id_fk
                    [s_tb_grading_schemes].s_grading_scheme_system_id_pk == [c_tb_deliveries_master].c_delivery_grading_scheme_id_fk
             * 
             */
        
            using (ComplianceFactorsEntities ctx = new ComplianceFactorsEntities())
            {
                var enroll = (from e in ctx.e_tb_enrollments
                              where e.e_enroll_system_id_pk == new Guid(eid)
                              select e).FirstOrDefault();
                
                //default value
                score = Convert.ToString(enroll.e_enroll_score);

                if (null != enroll.c_tb_deliveries_master.s_tb_grading_schemes.s_grading_scheme_system_id_pk)
                {
                    // TODO: check for correct logic to determine whether grading scheme exists;
                                //  if exits, follow logic above; else, return as a string the original enroll_score value
                }
            }

            return score;
        }

        #region Delegates
        private void CommentsDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetComments(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void CreditDelegate(string key, Dictionary<string, string> json_values)
        {
            bool? tmp = GetCredit(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ExitDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetExit(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void EntryDelegate(string key, Dictionary<string, string> json_values)
        {
            bool tmp = GetEntry(json_values);
            //if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void LessonLocationDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetLessonLocation(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void LessonModeDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetLessonMode(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void LessonStatusDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetLessonStatus(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void MasteryScoreDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetMasteryScore(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ScoreRawDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetScoreRaw(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ScoreMinDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetScoreMin(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ScoreMaxDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetScoreMax(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void SessionTimeDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetSessionTime(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void SuspendDataDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetSuspendData(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void TerminateDelegate(string key, Dictionary<string, string> json_values)
        {
            bool tmp = GetTerminate(json_values);
            this.m_values[key] = tmp;
        }

        #endregion

        #region Getters

        private string GetComments(Dictionary<string, string> json_values)
        {
            string comments;
            json_values.TryGetValue("cmi.comments", out comments);

            return comments;
        }

        private bool? GetCredit(Dictionary<string, string> json_values)
        {   // CMI determines whether taken for credit or no-credit.
            string tmp;
            bool? credit;
            json_values.TryGetValue("cmi.core.credit", out tmp);

            string lesson_mode = GetLessonMode(json_values);

            if (tmp != null)
            {
                // only first char significant ('n','c')
                tmp = tmp.Substring(0, 1);

                if ("n" == tmp)
                    credit = false; //"no-credit";
                else
                    credit = true; //"credit";
            }
            else
            {
                // If unrecognized or no value received, 'credit' assumed.
                credit = true; //"credit";
            }

            if ("browsed" == lesson_mode)
            {
                credit = false; // "no-credit";
            }

            return credit;
        }

        /// <summary>
        /// For testing.
        /// </summary>
        /// <param name="key">key into m_values</param>
        /// <returns>the value from m_values</returns>
        public object GetValue(string key)
        {
            return m_values[key];
        }

        private double? GetDouble(string key, Dictionary<string, string> json_values)
        {
            double? ret = null;
            string tmp = null;
            json_values.TryGetValue(key, out tmp);
            if (tmp != null)
            {
                double d;
                bool res = Double.TryParse(tmp, out d);
                if (res)
                    ret = d;
            }

            return ret;
        }

        private bool GetEntry(Dictionary<string, string> json_values)
        {
            
            // a, r => core.entry  {ab-initio,resume,""}

            // If received from the course (in HACP, PutParam), then second value is core.exit
            // It is never received as core.entry---this is only an outgoing value to the course via GetParam
            // In the JS API, the values have distinct keys.
            
            // Output only (LMS -> course)
            // {"ab-initio", "resume", ""}
            // Only first character significant

            string entry;
            json_values.TryGetValue("cmi.core.entry", out entry);

            return (entry == "1");
        }

        private string GetExit(Dictionary<string, string> json_values)
        {
            // l, t, s => core.exit {logout,time-out,suspend, ""} 
            Dictionary<string, string> valid_exits = new Dictionary<string, string>
                {
                    {"l", "logout"},
                    {"t", "time-out"},
                    {"s", "suspend"}
                };
            
            string exit = null;
            json_values.TryGetValue("cmi.core.exit", out exit);

            if (null != exit)
            {
                exit = exit.Substring(0, 1).ToLower();
                if (valid_exits.ContainsKey(exit))
                    exit = valid_exits[exit];
                else
                    exit = "";
            }
            else
                exit = "";

            return exit;
        }

        private string GetLessonLocation(Dictionary<string, string> json_values)
        { // CMI must only get/set this value
            string lesson_location;
            json_values.TryGetValue("cmi.core.lesson_location", out lesson_location);

            return lesson_location;
        }

        private string GetLessonMode(Dictionary<string, string> json_values)
        {
            // {"browse","normal","review"}; only first character significant
            // Output only (LMS -> course)
            
            string lesson_mode;
            json_values.TryGetValue("cmi.core.lesson_mode", out lesson_mode);

            return lesson_mode;
        }

        private string GetLessonStatus(Dictionary<string, string> json_values)
        {
            string ret = null;
            string lesson_status = null;
            json_values.TryGetValue("cmi.core.lesson_status", out lesson_status);
            lesson_status = m_map_lesson_status[lesson_status.Substring(0,1)];

            string lesson_mode = GetLessonMode(json_values);
            bool? credit = GetCredit(json_values);
            double? mastery_score = GetMasteryScore(json_values);
            double? score_raw = GetScoreRaw(json_values);

            if (lesson_status != null)
            {
                if (lesson_mode == "normal")
                {
                    if (lesson_status == "incomplete")
                    {
                        ret = "incomplete";
                    }
                    else
                    {
                        if (credit ?? true)
                        {
                            if (mastery_score != null && score_raw != null)
                            {
                                if (score_raw >= mastery_score)
                                    ret = "passed";
                                else
                                    ret = "failed";
                            }
                        }
                    }
                }
                else if (lesson_mode == "browse")
                {
                    if (lesson_status == "not attempted")
                        ret = "browsed";
                }
            }

            return ret;
        }

        private double? GetMasteryScore(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.student_data.mastery_score", json_values);
        }

        private double? GetScoreRaw(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.core.score.raw", json_values);
        }

        private double? GetScoreMax(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.core.score.max", json_values);
        }

        private double? GetScoreMin(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.core.score.min", json_values);
        }

        /// <summary>
        /// GetSessionTime returns the current session time in seconds, but
        /// only when the session is terminated. Otherwise, it returns zero
        /// seconds--allowing the value to be added to total_time without
        /// affecting the sum unless and until terminate is called.
        /// </summary>
        /// <param name="json_values"></param>
        /// <returns></returns>
        private int? GetSessionTime(Dictionary<string, string> json_values)
        {
            int? ret;

            if (GetTerminate(json_values))
                ret = GetTime("cmi.core.session_time", json_values);
            else
                ret = 0;

            return ret;
        }

        private string GetStudentId(Dictionary<string, string> json_values)
        {
            string student_id;
            json_values.TryGetValue("cmi.core.student_id", out student_id);

            return student_id;
        }

        private string GetStudentName(Dictionary<string, string> json_values)
        {
            string student_name;
            json_values.TryGetValue("cmi.core.student_name", out student_name);

            return student_name;
        }

        private string GetSuspendData(Dictionary<string, string> json_values)
        {
            string suspend_data;
            json_values.TryGetValue("cmi.suspend_data", out suspend_data);

            return suspend_data;
        }

        private bool GetTerminate(Dictionary<string, string> json_values)
        {
            bool ret = false;
            string terminate;
            json_values.TryGetValue("cmi.terminate", out terminate);

            if (null != terminate && "true" == terminate)
                ret = true;

            return ret;
        }

        private int? GetTime(string key, Dictionary<string, string> json_values)
        {
            int? ret = null;
            string tmp = null;
            json_values.TryGetValue(key, out tmp);
            if (tmp != null)
            {
                TimeSpan t;
                bool res = TimeSpan.TryParse(tmp, out t);
                if (res)
                    ret = (int)t.TotalSeconds;
            }

            return ret;
        }

        #endregion

        #region Utility Methods

        private bool LessonCompleted(string status)
        {
            return m_lesson_statuses_completed.Contains(status);
        }

        #endregion
    }

}
