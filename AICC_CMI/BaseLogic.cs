using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AICC_CMI
{
    public class BaseLogic
    {
        private dynamic m_json = null;
        private Dictionary<string, object> m_dValues;

        //private string m_lessonstatus = "";
        //private double m_scoreraw, m_score_max, m_score_min, m_mastery_score;

        public BaseLogic()
        {
        }

        public BaseLogic(object json)
        {
            m_json = json;
        }

        //public string LessonStatus
        //{
        //    get { return m_lessonstatus; }
        //    set
        //    {
        //        if (m_json.cmi.core.lesson_mode == "normal")
        //        {
        //            if (m_json.cmi.core.credit == "credit")
        //            {
        //                if (m_json.cmi.student_data.mastery_score != "" && m_json.cmi.core.score.raw != "")
        //                {
        //                    this.SetScoreRaw(m_json.cmi.core.score.raw);
        //                    this.SetMasteryScore(m_json.cmi.student_data.mastery_score);

        //                    if (this.ScoreRaw >= this.MasteryScore)
        //                        m_lessonstatus = "passed";
        //                    else
        //                        m_lessonstatus = "failed";
        //                }
        //            }
        //        }
        //        if (m_json.cmi.core.lesson_mode == "browse")
        //        {
        //            if (m_json.cmi.core.lesson_status == "not attempted")
        //                m_lessonstatus = "browsed";
        //        }
        //    }
        //}

        //public double ScoreRaw
        //{
        //    get { return m_scoreraw; }
        //    set { m_scoreraw = value; }
        //}

        //public double ScoreMax
        //{
        //    get { return m_score_max; }
        //    set { }
        //}

        //public double ScoreMin
        //{
        //    get { return m_score_min; }
        //    set { }
        //}

        //public double MasteryScore
        //{
        //    get { return m_mastery_score; }
        //    set { }
        //}

        //private void SetScoreRaw(string value)
        //{
        //    this.ScoreRaw = Convert.ToDouble(value);
        //}

        //private void SetMasteryScore(string value)
        //{
        //    this.MasteryScore = Convert.ToDouble(value);
        //}


        //private bool isPopulated = false;

        // public BaseLogic(guid) // overloaded ctor
                // { 
                //      assign all properties from EF;
                //      isPopulated = true;
                // }

        //public Persist(ComplianceFactorsEntities context)
        //{
        //    foreach (key, value in m_dValues)
        //    {
        //        context[dMap[key]] = value;
        //    }

        //    //if (foo has changed)
        //    //  context.foo = this.foo
        //}

        //public void ConsumeJSObj(dynamic m_json)
        //{
        //    keys = m_json.Keys;
        //    foreach (key in keys)
        //    {
        //        mthd = delegates[key];
        //        mthd(key, m_json);
        //    }
        //}

        //private void LessonStatusDelegate(string key, dynamic m_json)
        //{
        //    string tmp = CalcLessonStatus(key, m_json);
        //    if (tmp.Length > 0)
        //        this.m_dValues[key] = tmp;
        //}

        //private string CalcLessonStatus(string key, dynamic m_json)
        //{
        //    string ret = "";
        //    string lesson_status;
        //    m_json.TryGetValue("cmi.core.lesson_status", out lesson_status);

        //    if (lesson_status != "")
        //    {
        //        if (m_json.cmi.core.lesson_mode == "normal")
        //        {
        //            if (m_json.cmi.core.credit == "credit")
        //            {
        //                if (m_json.cmi.student_data.mastery_score != "" && m_json.cmi.core.score.raw != "")
        //                {
        //                    this.SetScoreRaw(m_json.cmi.core.score.raw);
        //                    this.SetMasteryScore(m_json.cmi.student_data.mastery_score);

        //                    if (this.ScoreRaw >= this.MasteryScore)
        //                        ret = "passed";
        //                    else
        //                        ret = "failed";
        //                }
        //            }
        //        }
        //        if (m_json.cmi.core.lesson_mode == "browse")
        //        {
            //        if (m_json.cmi.core.lesson_status == "not attempted")
            //            ret = "browsed";
            //    }
            //}

            //return ret;

        //}
    }

}
