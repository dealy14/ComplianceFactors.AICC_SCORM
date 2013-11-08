using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
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

        private const string OLT_Player = "cd8a0438-0631-4996-8bc0-5b9609e70cb6"; // GUID for OLT Player as 'actor'

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

        #region Initialization
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

            m_delegates["cmi.launch_data"] = LaunchDataDelegate;
            m_delegates["cmi.evaluation.comments.n.content"] = EvaluationCommentsNContentDelegate;
            m_delegates["cmi.evaluation.comments.n.location"] = EvaluationCommentsNLocationDelegate;
            m_delegates["cmi.evaluation.comments.n.time"] = EvaluationCommentsNTimeDelegate;
            m_delegates["cmi.comments_from_lms"] = CommentsFromLmsDelegate;
            m_delegates["cmi.objectives.n.id"] = ObjectivesNIdDelegate;
            m_delegates["cmi.objectives.n.score.raw"] = ObjectivesNScoreRawDelegate;
            m_delegates["cmi.objectives.n.score.min"] = ObjectivesNScoreMinDelegate;
            m_delegates["cmi.objectives.n.score.max"] = ObjectivesNScoreMaxDelegate;
            m_delegates["cmi.objectives.n.status"] = ObjectivesNStatusDelegate;
            m_delegates["cmi.student_data.attempt_number"] = StudentDataAttemptNumberDelegate;
            m_delegates["cmi.student_data.tries.n.score.raw"] = StudentDataTriesNScoreRawDelegate;
            m_delegates["cmi.student_data.tries.n.score.min"] = StudentDataTriesNScoreMinDelegate;
            m_delegates["cmi.student_data.tries.n.score.max"] = StudentDataTriesNScoreMaxDelegate;
            m_delegates["cmi.student_data.tries.n.status"] = StudentDataTriesNStatusDelegate;
            m_delegates["cmi.student_data.tries.n.time"] = StudentDataTriesNTimeDelegate;
            m_delegates["cmi.student_data.mastery_score"] = StudentDataMasteryScoreDelegate;
            m_delegates["cmi.student_data.max_time_allowed"] = StudentDataMaxTimeAllowedDelegate;
            m_delegates["cmi.student_data.time_limit_action"] = StudentDataTimeLimitActionDelegate;
            m_delegates["cmi.student_data.tries_during_lesson"] = StudentDataTriesDuringLessonDelegate;
            m_delegates["cmi.student_preference.audio"] = StudentPreferenceAudioDelegate;
            m_delegates["cmi.student_preference.language"] = StudentPreferenceLanguageDelegate;
            m_delegates["cmi.student_preference.lesson_type"] = StudentPreferenceLessonTypeDelegate;
            m_delegates["cmi.student_preference.speed"] = StudentPreferenceSpeedDelegate;
            m_delegates["cmi.student_preference.text"] = StudentPreferenceTextDelegate;
            m_delegates["cmi.student_preference.text_color"] = StudentPreferenceTextColorDelegate;
            m_delegates["cmi.student_preference.text_location"] = StudentPreferenceTextLocationDelegate;
            m_delegates["cmi.student_preference.text_size"] = StudentPreferenceTextSizeDelegate;
            m_delegates["cmi.student_preference.video"] = StudentPreferenceVideoDelegate;
            m_delegates["cmi.student_preference.windows.1"] = StudentPreferenceWindows1Delegate;
            m_delegates["cmi.interactions.n.id"] = InteractionsNIdDelegate;
            m_delegates["cmi.interactions.n.objectives.n.id"] = InteractionsNObjectivesNIdDelegate;
            m_delegates["cmi.interactions.n.time"] = InteractionsNTimeDelegate;
            m_delegates["cmi.interactions.n.type"] = InteractionsNTypeDelegate;
            m_delegates["cmi.interactions.n.correct_responses.n.pattern"] = InteractionsNCorrectResponsesNPatternDelegate;
            m_delegates["cmi.interactions.n.weighting"] = InteractionsNWeightingDelegate;
            m_delegates["cmi.interactions.n.student_response"] = InteractionsNStudentResponseDelegate;
            m_delegates["cmi.interactions.n.result"] = InteractionsNResultDelegate;
            m_delegates["cmi.interactions.n.latency"] = InteractionsNLatencyDelegate;
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
            m_dbmap["cmi.comments"] = "e_tb_enrollments.e_enroll_instructor_comments";

            // optional elements
            m_dbmap["cmi.student_data.attempt_number"] = "e_tb_users_lesson_data.e_attempt_number";
            m_dbmap["cmi.student_data.lesson_status.n"] = "e_tb_users_lesson_data.e_lesson_status";
            m_dbmap["cmi.student_data.score.n"] = "e_tb_users_lesson_data.e_score";
            m_dbmap["cmi.student_data.mastery_score"] = "e_tb_users_lesson_data.e_mastery_score";
            m_dbmap["cmi.student_data.max_time_allowed"] = "e_tb_users_lesson_data.e_max_time_allowed";
            m_dbmap["cmi.student_data.time_limit_action"] = "e_tb_users_lesson_data.e_time_limit_action";
            m_dbmap["cmi.student_data.tries_during_lesson"] = "e_tb_users_lesson_data.e_tries_during_lesson";

            m_dbmap["cmi.student_data.tries.n.score"] = "e_tb_users_lesson_data_tries.e_try_score";
            m_dbmap["cmi.student_data.tries.n.score.raw"] = "e_tb_users_lesson_data_tries.e_try_score_raw";
            m_dbmap["cmi.student_data.tries.n.score.max"] = "e_tb_users_lesson_data_tries.e_try_score_max";
            m_dbmap["cmi.student_data.tries.n.score.min"] = "e_tb_users_lesson_data_tries.e_try_score_min";
            m_dbmap["cmi.student_data.tries.n.status"] = "e_tb_users_lesson_data_tries.e_try_status";
            m_dbmap["cmi.student_data.tries.n.time"] = "e_tb_users_lesson_data_tries.e_try_time";

            m_dbmap["cmi.objectives.n.id"] = "e_tb_objectives.e_objective_id_pk";

            m_dbmap["cmi.objectives.n.score.raw"] = "e_tb_objectives.e_objective_score";
            m_dbmap["cmi.objectives.n.score.min"] = "e_tb_objectives.e_objective_score_min";
            m_dbmap["cmi.objectives.n.score.max"] = "e_tb_objectives.e_objective_score_max";
            m_dbmap["cmi.objectives.n.status"] = "e_tb_objectives.e_objective_status";

            m_dbmap["cmi.evaluation.comments.n.content"] = "e_tb_comments.e_comment_content";
            m_dbmap["cmi.evaluation.comments.n.time"] = "e_tb_comments.e_comment_time";
            m_dbmap["cmi.evaluation.comments.n.location"] = "e_tb_comments.e_comment_location";

            m_dbmap["cmi.interactions.n.date"] = "e_tb_interactions.e_interactions_date";
            m_dbmap["cmi.interactions.n.time"] = "e_tb_interactions.e_interactions_time";
            m_dbmap["cmi.interactions.n.id"] = "e_tb_interactions.e_interactions_id";
            m_dbmap["cmi.interactions.n.objective.n.id"] = "e_tb_interactions.e_interactions_objective_id";
            m_dbmap["cmi.interactions.n.type"] = "e_tb_interactions.e_interactions_type";
            m_dbmap["cmi.interactions.n.correct_response.n.pattern"] = "e_tb_interactions_correct_responses.e_interactions_correct_response_pattern";
            m_dbmap["cmi.interactions.n.student_response"] = "e_tb_interactions.e_interactions_student_response";
            m_dbmap["cmi.interactions.n.result"] = "e_tb_interactions.e_interactions_result";
            m_dbmap["cmi.interactions.n.weighting"] = "e_tb_interactions.e_interactions_weighting";
            m_dbmap["cmi.interactions.n.latency"] = "e_tb_interactions.e_interactions_latency";

            m_dbmap["cmi.student_preference.audio"] = "u_tb_users_lesson_prefs.u_audio_pref";
            m_dbmap["cmi.student_preference.language"] = "u_tb_users_lesson_prefs.u_lang_pref";
            m_dbmap["cmi.student_preference.lesson_type"] = "u_tb_users_lesson_prefs.u_lesson_type_pref";
            m_dbmap["cmi.student_preference.speed"] = "u_tb_users_lesson_prefs.u_speed_pref";
            m_dbmap["cmi.student_preference.text"] = "u_tb_users_lesson_prefs.u_text_pref";
            m_dbmap["cmi.student_preference.text_color"] = "u_tb_users_lesson_prefs.u_text_color_pref";
            m_dbmap["cmi.student_preference.text_location"] = "u_tb_users_lesson_prefs.u_text_location_pref";
            m_dbmap["cmi.student_preference.text_size"] = "u_tb_users_lesson_prefs.u_text_size_pref";
            m_dbmap["cmi.student_preference.video"] = "u_tb_users_lesson_prefs.u_video_pref";
            m_dbmap["Enrollment ID related separate Table to be added"] = "u_tb_users_lesson_prefs_windows.u_window_pref";

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
        #endregion

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

        #region Persistence and Completion Logic

        public void Persist(string enrollment_id)
        {
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
                            if ((string)pair.Value == "incomplete"){
                                enroll.e_enroll_status_id_fk = Guid.Parse("655f1d57-1a6e-498f-b341-33c8c04ab430"); // Incomplete
                            }
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

                // COMPLETION PROCESS
                // Was LMSFinish / Terminate called?
                if (terminate && LessonCompleted(enroll.e_enroll_lesson_status))
                {
                    // Do completion process
                    // 1. create transcript record
                    // 2. disable enrollment so as not to show in 'my courses'
                    // 3. insert audit log record
                    // 4. handle possible recurrence
                    // 5. handle possible curriculum update

                    enroll.e_enroll_completion_date = DateTime.Now;
                    enroll.e_enroll_active_flag = false;

                    string pass_status_fk;
                    Guid tx_status_id_fk;
                    Guid? tx_grade_id_fk = null;
                    
                    var tx_compl_score = CalcCompletionScore(enroll.e_enroll_score,
                        enroll.c_tb_deliveries_master.c_delivery_grading_scheme_id_fk, out pass_status_fk);
                    
                    switch (pass_status_fk)
                    {
                        case "":
                            tx_status_id_fk = enroll.e_enroll_status_id_fk;
                            break;
                        default:
                            tx_status_id_fk = Guid.Parse(pass_status_fk);
                            tx_grade_id_fk = enroll.c_tb_deliveries_master.c_delivery_grading_scheme_id_fk;
                            break;
                    }

                    var tx = new t_tb_transcripts
                    {
                        t_transcript_id_pk = Guid.NewGuid(),
                        t_transcript_user_id_fk = enroll.e_enroll_user_id_fk, 
                        t_transcript_course_id_fk = enroll.e_enroll_course_id_fk, 
                        t_transcript_delivery_id_fk = enroll.e_enroll_delivery_id_fk, 
                        t_transcript_attendance_id_fk = OLT_Player,
                        t_transcript_passing_status_id_fk = enroll.e_enroll_lesson_status, 
                        t_transcript_completion_date_time = DateTime.Now, 
                        t_transcript_completion_type_id_fk = OLT_Player,
                        t_transcript_marked_by_user_id_fk = new Guid(),// all zeroes
                        t_transcript_actual_date = DateTime.Now,
                        t_transcript_target_due_date = enroll.e_enroll_target_due_date,
                        t_transcript_score = enroll.e_enroll_score,
                        t_transcript_credits = enroll.c_tb_deliveries_master.c_delivery_credit_units,
                        t_transcript_hours = enroll.c_tb_deliveries_master.c_delivery_credit_hours,
                        t_transcript_time_spent = enroll.e_enroll_time_spent,
                        t_transcript_completion_score = tx_compl_score,
                        t_transcript_status_id_fk = tx_status_id_fk,
                        t_transcript_active_flag = true,
                        t_transcript_grade_id_fk = tx_grade_id_fk
                    };
                    context.t_tb_transcripts.AddObject(tx);
                    context.SaveChanges();

                    //TODO: score min/max must be added to transcripts table ???? maybe not... (not in player logic document)
                    

                    // A. Insert completion record in audit log
                    insertAuditRecord(enroll.u_tb_users_master.u_user_id_pk, "Marked Completion / Type (OLT Player)", 
                                "(Completed, Attendance='OLT PLayer', Passing Status=" + pass_status_fk 
                                    + ", Completion Score=" + enroll.e_enroll_score.ToString(), Guid.Parse(enrollment_id), 
                                    "e_tb_enrollments", null);

                    var course = enroll.c_tb_courses_master;

                    // B. Check for course recurrence
                    if (course.c_course_recurrence_every != null && course.c_course_recurrence_every != 0)
                    {
                        // Course has recurrence, so calc next due date and create new enrollment record
                        
                        
                        DateTime new_date = CalcRecurrenceDate(course.c_course_recurrence_date_option,
                                                      (DateTime)course.c_course_recurrence_date,
                                                      (DateTime)enroll.u_tb_users_master.u_hris_hire_date,
                                                      (DateTime)enroll.e_enroll_approval_date,
                            // TODO: Should be <e_tb_courses_assign.e_course_assign_date_time>, but no obvious way to get assignment record
                                                      (int)course.c_course_recurrence_every,
                                                      course.c_course_recurrence_period
                            );
                        // new_date = [X] [days/months/years] from [start_date]
                        
                        // Create new enrollment record
                        insertEnrollmentRecord(enroll.u_tb_users_master.u_user_id_pk, course.c_course_system_id_pk,
                                                enroll.e_enroll_delivery_id_fk, new_date, (bool)enroll.e_enroll_required_flag);
                    }

                    // C.  Curriculum update and recurrence logic (e_tb_curricula_assign)

                    updateAssignedCurricula(user_id: enroll.e_enroll_user_id_fk, course_id: enroll.e_enroll_course_id_fk,
                                            delivery_id: enroll.e_enroll_delivery_id_fk);

                }
            }
        }
        
        /*
           So if it is in 1 or more curricula assign to the user, the Status of the curriculum or curricula needs to be updated 
         * and the course reassigned with the new due date(s) if it is recurring.
         * 
         *      % complete: e_tb_curricula_assign.e_curriculum_assign_percent_complete
         *      target due date: e_tb_curricula_assign.e_curriculum_assign_target_due_date
         */
        
        // Determine whether curriculum is assigned to user/course; if so, update accordingly
        private void updateAssignedCurricula(Guid user_id, Guid course_id, Guid delivery_id)
        {
            Guid curriculum_status_assigned = new Guid("77dc2499-2fef-4952-97f2-d6f04fa001e2");
            Guid curriculum_status_inprogress = new Guid("38d9f4e2-7f7a-4130-8fff-0e26cc892109");
            Guid curriculum_status_acquired = new Guid("b0c6cd90-7fe3-4af3-80df-2040c09e5b05");
   
            //course_id = Guid.Parse("d18e9bc8-4cca-4770-8bb7-010504e341d5");
            //user_id = Guid.Parse("48433026-7d99-4cc1-ade7-09b23b1bc5ef");

            using (var ctx = new ComplianceFactorsEntities())
            {
                //  Does course and user have curriculum assigned?
                const string query_string = @"select VALUE e_tb_curricula_assign FROM e_tb_curricula_assign 
                                                where e_tb_curricula_assign.e_curriculum_assign_curriculum_id_fk in 
                                                set(select VALUE (c_tb_curriculum_path_courses.c_curricula_id_fk) from c_tb_curriculum_path_courses 
                                                    where c_tb_curriculum_path_courses.c_curricula_path_course_id_fk = @courseid) 
                                                and e_tb_curricula_assign.e_curriculum_assign_user_id_fk = @userid";

                ObjectQuery<e_tb_curricula_assign> assignedCurricula =
                    ctx.CreateQuery<e_tb_curricula_assign>(query_string, new ObjectParameter[]
                        {
                            new ObjectParameter("courseid", course_id),
                            new ObjectParameter("userid", user_id)
                        });
                
                // Iterate through matching curricula 
                foreach (e_tb_curricula_assign curriculum in assignedCurricula.ToList())
                {
                    // If progress == 0%, then change e_curricula_assign_status_id_fk from 'assigned' to 'in progress'
                    if (0 == curriculum.e_curriculum_assign_percent_complete)
                    {
                        curriculum.e_curriculum_assign_status_id_fk = curriculum_status_inprogress;
                        curriculum.e_curriculum_assign_status_change_date = DateTime.Now;    
                    }

                    Guid? current_status = curriculum.e_curriculum_assign_status_id_fk;
                    Guid curriculum_id = curriculum.e_curriculum_assign_curriculum_id_fk;

                    // Determine number of courses for this curriculum path
                    int num_courses_in_curriculum = (from courses in ctx.c_tb_curriculum_path_courses
                                                     where
                                                         courses.c_curricula_id_fk == curriculum_id
                                                     select courses).Count();
                    // Extrapolate # courses completed based on current % completed
                    double courses_completed = Math.Round(((double)curriculum.e_curriculum_assign_percent_complete * 0.01) 
                                                    * num_courses_in_curriculum);
                    // Add 1 to the number of courses completed
                    int percent_complete = (int) ((courses_completed + 1)
                                                  /(double) num_courses_in_curriculum*100);

                    curriculum.e_curriculum_assign_percent_complete = percent_complete;

                    // Is curriculum complete? If yes => Curriculum 'Acquired'
                    if (100 == percent_complete)
                    {
                        curriculum.e_curriculum_assign_status_id_fk = curriculum_status_acquired;
                        curriculum.e_curriculum_assign_status_change_date = DateTime.Now;

                        insertCurriculumStatusHistoryRecord(user_id: user_id,
                                                            curriculum_id: curriculum_id,
                                                            original_assignment_date:
                                                                curriculum.e_curriculum_assign_date_time,
                                                            required: curriculum.e_curriculum_assign_required_flag,
                                                            original_target_due_date:
                                                                curriculum.e_curriculum_assign_target_due_date,
                                                            new_status: curriculum_status_acquired,
                                                            previous_status: current_status,
                                                            percent_complete: percent_complete
                            );
                        ctx.SaveChanges();

                        // TODO: Generate and send 'Curriculum Acquired' notification

                        // Check for curriculum recurrence -- but only if there was no course-specific recurrence
                        if (!courseHasRecurrence(course_id) &&
                            curriculumHasRecurrence(curriculum_id:
                                                        curriculum.e_curriculum_assign_curriculum_id_fk))
                        {
                            var curr = (from c in ctx.c_tb_curriculum_master
                                        where c.c_curriculum_system_id_pk == curriculum_id
                                        select c).FirstOrDefault();
                            DateTime hire_date = getHireDate(user_id);

                            // Re-assign the course to employee using curriculum recurrence criteria
                            DateTime new_date = CalcRecurrenceDate(curr.c_curriculum_recurance_date_option,
                                                          (DateTime)curr.c_curriculum_recurance_date,
                                                          (DateTime)hire_date,
                                                          (DateTime)curriculum.e_curriculum_assign_date_time,
                                                          (int)curr.c_curriculum_recurrance_every,
                                                          curr.c_curriculum_recurrance_period
                                );

                            // Create new enrollment record
                            insertEnrollmentRecord(curriculum.e_curriculum_assign_user_id_fk, course_id,
                                                   delivery_id, new_date,
                                                   curriculum.e_curriculum_assign_required_flag != null &&
                                                   (bool) curriculum.e_curriculum_assign_required_flag);
                        }
                    }
                    ctx.SaveChanges();    
                }
            }
        }

        private Guid insertCurriculumStatusHistoryRecord(Guid user_id, Guid curriculum_id, DateTime original_assignment_date, bool? required, DateTime? original_target_due_date, Guid new_status, Guid? previous_status, int percent_complete)
        {
            Guid player_status_change_type_id = new Guid("3083c38c-a8be-4e08-abe3-e768ffa3d6a1");
            Guid new_pk_id = Guid.NewGuid();

            using (var ctx = new ComplianceFactorsEntities())
            {
                var rec = new e_tb_curricula_statuses_history
                {
                    e_curriculum_assign_system_id_pk = new_pk_id,
                    e_curriculum_assign_user_id_fk = user_id,
                    e_curriculum_assign_curriculum_id_fk = curriculum_id,
                    e_curriculum_assign_date_time = original_assignment_date,
                    e_curriculum_assign_original_target_due_date = original_target_due_date,
                    e_curriculum_assign_after_status_id_fk = new_status,
                    e_curriculum_assign_recert_status_change_type_id_fk = player_status_change_type_id,
                    e_curriculum_before_status_id_fk = previous_status,
                    e_curriculum_assign_status_change_date_time = DateTime.Now,
                    e_curriculum_assign_percent_complete = percent_complete,
                    e_curriculum_assign_required_flag = required,
                    //e_curriculum_assign_cert_date = DateTime.Now,       // TODO: calculate new date
                    //e_curriculum_assign_recert_due_date = DateTime.Now  // TODO: calculate new date
                };
                ctx.e_tb_curricula_statuses_history.AddObject(rec);
                ctx.SaveChanges();
            }
            return new_pk_id;
        }

        private DateTime CalcRecurrenceDate(string recurrence_date_option, DateTime recurrence_date, 
                                            DateTime hire_date, DateTime assign_date, int recurrence_every, 
                                            string recurrence_period)
        {
            DateTime start_date;

            // determine start date to use in calc
            switch (recurrence_date_option)
            {
                case "fixed":
                    start_date = recurrence_date;
                    break;
                case "hire":
                    start_date = hire_date;
                    break;
                case "assignment":
                    start_date = assign_date;
                    break;
                case "completion":
                    start_date = DateTime.Now;
                    break;
                default:
                    start_date = DateTime.Now; // default to completion date
                    break;
            }

            int units = recurrence_every; // number of units
            DateTime new_date;

            // TimeDate units
            switch (recurrence_period)
            {
                case "days":
                    new_date = start_date.AddDays(units);
                    break;
                case "months":
                    new_date = start_date.AddMonths(units);
                    break;
                case "years":
                    new_date = start_date.AddYears(units);
                    break;
                default:
                    new_date = start_date.AddDays(units);  //default to Days
                    break;
            }
            
            return new_date;
        }
        
        private string CalcCompletionScore(decimal? raw_score, Guid? grading_scheme_id, out string pass_status_fk)
        {
            // default value
            string score = raw_score.ToString();
            pass_status_fk = "";

            if (null == grading_scheme_id)
            {
                return score;
            }

            /*
             * Notes
             *      [c_tb_deliveries_master].c_delivery_system_id_pk  ==    [e_tb_enrollments].e_enroll_delivery_id_fk
                    [s_tb_grading_schemes].s_grading_scheme_system_id_pk == [c_tb_deliveries_master].c_delivery_grading_scheme_id_fk
             *  t_transcript_grade_id_fk
             */

            using (var ctx = new ComplianceFactorsEntities())
            {
                var gs = (ctx.s_tb_grading_schemes_values.Where(
                    g => g.s_grading_scheme_system_id_fk == grading_scheme_id
                         &&
                         (raw_score >= g.s_grading_scheme_value_min_score &&
                          raw_score <= g.s_grading_scheme_value_max_score))).FirstOrDefault();
                
                if (null != gs)
                {
                    score = gs.s_grading_scheme_value_grade;
                    pass_status_fk = gs.s_grading_scheme_value_pass_status_id_fk;
                }
            }

            return score;
        }
         
        // Determines whether course has a recurrence
        private bool courseHasRecurrence(Guid course_id)
        {
            bool ret = false;

            using (var ctx = new ComplianceFactorsEntities())
            {
                var r = ctx.c_tb_courses_master.FirstOrDefault(i => i.c_course_system_id_pk == course_id);

                if (r.c_course_recurrence_every != null && r.c_course_recurrence_every != 0)
                {
                    ret = true;
                }
            }
            
            return ret;
        }

        // Determines whether curriculum has a recurrence
        private bool curriculumHasRecurrence(Guid curriculum_id)
        {
            bool ret = false;

            using (var ctx = new ComplianceFactorsEntities())
            {
                var r = ctx.c_tb_curriculum_master.FirstOrDefault(i => i.c_curriculum_system_id_pk == curriculum_id);

                if (r.c_curriculum_recurrance_every != null && r.c_curriculum_recurrance_every != 0)
                {
                    ret = true;
                }
            }

            return ret;
        }

        private DateTime getHireDate(Guid user_id)
        {
            DateTime hire_date = new DateTime();

            using (var ctx = new ComplianceFactorsEntities())
            {
                var r = ctx.u_tb_users_master.FirstOrDefault(i => i.u_user_id_pk == user_id);

                if (r != null)
                {
                    if (r.u_hris_hire_date != null)
                        hire_date = (DateTime) r.u_hris_hire_date;
                }
            }
            return hire_date;
        } 

        private Guid insertAuditRecord(Guid user_id, string action_description, string values, Guid affected_object_id, 
                                        string affected_object_table, string ip_address)
        {
            Guid new_id = Guid.NewGuid();

            using (var ctx = new ComplianceFactorsEntities())
            {
                var rec = new a_tb_audit_log
                    {
                        GUID = new_id,
                        a_audit_log_id_pk = new_id,
                        a_user_id_fk = user_id,
                        a_user_details = user_id.ToString(),
                        a_action_desc = action_description,
                        a_values = values,
                        a_ip_address = ip_address,
                        a_date_time = DateTime.Now,
                        a_affected_object_id_fk = affected_object_id,
                        a_affected_object_table_name = affected_object_table
                    };
                ctx.a_tb_audit_log.AddObject(rec);
                ctx.SaveChanges();
                new_id = rec.a_audit_log_id_pk;
            }

            return new_id;
        }

        private Guid insertEnrollmentRecord(Guid user_id, Guid course_id, Guid delivery_id, DateTime new_target_due_date, bool required)
        {
            Guid system_completion_assign_id = new Guid("0481a04e-ec8a-410a-bd4e-ef415d884857");
            Guid enrolled_status_id = new Guid("8d8adfbc-6e24-4081-8e3e-f7675d1dcecc");
            Guid new_enrollment_id = Guid.NewGuid();

            using (var ctx = new ComplianceFactorsEntities())
            {
                var rec = new e_tb_enrollments
                {
                    e_enroll_system_id_pk = new_enrollment_id,
                    e_enroll_user_id_fk = user_id,
                    e_enroll_course_id_fk = course_id,
                    e_enroll_delivery_id_fk = delivery_id,
                    e_enroll_enroll_date_time = DateTime.Now,
                    e_enroll_enroll_type_id_fk = system_completion_assign_id,
                    e_enroll_required_flag = required,
                    e_enroll_target_due_date = new_target_due_date,
                    e_enroll_status_id_fk = enrolled_status_id,
                    e_enroll_active_flag = true
                };
                ctx.e_tb_enrollments.AddObject(rec);
                ctx.SaveChanges();
            }
            return new_enrollment_id;
        }

        #endregion

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

        private void LaunchDataDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetLaunchData(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void EvaluationCommentsNContentDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetEvaluationCommentsNContent(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void EvaluationCommentsNLocationDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetEvaluationCommentsNLocation(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void EvaluationCommentsNTimeDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetEvaluationCommentsNTime(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void CommentsFromLmsDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetCommentsFromLms(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ObjectivesNIdDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetObjectivesNId(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ObjectivesNScoreRawDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetObjectivesNScoreRaw(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ObjectivesNScoreMinDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetObjectivesNScoreMin(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ObjectivesNScoreMaxDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetObjectivesNScoreMax(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void ObjectivesNStatusDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetObjectivesNStatus(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataAttemptNumberDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentDataAttemptNumber(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataTriesNScoreRawDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetStudentDataTriesNScoreRaw(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataTriesNScoreMinDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetStudentDataTriesNScoreMin(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataTriesNScoreMaxDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetStudentDataTriesNScoreMax(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataTriesNStatusDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentDataTriesNStatus(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataTriesNTimeDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetStudentDataTriesNTime(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataMasteryScoreDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetStudentDataMasteryScore(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataMaxTimeAllowedDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetStudentDataMaxTimeAllowed(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataTimeLimitActionDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentDataTimeLimitAction(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentDataTriesDuringLessonDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetStudentDataTriesDuringLesson(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceAudioDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetStudentPreferenceAudio(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceLanguageDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentPreferenceLanguage(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceLessonTypeDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentPreferenceLessonType(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceSpeedDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetStudentPreferenceSpeed(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceTextDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetStudentPreferenceText(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceTextColorDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentPreferenceTextColor(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceTextLocationDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentPreferenceTextLocation(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceTextSizeDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentPreferenceTextSize(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceVideoDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentPreferenceVideo(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void StudentPreferenceWindows1Delegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetStudentPreferenceWindows1(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNIdDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetInteractionsNId(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNObjectivesNIdDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetInteractionsNObjectivesNId(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNTimeDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetInteractionsNTime(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNTypeDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetInteractionsNType(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNCorrectResponsesNPatternDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetInteractionsNCorrectResponsesNPattern(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNWeightingDelegate(string key, Dictionary<string, string> json_values)
        {
            double? tmp = GetInteractionsNWeighting(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNStudentResponseDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetInteractionsNStudentResponse(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNResultDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetInteractionsNResult(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void InteractionsNLatencyDelegate(string key, Dictionary<string, string> json_values)
        {
            int? tmp = GetInteractionsNLatency(json_values);
            if (tmp != null)
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

            if (!String.IsNullOrEmpty(tmp))
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
            if (!String.IsNullOrEmpty(tmp))
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

            if (!String.IsNullOrEmpty(exit))
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

            /*string lesson_mode = GetLessonMode(json_values);
            bool? credit = GetCredit(json_values);
            double? mastery_score = GetMasteryScore(json_values);
            double? score_raw = GetScoreRaw(json_values);
            */
            ret = lesson_status;
/*          
 * According to the AICC standard version 4.0 (pg 22), the Lesson Status must be
 * reported by the AU (i.e. the course) in the HACP binding, whereas the API binding 
 * does not require the value. However, the API/JavaScript calculates the proper 
 * lesson status. So, this code is redundant.

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
            }*/

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

        private string GetLaunchData(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.launch_data", out val);

            return val;
        }

        private string GetEvaluationCommentsNContent(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.evaluation.comments.n.content", out val);

            return val;
        }

        private string GetEvaluationCommentsNLocation(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.evaluation.comments.n.location", out val);

            return val;
        }

        private int? GetEvaluationCommentsNTime(Dictionary<string, string> json_values)
        {
            return GetTime("cmi.evaluation.comments.n.time", json_values);
        }

        private string GetCommentsFromLms(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.comments_from_lms", out val);

            return val;
        }

        private string GetObjectivesNId(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.objectives.n.id", out val);

            return val;
        }

        private double? GetObjectivesNScoreRaw(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.objectives.n.score.raw", json_values);
        }

        private double? GetObjectivesNScoreMin(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.objectives.n.score.min", json_values);
        }

        private double? GetObjectivesNScoreMax(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.objectives.n.score.max", json_values);
        }

        private string GetObjectivesNStatus(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.objectives.n.status", out val);

            return val;
        }

        private string GetStudentDataAttemptNumber(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_data.attempt_number", out val);

            return val;
        }

        private double? GetStudentDataTriesNScoreRaw(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.student_data.tries.n.score.raw", json_values);
        }

        private double? GetStudentDataTriesNScoreMin(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.student_data.tries.n.score.min", json_values);
        }

        private double? GetStudentDataTriesNScoreMax(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.student_data.tries.n.score.max", json_values);
        }

        private string GetStudentDataTriesNStatus(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_data.tries.n.status", out val);

            return val;
        }

        private int? GetStudentDataTriesNTime(Dictionary<string, string> json_values)
        {
            return GetTime("cmi.student_data.tries.n.time", json_values);
        }

        private double? GetStudentDataMasteryScore(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.student_data.mastery_score", json_values);
        }

        private int? GetStudentDataMaxTimeAllowed(Dictionary<string, string> json_values)
        {
            return GetTime("cmi.student_data.max_time_allowed", json_values);
        }

        private string GetStudentDataTimeLimitAction(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_data.time_limit_action", out val);

            return val;
        }

        private int? GetStudentDataTriesDuringLesson(Dictionary<string, string> json_values)
        {
            int? ret = null;
            string tmp = null;
            json_values.TryGetValue("cmi.student_data.tries_during_lesson", out tmp);

            int i;
            bool res = Int32.TryParse(tmp, out i);
            if (res)
                ret = i;

            return ret;
        }

        private int? GetStudentPreferenceAudio(Dictionary<string, string> json_values)
        {
            int? ret = null;
            string tmp = null;
            json_values.TryGetValue("cmi.student_preference.audio", out tmp);

            int i;
            bool res = Int32.TryParse(tmp, out i);
            if (res)
                ret = i;

            return ret;
        }

        private string GetStudentPreferenceLanguage(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_preference.language", out val);

            return val;
        }

        private string GetStudentPreferenceLessonType(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_preference.lesson_type", out val);

            return val;
        }

        private int? GetStudentPreferenceSpeed(Dictionary<string, string> json_values)
        {
            int? ret = null;
            string tmp = null;
            json_values.TryGetValue("cmi.student_preference.speed", out tmp);

            int i;
            bool res = Int32.TryParse(tmp, out i);
            if (res)
                ret = i;

            return ret;
        }

        private int? GetStudentPreferenceText(Dictionary<string, string> json_values)
        {
            int? ret = null;
            string tmp = null;
            json_values.TryGetValue("cmi.student_preference.text", out tmp);

            int i;
            bool res = Int32.TryParse(tmp, out i);
            if (res)
                ret = i;

            return ret;
        }

        private string GetStudentPreferenceTextColor(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_preference.text_color", out val);

            return val;
        }

        private string GetStudentPreferenceTextLocation(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_preference.text_location", out val);

            return val;
        }

        private string GetStudentPreferenceTextSize(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_preference.text_size", out val);

            return val;
        }

        private string GetStudentPreferenceVideo(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_preference.video", out val);

            return val;
        }
        
        private string GetStudentPreferenceWindows1(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.student_preference.windows.1", out val);

            return val;
        }

        private string GetInteractionsNId(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.interactions.n.id", out val);

            return val;
        }

        private string GetInteractionsNObjectivesNId(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.interactions.n.objectives.n.id", out val);

            return val;
        }

        private int? GetInteractionsNTime(Dictionary<string, string> json_values)
        {
            return GetTime("cmi.interactions.n.time", json_values);
        }

        private string GetInteractionsNType(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.interactions.n.type", out val);

            return val;
        }

        private string GetInteractionsNCorrectResponsesNPattern(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.interactions.n.correct_responses.n.pattern", out val);

            return val;
        }

        private double? GetInteractionsNWeighting(Dictionary<string, string> json_values)
        {
            return GetDouble("cmi.interactions.n.weighting", json_values);
        }

        private string GetInteractionsNStudentResponse(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.interactions.n.student_response", out val);

            return val;
        }

        private string GetInteractionsNResult(Dictionary<string, string> json_values)
        {
            string val;
            json_values.TryGetValue("cmi.interactions.n.result", out val);

            return val;
        }

        private int? GetInteractionsNLatency(Dictionary<string, string> json_values)
        {
            return GetTime("cmi.interactions.n.latency", json_values);
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
