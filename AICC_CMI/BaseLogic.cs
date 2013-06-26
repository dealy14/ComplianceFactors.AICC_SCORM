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
            m_delegates["cmi.core.lesson_status"] = LessonStatusDelegate;
        }

        public void ConsumeJSObj(Dictionary<string, string> m_json_values)
        {
            Dictionary<string, string>.KeyCollection keys = m_json_values.Keys;
            foreach (string key in keys)
            {
                if (m_delegates.ContainsKey(key))
                {
                    FieldDelegate del = m_delegates[key];
                    del(key, m_json_values);
                }
            }
        }

        #region Delegates
        private void LessonStatusDelegate(string key, Dictionary<string, string> json_values)
        {
            string tmp = GetLessonStatus(json_values);
            if (tmp != null)
                this.m_values[key] = tmp;
        }
        #endregion

        #region Getters

        private string GetCredit(Dictionary<string, string> m_json_values)
        {
            string credit;
            m_json_values.TryGetValue("cmi.core.credit", out credit);

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

        private double? GetDouble(string key, Dictionary<string, string> m_json_values)
        {
            double? ret = null;
            string tmp = null;
            m_json_values.TryGetValue(key, out tmp);
            if (tmp != null)
            {
                double d;
                bool res = Double.TryParse(tmp, out d);
                if (res)
                    ret = d;
            }

            return ret;
        }

        private string GetLessonMode(Dictionary<string, string> m_json_values)
        {
            string lesson_mode;
            m_json_values.TryGetValue("cmi.core.lesson_mode", out lesson_mode);

            return lesson_mode;
        }

        private string GetLessonStatus(Dictionary<string, string> m_json_values)
        {
            string ret = null;
            string lesson_status = null;
            m_json_values.TryGetValue("cmi.core.lesson_status", out lesson_status);
            string lesson_mode = GetLessonMode(m_json_values);
            string credit = GetCredit(m_json_values);
            double? mastery_score = GetMasteryScore(m_json_values);
            double? score_raw = GetScoreRaw(m_json_values);

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
                if (lesson_mode == "browse")
                {
                    if (lesson_status == "not attempted")
                        ret = "browsed";
                }
            }

            return ret;
        }

        private double? GetMasteryScore(Dictionary<string, string> m_json_values)
        {
            return GetDouble("cmi.student_data.mastery_score", m_json_values);
        }

        private double? GetScoreRaw(Dictionary<string, string> m_json_values)
        {
            return GetDouble("cmi.core.score.raw", m_json_values);
        }

        private double? GetScoreMax(Dictionary<string, string> m_json_values)
        {
            return GetDouble("cmi.core.score.max", m_json_values);
        }

        private double? GetScoreMin(Dictionary<string, string> m_json_values)
        {
            return GetDouble("cmi.core.score.min", m_json_values);
        }

        #endregion
    }

}
