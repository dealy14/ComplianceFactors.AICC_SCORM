using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Dynamic;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics;
using System.Web.Script.Services;
using System.Data.Entity;
using System.Data;

namespace LMS_Prototype_1
{
    /// <summary>
    /// AICC_SCORM Web Service
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class AICC_SCORM : WebService
    {
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PostCMI(string cmi_dto)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

            dynamic cmi = jss.Deserialize(cmi_dto, typeof(object)) as dynamic;

            /*
                cmi.comments
                cmi.comments
                cmi.comments_from_lms
            */
            
            //var test = cmi["interactions_"+0].id;

            var interactions_count = cmi.interactions._count;
           
            string enrollment_id = cmi.enrollment_id;

            using (ComplianceFactorsEntities context = new ComplianceFactorsEntities())
            {
               
                var enroll = (from e in context.e_tb_enrollments
                               where e.e_enroll_system_id_pk == new Guid(enrollment_id)
                                 select e).FirstOrDefault();
                
                if (enroll == null) //no record found, invalid eid
                {
                    return "Invalid Enrollment";
                }

                var score_raw = cmi.core.score.raw ?? "";
                enroll.e_enroll_score = Convert.ToDecimal(score_raw);

                enroll.e_enroll_lesson_location = cmi.core.lesson_location;
                enroll.e_enroll_lesson_status = cmi.core.lesson_status ?? "";
                enroll.e_enroll_exit = cmi.core.exit;

                var score_max = (cmi.core.score.max ?? "");
                var score_min = (cmi.core.score.min ?? "");
                //enroll.e_enroll_score_max = Convert.ToDecimal(score_max);
                //enroll.e_enroll_score_min = Convert.ToDecimal(score_min);

                if (null == enroll.e_enroll_time_spent)
                    enroll.e_enroll_time_spent = TimeSpan.Parse(cmi.core.total_time).Seconds;
                else
                    enroll.e_enroll_time_spent = TimeSpan.Parse(cmi.core.total_time).Seconds 
                                                    + TimeSpan.Parse(cmi.core.session_time).Seconds;

                enroll.e_enroll_suspend_data = cmi.suspend_data;
                //enroll.e_enroll_student_comments = cmi.comments; 
                    // TODO: review logic for this element (double meaning, depending on direction of call)
                
                context.SaveChanges();
            
                
                //Was LMSFinish / Terminate called?
                if (cmi.terminate == "true" && lessonCompleted(cmi.core.lesson_status) )
                {
                    // do completion process; create transcript record; disable enrollment so as not to show in 'my courses'

                    enroll.e_enroll_completion_date = DateTime.Now;
                    enroll.e_enroll_active_flag = false;

                    t_tb_transcripts tx = t_tb_transcripts.Createt_tb_transcripts(Guid.NewGuid(),
                        enroll.e_enroll_user_id_fk,
                        enroll.e_enroll_course_id_fk,
                        enroll.e_enroll_delivery_id_fk,
                        "cd8a0438-0631-4996-8bc0-5b9609e70cb6",//"OLT Player",
                        enroll.e_enroll_lesson_status,
                        DateTime.Now,
                        "cd8a0438-0631-4996-8bc0-5b9609e70cb6",//"OLT Player",
                        new Guid(),// all zeroes
                        DateTime.Now
                        );
                    tx.t_transcript_target_due_date = enroll.e_enroll_target_due_date;
                    tx.t_transcript_status_id_fk = enroll.e_enroll_status_id_fk;
                    tx.t_transcript_score = enroll.e_enroll_score;
                    
                    //TODO: score min/max must be added to transcripts table
                    
                    tx.t_transcript_credits = enroll.c_tb_deliveries_master.c_delivery_credit_units;
                    
                    tx.t_transcript_hours = enroll.c_tb_deliveries_master.c_delivery_credit_hours;
                    tx.t_transcript_time_spent = enroll.e_enroll_time_spent;
                    //tx.t_transcript_completion_score = enroll.e_enroll_score;
                    
                    tx.t_transcript_active_flag = true;
                    
                    context.SaveChanges();
                }
            
            }

            return "";
        }

        private bool lessonCompleted(string lesson_status){
        
            switch (lesson_status.ToLower()){
                case "completed":
                case "c":
                case "passed":
                case "p":
                case "failed":
                case "f":
                case "browsed":
                case "b":
                    return true;
                default:
                    return false;
            }

        }
    }
    public class DynamicJsonObject : DynamicObject
    {
        private IDictionary<string, object> Dictionary { get; set; }

        public DynamicJsonObject(IDictionary<string, object> dictionary)
        {
            this.Dictionary = dictionary;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this.Dictionary[binder.Name];

            if (result is IDictionary<string, object>)
            {
                result = new DynamicJsonObject(result as IDictionary<string, object>);
            }
            else if (result is ArrayList && (result as ArrayList) is IDictionary<string, object>)
            {
                result = new List<DynamicJsonObject>((result as ArrayList).ToArray().Select(x => new DynamicJsonObject(x as IDictionary<string, object>)));
            }
            else if (result is ArrayList)
            {
                var list = new List<object>();
                foreach (var o in (result as ArrayList).ToArray())
                {
                    if (o is IDictionary<string, object>)
                    {
                        list.Add(new DynamicJsonObject(o as IDictionary<string, object>));
                    }
                    else
                    {
                        list.Add(o);
                    }
                }
                result = list;
            }

            return this.Dictionary.ContainsKey(binder.Name);
        }
    }

    public class DynamicJsonConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (type == typeof(object))
            {
                return new DynamicJsonObject(dictionary);
            }

            return null;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(object) })); }
        }
    }
}
