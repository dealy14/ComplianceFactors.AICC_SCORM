﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LMS_Prototype_1
{
    public partial class AICC_SCORM_API_js : System.Web.UI.Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Response.ContentType = "application/x-javascript";
        }

        protected string enrollment_id, student_id, student_name,
                 lesson_mode, lesson_location, credit,
                 lesson_status, vExit, vEntry, launch_data, suspend_data,
                 total_time, score_raw, score_max, score_min, mastery_score;
        protected string aicc_url, aicc_sid, content_url;

        protected void Page_Load(object sender, EventArgs e)
        {
            enrollment_id = this.Request["eid"]; // accepts 'eid' guid value from GET or POST (should be GET, due to AICC_URL/SID)
            //enrollment_id = "49e8ae11-ecfa-4c08-8bd9-bba9a8521414";
            //string student_id, student_name, lesson_location, credit, lesson_status, vExit, vEntry;

            // AICC HACP URL Parse
            aicc_url = this.Request["aicc_url"];
            aicc_sid = this.Request["aicc_sid"];

            using (ComplianceFactorsEntities context = new ComplianceFactorsEntities())
            {
                var enroll = (from en in context.e_tb_enrollments
                              where en.e_enroll_system_id_pk == new Guid(enrollment_id)
                              select en).FirstOrDefault();

                if (enroll == null) //no record found, invalid eid
                {
                    Response.Write("Invalid Enrollment");
                    Response.End();
                }

                content_url = enroll.c_tb_deliveries_master.c_olt_launch_url;

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
                launch_data = enroll.e_enroll_launch_data ?? "";
                suspend_data = enroll.e_enroll_suspend_data ?? "";

                score_raw = Convert.ToString(enroll.e_enroll_score);
                score_max = Convert.ToString(enroll.e_enroll_score_max);
                score_min = Convert.ToString(enroll.e_enroll_score_min);

                lesson_mode = (enroll.e_enroll_lesson_mode ?? "normal"); //for now...
                total_time = TimeSpan.FromSeconds((double)(enroll.e_enroll_time_spent ?? 0)).ToString(@"hh\:mm\:ss");

                //context.SaveChanges();
            }

        }
    }
}