using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.Entity;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.Collections;
using System.Dynamic;
using EntityFrameworkLayer;
using HACP;

namespace LMS_Prototype_1.Courses
{
    public partial class HACP_Handler : System.Web.UI.Page
    {
        protected string enrollment_id, student_id, student_name,
                    lesson_mode, lesson_location, credit,
                    lesson_status, vExit, vEntry, launch_data, suspend_data,
                    total_time, score_raw, score_max, score_min, mastery_score;

        protected string responseString = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            string command = Request.Params["command"];
            string sessionid = Request.Params["session_id"];
            string aiccdata = Request.Params["aicc_data"];

            if (String.IsNullOrEmpty(command))
            {
                sendOutput("error=1\r\nerror_text=Invalid Command\r\n");
            }
            if (String.IsNullOrEmpty(sessionid))
            {
                sendOutput("error=3\r\nerror_text=Invalid Session ID\r\n");
            }

            switch (command.ToLower())
            {
                case "getparam":
                    using (ComplianceFactorsEntities context = new ComplianceFactorsEntities())
                    {
                        var enroll = (from en in context.e_tb_enrollments
                                      where en.e_enroll_system_id_pk == new Guid(sessionid)
                                      select en).FirstOrDefault();

                        if (enroll == null) //no record found, invalid eid
                        {
                            sendOutput("error=3\r\nerror_text=Invalid Session ID\r\n");
                        }

                        student_id = enroll.e_enroll_user_id_fk.ToString();
                        student_name = enroll.u_tb_users_master.u_last_name + ", "
                                        + enroll.u_tb_users_master.u_first_name
                                        + " " + enroll.u_tb_users_master.u_middle_name;
                        lesson_location = enroll.e_enroll_lesson_location ?? "";
                        credit = ((bool)(enroll.e_enroll_credit ?? true)) ? "credit" : "no-credit";

                        // Status
                        //  account for null/empty value
                        if (String.IsNullOrEmpty(enroll.e_enroll_lesson_status))
                        {
                            lesson_status = "not attempted";
                        }
                        // set status to 'incomplete'
                        if (enroll.e_enroll_lesson_status == "not attempted")
                        {
                            lesson_status = "incomplete";
                        }


                        vExit = enroll.e_enroll_exit;
                        if (vExit == "time-out" || vExit == "logout" || String.IsNullOrEmpty(vExit))
                        {
                            vEntry = "";
                        }
                        else if (vExit == "suspend")
                        {
                            vEntry = "resume";
                            //enroll.e_enroll_entry = true;
                        }
                        else
                        {
                            vEntry = "ab-initio";
                            //enroll.e_enroll_entry = false;
                        } 

                        //vEntry = ((bool)(enroll.e_enroll_entry ?? false)) ? "resume" : "ab-initio"; // ab-initio, resume, or ""->neither of the former
                        suspend_data = enroll.e_enroll_suspend_data ?? "";
                        launch_data = enroll.e_enroll_launch_data ?? "";

                        score_raw = Convert.ToString(enroll.e_enroll_score);
                        score_max = Convert.ToString(enroll.e_enroll_score_max);
                        score_min = Convert.ToString(enroll.e_enroll_score_min);

                        lesson_mode = (enroll.e_enroll_lesson_mode ?? "normal"); //for now...
                        total_time = TimeSpan.FromSeconds((double)(enroll.e_enroll_time_spent ?? 0)).ToString(@"hh\:mm\:ss");
                
                        //context.SaveChanges();
                    }

                    responseString += "error=0\r\nerror_text=Successful\r\naicc_data=";
                    responseString += "[Core]\r\n";
                    responseString += "Student_ID=" + student_id + "\r\n";
                    responseString += "Student_Name=" + student_name + "\r\n";
                    responseString += "Lesson_Location=" + lesson_location + "\r\n";
                    responseString += "Credit=" + credit + "\r\n";
                    responseString += "Lesson_Status=" + lesson_status + ", " + vExit  + "\r\n";;
                    
                    var score = score_raw;
                    if (!String.IsNullOrEmpty(score_max)) { score += ", " + score_max; }
                    if (!String.IsNullOrEmpty(score_min)) { score += ", " + score_min; }
                    responseString += "Score=" + score + "\r\n";

                    responseString += "Time=" + total_time + "\r\n";
                    responseString += "Lesson_Mode=" + lesson_mode + "\r\n";
                    responseString += "[Core_Lesson]\r\n";
                    if (suspend_data != "") { responseString += suspend_data + "\r\n"; }
                    responseString += "[Core_Vendor]\r\n" + launch_data + "\r\n";
                    
                    /*
                    "[Evaluation]\r\nCourse_ID = {".."}\r\n";
                    "[Student_Data]\r\n";
                    'Mastery_Score='.."\r\n";
                    'Max_Time_Allowed='.."\r\n";
                    'Time_Limit_Action='.."\r\n";
                    */

                    sendOutput(responseString);

                    break;

                case "putparam":
                    //save passed parameters
                    
                    //parse putparam
                    HACP_Parser hacpParser = new HACP_Parser();
                    
                    hacpParser.parsePutParam(sessionid, HttpUtility.UrlDecode(aiccdata));

                    //persist values...

                    break;

                case "putcomments":
                    // parse CSV
                    break;

                
                case "putinteractions":
                    // parse CSV
                    break;

                case "exitau":
                    // wrap up session
                    
                    // use cmi.terminate to trigger end of session
                    
                    break;

                default:
                    sendOutput("error=1\r\nerror_text=Invalid Command\r\n");
                    break;
            }



            // TODO: handle 'running' and 'terminated'...
        

        
        }

        private void sendOutput(string output){
            Response.Write(output);
            Response.End();
        }
    
    
 
    }
}