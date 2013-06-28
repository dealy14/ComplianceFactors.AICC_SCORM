using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AICC_CMI
{
    public class JS_API_Logic : BaseLogic
    {
        protected override void InitLessonStatusMap()
        {
            m_map_lesson_status.Add("a", "attempted");
            m_map_lesson_status.Add("b", "browsed");
            m_map_lesson_status.Add("c", "completed");
            m_map_lesson_status.Add("f", "failed");
            m_map_lesson_status.Add("i", "incomplete");
            m_map_lesson_status.Add("p", "passed");
        }

        protected override void InitLessonStatusesCompleted()
        {
            m_lesson_statuses_completed.Add("completed"); 
            m_lesson_statuses_completed.Add("passed"); 
            m_lesson_statuses_completed.Add("failed");
            m_lesson_statuses_completed.Add("browsed");
        }
    }
}
