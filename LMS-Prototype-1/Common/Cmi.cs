using System;
using System.Collections.Generic;

/*
 * CMI class (POCO)
 *
 * Description: The C# analogue of the JSON class 
 *              in the AICC/SCORM API.
 * 
 */
class Cmi
{
    public class Core
    {
        public class Score
        {
            public double raw { get; set; } // cmi.core.score.raw
            public double min { get; set; } // cmi.core.score.min
            public double max { get; set; } // cmi.core.score.max
        }
        
        public string student_id { get; set; } // cmi.core.student_id
        public string student_name { get; set; } // cmi.core.student_name
        public string lesson_location { get; set; } // cmi.core.lesson_location
        public bool credit { get; set; } // cmi.core.credit
        public string lesson_mode { get; set; } // cmi.core.lesson_mode
        public int lesson_status { get; set; } // cmi.core.lesson_status

        //public int session_time { get; set; } // cmi.core.session_time
        public int total_time { get; set; } // cmi.core.total_time
        public string exit { get; set; } // cmi.core.exit
        public bool entry { get; set; }  // cmi.core.entry

        public Score score { get; set; }

    }

    public class Student_Data
    {
        public class Tries
        {
            public class Score
            {
                public double raw { get; set; } // cmi.core.score.raw
                public double min { get; set; } // cmi.core.score.min
                public double max { get; set; } // cmi.core.score.max
            }
            public double score { get; set; } // cmi.student_data.tries.n.score
            public double score_raw { get; set; } // cmi.student_data.tries.n.score.raw
            public double score_max { get; set; } // cmi.student_data.tries.n.score.max
            public double score_min { get; set; } // cmi.student_data.tries.n.score.min
            
            public string status { get; set; } // cmi.student_data.tries.n.status
            public int time { get; set; } // cmi.student_data.tries.n.time
        }

        public int attempt_number { get; set; } // cmi.student_data.attempt_number
        public string lesson_status { get; set; } // cmi.student_data.lesson_status.n
        public double score { get; set; } // cmi.student_data.score.n
        public double mastery_score { get; set; } // cmi.student_data.mastery_score

        public int max_time_allowed { get; set; } // cmi.student_data.max_time_allowed
        public string time_limit_action { get; set; } // cmi.student_data.time_limit_action
        public int tries_during_lesson { get; set; } // cmi.student_data.tries_during_lesson

        public List<Tries> tries { get; set; }

    }

    public class Objectives
    {
        public class Score
        {
            public double raw { get; set; } // cmi.core.score.raw
            public double min { get; set; } // cmi.core.score.min
            public double max { get; set; } // cmi.core.score.max
        }
        public string id { get; set; } // cmi.objectives.n.id
        public Score score { get; set; }
            //cmi.objectives.n.score.raw
            //cmi.objectives.n.score.min
            //cmi.objectives.n.score.max
        public string status { get; set; } // cmi.objectives.n.status

    }

    public class Evaluation
    {
        public class Comments
        {
             public string content { get; set; } // cmi.evaluation.comments.n.content
             public int time { get; set; } // cmi.evaluation.comments.n.time
             public string location { get; set; } // cmi.evaluation.comments.n.location
        }
        
        public List<Comments> comments { get; set; }
    }


    public class Interactions
    {
        public class Objective { public string id { get; set; } }
        public class Correct_Response { public string pattern { get; set; } }
        
        public string id { get; set; } // cmi.interactions.n.id
        public string date { get; set; } // cmi.interactions.n.date
        public string time { get; set; } // cmi.interactions.n.time
        
        public List<Objective> objectives { get; set; } // cmi.interactions.n.objective.n.id
        
        public string type { get; set; } // cmi.interactions.n.type

        public List<Correct_Response> correct_responses { get; set; } // cmi.interactions.n.correct_response.n.pattern
        
        public string student_response { get; set; } // cmi.interactions.n.student_response
        public string result { get; set; } // cmi.interactions.n.result
        public double weighting { get; set; } // cmi.interactions.n.weighting
        public int latency { get; set; } // cmi.interactions.n.latency
    }

    public class Student_Preference
    {
        //public class Windows
        public int audio { get; set; } // cmi.student_preference.audio
        public string language { get; set; } // cmi.student_preference.language
        public string lesson_type { get; set; } // cmi.student_preference.lesson_type
        public int speed { get; set; } // cmi.student_preference.speed
        public int text { get; set; } // cmi.student_preference.text
        public string text_color { get; set; } // cmi.student_preference.text_color
        public string text_location { get; set; } // cmi.student_preference.text_location
        public string text_size { get; set; } // cmi.student_preference.text_size
        public string video { get; set; } // cmi.student_preference.video

        public List<string> windows { get; set; } // cmi.student_preference.windows.n
    }

    public class RootObject
    {
        public string suspend_data { get; set; } // cmi.suspend_data 
        public string launch_data { get; set; } // cmi.launch_data
        public string comments_from_lms { get; set; } // cmi.comments - instructor comments (sent to content/API)
        public string comments { get; set; } // cmi.comments - student comments (sent to LMS)

        public Core core { get; set; }
        public Student_Data student_data { get; set; }
        public List<Objectives> objectives { get; set; }
        public Evaluation evaluation { get; set; }
        public List<Interactions> interactions { get; set; }
        public Student_Preference student_preference { get; set; }

    }
    
}