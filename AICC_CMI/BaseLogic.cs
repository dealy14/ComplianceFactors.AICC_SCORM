using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AICC_CMI
{
    public class BaseLogic
    {
        private delegate void FieldDelegate(string key, Dictionary<string, string> json_values);

        private Dictionary<string, string> m_json_values = null;
        private Dictionary<string, object> m_values = new Dictionary<string, object>();
        private Dictionary<string, FieldDelegate> m_delegates = new Dictionary<string, FieldDelegate>();

        public BaseLogic()
        {
            InitDelegates();
        }

        public BaseLogic(Dictionary<string, string> json_values)
        {
            InitDelegates();
            m_json_values = json_values;
        }

        private void InitDelegates()
        {
            m_delegates["cmi.core.credit"] = CreditDelegate;
            m_delegates["cmi.core.lesson_location"] = LessonLocationDelegate;
            m_delegates["cmi.core.lesson_mode"] = LessonModeDelegate;
            m_delegates["cmi.core.lesson_status"] = LessonStatusDelegate;
            m_delegates["cmi.student_data.mastery_score"] = MasteryScoreDelegate;
            m_delegates["cmi.core.score.raw"] = ScoreRawDelegate;
            m_delegates["cmi.core.score.min"] = ScoreMinDelegate;
            m_delegates["cmi.core.score.max"] = ScoreMaxDelegate;
            m_delegates["cmi.core.session_time"] = SessionTimeDelegate;
            m_delegates["cmi.suspend_data"] = SuspendDataDelegate;
            m_delegates["cmi.comments"] = CommentsDelegate;
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

        #region Delegates
        private void CommentsDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetComments(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void CreditDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetCredit(json_values);
            if (tmp != null)
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
            TimeSpan? tmp = GetSessionTime(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }

        private void SuspendDataDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetSuspendData(json_values);
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

        private string GetCredit(Dictionary<string, string> json_values)
        {
            string credit;
            json_values.TryGetValue("cmi.core.credit", out credit);

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
            string entry;
            json_values.TryGetValue("cmi.core.entry", out entry);

            return (entry == "1");
        }

        private string GetExit(Dictionary<string, string> json_values)
        {
            string exit;
            json_values.TryGetValue("cmi.core.exit", out exit);

            return exit;
        }

        private string GetLessonLocation(Dictionary<string, string> json_values)
        {
            string lesson_location;
            json_values.TryGetValue("cmi.core.lesson_location", out lesson_location);

            return lesson_location;
        }

        private string GetLessonMode(Dictionary<string, string> json_values)
        {
            string lesson_mode;
            json_values.TryGetValue("cmi.core.lesson_mode", out lesson_mode);

            return lesson_mode;
        }

        private string GetLessonStatus(Dictionary<string, string> json_values)
        {
            string ret = null;
            string lesson_status = null;
            json_values.TryGetValue("cmi.core.lesson_status", out lesson_status);
            string lesson_mode = GetLessonMode(json_values);
            string credit = GetCredit(json_values);
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
                        if (credit == "credit")
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

        private TimeSpan? GetSessionTime(Dictionary<string, string> json_values)
        {
            return GetTime("cmi.core.session_time", json_values);
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

        private TimeSpan? GetTime(string key, Dictionary<string, string> json_values)
        {
            TimeSpan? ret = null;
            string tmp = null;
            json_values.TryGetValue(key, out tmp);
            if (tmp != null)
            {
                TimeSpan t;
                bool res = TimeSpan.TryParse(tmp, out t);
                if (res)
                    ret = t;
            }

            return ret;
        }

        #endregion
    }

}
