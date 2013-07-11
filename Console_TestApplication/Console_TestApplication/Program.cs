using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using LMS_Prototype_1;
using System.Diagnostics;
using AICC_CMI;
using System.Data.Entity;
using System.Data;
using EntityFrameworkLayer;
using System.Web.Script.Serialization;
using HACP;

namespace Console_TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            TempTest();

            HACPParserTest();
            JSAPILogicTest();
            HACP_API_Logic_Test();

            AuditTest();

            JSONTest();

            Console.Write("press any key to continue...");
            Console.ReadKey(false);
        }

        static void TempTest()
        {
        }

        static void JSAPILogicTest()
        {
            Dictionary<string, string> json = new Dictionary<string,string>();
            json.Add("cmi.core.lesson_location", "end");
            json.Add("cmi.core.credit", "credit");
            json.Add("cmi.core.lesson_status", "attempted");
            json.Add("cmi.core.score.raw", "8.5");
            json.Add("cmi.core.score.min", "0");
            json.Add("cmi.core.score.max", "10");
            json.Add("cmi.core.session_time", "00:47:00");
            json.Add("cmi.core.total_time", "1:23:00");
            json.Add("cmi.core.lesson_mode", "normal");
            json.Add("cmi.student_data.mastery_score", "8");
            json.Add("cmi.suspend_data", "9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df");
            json.Add("cmi.comments", "<1>The background color is too blue!<1.e><2>The CDU\r\npanel has the incorrect ‘way points’ displayed for\r\nthis route. <2.e><3>The CDU panel has the incorrect\r\n‘way points’ displayed for this route. <3.e><4>The\r\nCDU panel has the incorrect ‘way points’ displayed\r\nfor this route. <e.4>");
            json.Add("cmi.terminate", "true");

            AICC_CMI.JS_API_Logic jslogic = new AICC_CMI.JS_API_Logic();
            jslogic.ConsumeJSObj(json);

            Debug.Assert(((string)jslogic.GetValue("cmi.core.lesson_location") == "end"));
            Debug.Assert(((bool)jslogic.GetValue("cmi.core.credit")));
            Debug.Assert(((string)jslogic.GetValue("cmi.core.lesson_status") == "passed"));
            Debug.Assert(((double)jslogic.GetValue("cmi.core.score.raw") == 8.5d));
            Debug.Assert(((double)jslogic.GetValue("cmi.core.score.min") == 0d));
            Debug.Assert(((double)jslogic.GetValue("cmi.core.score.max") == 10d));
            Debug.Assert(((int)jslogic.GetValue("cmi.core.session_time") == 2820));
            //Debug.Assert(((string)jslogic.GetValue("cmi.core.total_time", "1:23:00");
            Debug.Assert(((string)jslogic.GetValue("cmi.core.lesson_mode") == "normal"));
            Debug.Assert(((double)jslogic.GetValue("cmi.student_data.mastery_score") == 8d));
            Debug.Assert(((string)jslogic.GetValue("cmi.suspend_data") == "9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df"));
            Debug.Assert(((string)jslogic.GetValue("cmi.comments") == "<1>The background color is too blue!<1.e><2>The CDU\r\npanel has the incorrect ‘way points’ displayed for\r\nthis route. <2.e><3>The CDU panel has the incorrect\r\n‘way points’ displayed for this route. <3.e><4>The\r\nCDU panel has the incorrect ‘way points’ displayed\r\nfor this route. <e.4>"));

            json.Remove("cmi.core.lesson_location");
            json.Remove("cmi.core.lesson_status");
            json.Add("cmi.core.lesson_location", "somewhere before the end");
            json.Add("cmi.core.lesson_status", "incomplete");

            jslogic.ConsumeJSObj(json);

            Debug.Assert(((string) jslogic.GetValue("cmi.core.lesson_status") == "incomplete"));

            json.Remove("cmi.core.credit");
            json.Add("cmi.core.credit", "asdf");

            jslogic.ConsumeJSObj(json);

            Debug.Assert(((bool) jslogic.GetValue("cmi.core.credit") == true));

            json.Remove("cmi.core.credit");
            json.Add("cmi.core.credit", null);

            jslogic.ConsumeJSObj(json);

            Debug.Assert(((bool)jslogic.GetValue("cmi.core.credit") == true));

            json.Remove("cmi.core.credit");
            json.Add("cmi.core.credit", "no-credit");

            jslogic.ConsumeJSObj(json);

            Debug.Assert(((bool)jslogic.GetValue("cmi.core.credit") == false));

            json.Remove("cmi.core.credit");
            json.Remove("cmi.core.lesson_mode");
            json.Add("cmi.core.credit", "credit");
            json.Add("cmi.core.lesson_mode", "browsed");

            jslogic.ConsumeJSObj(json);

            Debug.Assert(((bool)jslogic.GetValue("cmi.core.credit") == false));

            json.Add("cmi.core.exit", "Logout");
            jslogic.ConsumeJSObj(json);
            Debug.Assert(((string) jslogic.GetValue("cmi.core.exit") == "logout"));

            json["cmi.core.exit"] = "boguscrap";
            jslogic.ConsumeJSObj(json);
            Debug.Assert(((string)jslogic.GetValue("cmi.core.exit") == ""));

            json["cmi.terminate"] = "true";
            jslogic.ConsumeJSObj(json);
            Debug.Assert((bool) jslogic.GetValue("cmi.terminate") == true);

            json["cmi.terminate"] = "asdfasdf";
            jslogic.ConsumeJSObj(json);
            Debug.Assert((bool)jslogic.GetValue("cmi.terminate") == false);

            // Testing entire unit
            json.Clear();
            json["cmi.core.lesson_location"] = "end";
            json["cmi.core.credit"] = "credit";
            json["cmi.core.lesson_status"] = "attempted";
            json["cmi.core.score.raw"] = "8.5";
            json["cmi.core.score.min"] = "0";
            json["cmi.core.score.max"] = "10";
            json["cmi.core.session_time"] = "00:47:00"; // 2820
            //json["cmi.core.total_time"] = "1:23:00";    // 4980   ... can't simply declare this if pulling from the database...
            json["cmi.core.lesson_mode"] = "normal";
            json["cmi.student_data.mastery_score"] = "8";
            json["cmi.suspend_data"] = "9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df";
            json["cmi.comments"] = "<1>The background color is too blue!<1.e><2>The CDU\r\npanel has the incorrect ‘way points’ displayed for\r\nthis route. <2.e><3>The CDU panel has the incorrect\r\n‘way points’ displayed for this route. <3.e><4>The\r\nCDU panel has the incorrect ‘way points’ displayed\r\nfor this route. <e.4>";
            json["cmi.terminate"] = "true";

            string enrollment_id = "71dc1b30-b013-4848-9155-17ae45330ca3";

            using (var ctx = new ComplianceFactorsEntities())
            {
               
                var enroll = (from en in ctx.e_tb_enrollments
                              where en.e_enroll_system_id_pk == new Guid(enrollment_id)
                              select en).FirstOrDefault();

                int? total_time = enroll.e_enroll_time_spent;

                jslogic.ConsumeJSObj(json);

                jslogic.Persist(enrollment_id);

                ctx.Refresh(System.Data.Objects.RefreshMode.StoreWins, enroll);

                Debug.Assert(enroll.e_enroll_lesson_location == "end");
                Debug.Assert(enroll.e_enroll_credit == true);
                Debug.Assert(enroll.e_enroll_lesson_status == "passed");
                Debug.Assert(enroll.e_enroll_score == 8.5m); // m --> decimal
                Debug.Assert(enroll.e_enroll_score_min == 0m);
                Debug.Assert(enroll.e_enroll_score_max == 10m);
                Debug.Assert(enroll.e_enroll_time_spent == total_time + 2820);
                Debug.Assert(enroll.e_enroll_lesson_mode == "normal");
                //Debug.Assert(enroll.cmi.student_data.mastery_score == "8");
                Debug.Assert(enroll.e_enroll_suspend_data == "9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df");
                Debug.Assert(enroll.e_enroll_student_comments == "<1>The background color is too blue!<1.e><2>The CDU\r\npanel has the incorrect ‘way points’ displayed for\r\nthis route. <2.e><3>The CDU panel has the incorrect\r\n‘way points’ displayed for this route. <3.e><4>The\r\nCDU panel has the incorrect ‘way points’ displayed\r\nfor this route. <e.4>");

            }

            

        }

        static void HACPParserTest()
        {

            /*
             [Core]
Lesson_Location = 87
Lesson_Status = C
Score =
Time = 00:02:30
[CORE_LESSON]
my lesson state data – 1111111111111111111000000000000000001110000
111111111111111111100000000000111000000000 – end my lesson state data
[COMMENTS]
<1><L.Slide#2> This slide has the fuel listed in the wrong units <e.1>
             
             */


            string value = "[core]\r\nTime = 1002:34:05\r\n" +
                           "lesson_location = end\r\n" +
                           "Score = 8, 10 , 0\r\n" + // ; Raw score of 8 with a maximum possible of 10 and minimum of 0.
                /*
                  * If Score.Raw is accompanied by Score.Max or Score.Min, it reflects the
                     performance of the learner relative to the max and min values.
                     If Score.Max accompanies Score.Raw with no Score.Min, Score.Min is
                     assumed to be “0”.
                     If Score.Min is included then Score.Max must be also be included.
                  */
                           "LessoN_Status = F\r\n" +
                //One of the following vocabulary values: “passed” , “failed”, “complete”,
                //  “incomplete”, “not attempted”, or “browsed”. All values are case
                //  insensitive. Only the first character is significant.

                           /* ***Core.Exit***
                             * 
                                * This element is appended to the keyword/value pair of Lesson_Status
                                    with “,” (comma) preceding it. There may be spaces trailing and leading
                                    this comma. The element value is case-insensitive with only the first
                                    character being significant. If the element is not present, a normal exit
                                    shall be assumed.
                             * 
                             * The value must be one of the following: “time-out” , “logout” , “suspend”
                                    or the empty string (“”).
                             * 
                             * LESSON_Status = Passed, Logout
                                                        */
                           "\r\n" +
                           "\r\n" +
                           "\r\n" +
                           "\r\n" +
                           "[CORE_lesson]\r\n   9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110" +
                           "\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df  " +
                /*
                    * Square brackets “[ ]” are not allowed.
                    * Leading and trailing whitespace (carriage-returns, tabs, spaces)
                    are not included.
                    * Embedded whitespace is allowed and must be included
                */
                           "\r\n" +
                           "[COMMENTS]" +
                           "\r\n" +
                           "<1>The background color is too blue!<1.e><2>The CDU\r\n" +
                           "panel has the incorrect ‘way points’ displayed for\r\n" +
                           "this route. <2.e><3>The CDU panel has the incorrect\r\n" +
                           "‘way points’ displayed for this route. <3.e><4>The\r\n" +
                           "CDU panel has the incorrect ‘way points’ displayed\r\n" +
                           "for this route. <e.4>\r\n"
                ;

            Dictionary<string, string> parsed_dictionary = null;

            HACP_Parser hacpParser = new HACP_Parser();

            parsed_dictionary = hacpParser.parsePutParam("asdfasdf", value);

            Debug.Assert(parsed_dictionary["cmi.core.session_time"] == "1002:34:05", "cmi.core.time is not correct");
            Debug.Assert(parsed_dictionary["cmi.core.lesson_location"] == "end");
            Debug.Assert(parsed_dictionary["cmi.core.score.raw"] == "8");
            Debug.Assert(parsed_dictionary["cmi.core.score.max"] == "10");
            Debug.Assert(parsed_dictionary["cmi.core.score.min"] == "0");
            Debug.Assert(parsed_dictionary["cmi.core.lesson_status"] == "f");
            Debug.Assert(parsed_dictionary["cmi.suspend_data"] == "9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df");
            Debug.Assert(parsed_dictionary["cmi.comments"] == "<1>The background color is too blue!<1.e><2>The CDU\r\npanel has the incorrect ‘way points’ displayed for\r\nthis route. <2.e><3>The CDU panel has the incorrect\r\n‘way points’ displayed for this route. <3.e><4>The\r\nCDU panel has the incorrect ‘way points’ displayed\r\nfor this route. <e.4>");

            value = "[core]\r\nScore = 8, 10\r\n";
            parsed_dictionary = hacpParser.parsePutParam("asdfasdf", value);

            Debug.Assert(parsed_dictionary.ContainsKey("cmi.core.session_time") == false);
            Debug.Assert(parsed_dictionary.ContainsKey("cmi.core.lesson_location") == false);
            Debug.Assert(parsed_dictionary["cmi.core.score.raw"] == "8");
            Debug.Assert(parsed_dictionary["cmi.core.score.max"] == "10");
            Debug.Assert(parsed_dictionary.ContainsKey("cmi.core.score.min") == false);
            Debug.Assert(parsed_dictionary.ContainsKey("cmi.core.lesson_status") == false);
            Debug.Assert(parsed_dictionary.ContainsKey("cmi.suspend_data") == false);
            Debug.Assert(parsed_dictionary.ContainsKey("cmi.comments") == false);

            value = "[core]\r\nlesson_location = 1,,,,,2\r\n";
            parsed_dictionary = hacpParser.parsePutParam("asdfasdf", value);
            
            Debug.Assert(parsed_dictionary["cmi.core.lesson_location"] == "1,,,,,2");

            value = "[core]\r\nlesson_location =Page 1\r\n";
            parsed_dictionary = hacpParser.parsePutParam("asdfasdf", value);
            
            Debug.Assert(parsed_dictionary["cmi.core.lesson_location"] == "Page 1");

            value = "[core]\r\nlesson_location = #$#&^%&^*$Q#)*%afgfg\r\n";
            parsed_dictionary = hacpParser.parsePutParam("asdfasdf", value);

            Debug.Assert(parsed_dictionary["cmi.core.lesson_location"] == "#$#&^%&^*$Q#)*%afgfg");

            value = "[core]\r\nlesson_status = Passed, Logout";
            parsed_dictionary = hacpParser.parsePutParam("asdfasdf", value);

            Debug.Assert(parsed_dictionary["cmi.core.lesson_status"] == "passed");
            //Debug.Assert(parsed_dictionary["cmi.core.exit"] == "Logout");

            value = "[core]\r\nCredit = c";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.credit"] == "c");

            value = "[core]\r\nCredit = Credit";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.credit"] == "credit");

            value = "[core]\r\nCredit =N ";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.credit"] == "n");

            value = "[core]\r\nSCORE= 0.654";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.score.raw"] == "0.654");

            value = "[core]\r\nSCORE=1.3, 2";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.score.raw"] == "1.3");
            Debug.Assert(parsed_dictionary["cmi.core.score.max"] == "2");

            value = "[core]\r\nSCORE= ";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.score.raw"] == "");

            value = "[core]\r\nTime = 00:12:23.3";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.session_time"] == "00:12:23.3");
            
            value = "[core]\r\nTime = 02:34:05";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.session_time"] == "02:34:05");

            value = "[core]\r\nTime = 019:12:23.3";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.session_time"] == "019:12:23.3");

            value = "[core]\r\nLesson_mode = Normal";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.lesson_mode"] == "normal");

            value = "[core]\r\nLesson_MODE = r";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.lesson_mode"] == "r");

            value = "[core]\r\nLESSON_MODE = browse";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.lesson_mode"] == "browse");

            value =
                "[core_lesson]\r\n1BookMark = Some book mark data\r\n2BookMark = Some more book mark data\r\n1StateData = Some state data\r\n2StateData = Some more state data.";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.suspend_data"] ==
                         "1BookMark = Some book mark data\r\n2BookMark = Some more book mark data\r\n1StateData = Some state data\r\n2StateData = Some more state data."
            );

            value =
                "[CORE_Vendor]\r\nLaunchParam1 = Some launch stuff\r\nLaunchParam2 = Some more launch stuff\r\nLaunchParam3 = Some launch stuff";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.launch_data"] ==
                         "LaunchParam1 = Some launch stuff\r\nLaunchParam2 = Some more launch stuff\r\nLaunchParam3 = Some launch stuff");

            value = "[Core]\r\nLESSON_Status=l, S";
            parsed_dictionary = hacpParser.parsePutParam("", value);

            Debug.Assert(parsed_dictionary["cmi.core.exit"] == "s");
        }

        static void JSONTest()
        {
            string json_string =
                "{\"core\":{\"score\":{\"_children\":\"raw, min, max\",\"raw\":\"0\",\"min\":\"0.00000\",\"max\":\"10.00000\"},\"_children\":\"student_id, student_name, lesson_location, credit, lesson_status, entry, score, total_time, lesson_mode, exit, session_time\",\"student_id\":\"27ec8f12-45de-41f6-bc6f-0912a91a5728\",\"student_name\":\"Ammons, Ryan \",\"lesson_location\":\"2\",\"credit\":\"credit\",\"lesson_status\":\"incomplete\",\"exit\":\"\",\"entry\":\"\",\"session_time\":\"00:00:05\",\"total_time\":\"07:50:05.00\",\"lesson_mode\":\"normal\"},\"objectives\":{\"_children\":\"id, score, status\",\"_count\":\"0\"},\"student_data\":{\"_children\":\"attempt_number, lesson_status, score, mastery_score, max_time_allowed, time_limit_action, tries_during_lesson, tries\",\"attempt_number\":\"\",\"mastery_score\":\"\",\"max_time_allowed\":\"\",\"time_limit_action\":\"\",\"tries_during_lesson\":\"\"},\"student_preference\":{\"_children\":\"audio, language, lesson_type, speed, text, text_color, text_location, text_size, video, windows\",\"audio\":\"0\",\"language\":\"\",\"lesson_type\":\"\",\"speed\":\"0\",\"text\":\"0\",\"text_color\":\"\",\"text_location\":\"\",\"text_size\":\"\",\"video\":\"\"},\"interactions\":{\"_children\":\"id, date, time, objectives, type, correct_responses, student_response, result, weighting, latency\",\"_count\":\"0\"},\"evaluation\":{\"comments\":{\"_count\":\"0\",\"_children\":\"content, location, time\"}},\"enrollment_id\":\"71dc1b30-b013-4848-9155-17ae45330ca3\",\"terminate\":\"false\",\"_children\":\"core, suspend_data, launch_data, comments, objectives, student_data, student_preference, interactions\",\"_version\":\"3.4\",\"suspend_data\":\"C1Enone$nP0Enone$nP0Enone$nPA\",\"launch_data\":\"\",\"comments\":\"\",\"comments_from_lms\":\"\"}";

           
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });
            dynamic cmi = jss.Deserialize(json_string, typeof(object)) as dynamic;

            Dictionary<string, string> cmi_dict = cmi.Flatten();

            //foreach (KeyValuePair<string, string> pair in cmi_dict)
            //    Console.WriteLine("Debug.Assert(cmi_dict[\"" + pair.Key + "\"] == \"" + pair.Value + "\";");

            Debug.Assert(cmi_dict["cmi.core.score._children"] == "raw, min, max");
            Debug.Assert(cmi_dict["cmi.core.score.raw"] == "0");
            Debug.Assert(cmi_dict["cmi.core.score.min"] == "0.00000");
            Debug.Assert(cmi_dict["cmi.core.score.max"] == "10.00000");
            Debug.Assert(cmi_dict["cmi.core._children"] == "student_id, student_name, lesson_location, credit, lesson_status, entry, score, total_time, lesson_mode, exit, session_time");
            Debug.Assert(cmi_dict["cmi.core.student_id"] == "27ec8f12-45de-41f6-bc6f-0912a91a5728");
            Debug.Assert(cmi_dict["cmi.core.student_name"] == "Ammons, Ryan ");
            Debug.Assert(cmi_dict["cmi.core.lesson_location"] == "2");
            Debug.Assert(cmi_dict["cmi.core.credit"] == "credit");
            Debug.Assert(cmi_dict["cmi.core.lesson_status"] == "incomplete");
            Debug.Assert(cmi_dict["cmi.core.exit"] == "");
            Debug.Assert(cmi_dict["cmi.core.entry"] == "");
            Debug.Assert(cmi_dict["cmi.core.session_time"] == "00:00:05");
            Debug.Assert(cmi_dict["cmi.core.total_time"] == "07:50:05.00");
            Debug.Assert(cmi_dict["cmi.core.lesson_mode"] == "normal");
            Debug.Assert(cmi_dict["cmi.objectives._children"] == "id, score, status");
            Debug.Assert(cmi_dict["cmi.objectives._count"] == "0");
            Debug.Assert(cmi_dict["cmi.student_data._children"] == "attempt_number, lesson_status, score, mastery_score, max_time_allowed, time_limit_action, tries_during_lesson, tries");
            Debug.Assert(cmi_dict["cmi.student_data.attempt_number"] == "");
            Debug.Assert(cmi_dict["cmi.student_data.mastery_score"] == "");
            Debug.Assert(cmi_dict["cmi.student_data.max_time_allowed"] == "");
            Debug.Assert(cmi_dict["cmi.student_data.time_limit_action"] == "");
            Debug.Assert(cmi_dict["cmi.student_data.tries_during_lesson"] == "");
            Debug.Assert(cmi_dict["cmi.student_preference._children"] == "audio, language, lesson_type, speed, text, text_color, text_location, text_size, video, windows");
            Debug.Assert(cmi_dict["cmi.student_preference.audio"] == "0");
            Debug.Assert(cmi_dict["cmi.student_preference.language"] == "");
            Debug.Assert(cmi_dict["cmi.student_preference.lesson_type"] == "");
            Debug.Assert(cmi_dict["cmi.student_preference.speed"] == "0");
            Debug.Assert(cmi_dict["cmi.student_preference.text"] == "0");
            Debug.Assert(cmi_dict["cmi.student_preference.text_color"] == "");
            Debug.Assert(cmi_dict["cmi.student_preference.text_location"] == "");
            Debug.Assert(cmi_dict["cmi.student_preference.text_size"] == "");
            Debug.Assert(cmi_dict["cmi.student_preference.video"] == "");
            Debug.Assert(cmi_dict["cmi.interactions._children"] == "id, date, time, objectives, type, correct_responses, student_response, result, weighting, latency");
            Debug.Assert(cmi_dict["cmi.interactions._count"] == "0");
            Debug.Assert(cmi_dict["cmi.evaluation.comments._count"] == "0");
            Debug.Assert(cmi_dict["cmi.evaluation.comments._children"] == "content, location, time");
            Debug.Assert(cmi_dict["cmi.enrollment_id"] == "71dc1b30-b013-4848-9155-17ae45330ca3");
            Debug.Assert(cmi_dict["cmi.terminate"] == "false");
            Debug.Assert(cmi_dict["cmi._children"] == "core, suspend_data, launch_data, comments, objectives, student_data, student_preference, interactions");
            Debug.Assert(cmi_dict["cmi._version"] == "3.4");
            Debug.Assert(cmi_dict["cmi.suspend_data"] == "C1Enone$nP0Enone$nP0Enone$nPA");
            Debug.Assert(cmi_dict["cmi.launch_data"] == "");
            Debug.Assert(cmi_dict["cmi.comments"] == "");
            Debug.Assert(cmi_dict["cmi.comments_from_lms"] == "");


        }

        static void HACP_API_Logic_Test()
        {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("cmi.core.lesson_location", "end");
            json.Add("cmi.core.credit", "credit");
            json.Add("cmi.core.lesson_status", "attempted");
            json.Add("cmi.core.score.raw", "8.5");
            json.Add("cmi.core.score.min", "0");
            json.Add("cmi.core.score.max", "10");
            json.Add("cmi.core.session_time", "00:47:00");
            json.Add("cmi.core.total_time", "1:23:00");
            json.Add("cmi.core.lesson_mode", "normal");
            json.Add("cmi.student_data.mastery_score", "8");
            json.Add("cmi.suspend_data", "9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df");
            json.Add("cmi.comments", "<1>The background color is too blue!<1.e><2>The CDU\r\npanel has the incorrect ‘way points’ displayed for\r\nthis route. <2.e><3>The CDU panel has the incorrect\r\n‘way points’ displayed for this route. <3.e><4>The\r\nCDU panel has the incorrect ‘way points’ displayed\r\nfor this route. <e.4>");
            json.Add("cmi.terminate", "true");

            HACP_Logic hacp_logic = new HACP_Logic();
            hacp_logic.ConsumeJSObj(json); // use same method as in JS API, since the HACP parser returns a flattened dict with the same data model (and names) as JS API

            Debug.Assert(((string)hacp_logic.GetValue("cmi.core.lesson_location") == "end"));
            Debug.Assert(((bool)hacp_logic.GetValue("cmi.core.credit")));
            Debug.Assert(((string)hacp_logic.GetValue("cmi.core.lesson_status") == "passed"));
            Debug.Assert(((double)hacp_logic.GetValue("cmi.core.score.raw") == 8.5d));
            Debug.Assert(((double)hacp_logic.GetValue("cmi.core.score.min") == 0d));
            Debug.Assert(((double)hacp_logic.GetValue("cmi.core.score.max") == 10d));
            Debug.Assert(((int)hacp_logic.GetValue("cmi.core.session_time") == 2820));
            //Debug.Assert(((string)hacp_logic.GetValue("cmi.core.total_time", "1:23:00");
            Debug.Assert(((string)hacp_logic.GetValue("cmi.core.lesson_mode") == "normal"));
            Debug.Assert(((double)hacp_logic.GetValue("cmi.student_data.mastery_score") == 8d));
            Debug.Assert(((string)hacp_logic.GetValue("cmi.suspend_data") == "9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df"));
            Debug.Assert(((string)hacp_logic.GetValue("cmi.comments") == "<1>The background color is too blue!<1.e><2>The CDU\r\npanel has the incorrect ‘way points’ displayed for\r\nthis route. <2.e><3>The CDU panel has the incorrect\r\n‘way points’ displayed for this route. <3.e><4>The\r\nCDU panel has the incorrect ‘way points’ displayed\r\nfor this route. <e.4>"));

        }

        private static void AuditTest()
        {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("cmi.core.lesson_location", "end");
            json.Add("cmi.core.credit", "credit");
            json.Add("cmi.core.lesson_status", "attempted");
            json.Add("cmi.core.score.raw", "8.5");
            json.Add("cmi.core.score.min", "0");
            json.Add("cmi.core.score.max", "10");
            json.Add("cmi.core.session_time", "00:47:00");
            json.Add("cmi.core.total_time", "1:23:00");
            json.Add("cmi.core.lesson_mode", "normal");
            json.Add("cmi.student_data.mastery_score", "8");
            json.Add("cmi.suspend_data",
                     "9 00 001010101100110\r\n000 001010101100110\r\n000001010101100110\r\ngl’;sdfgl’;sdfhgl’;sdfhgls’;df");
            json.Add("cmi.comments",
                     "<1>The background color is too blue!<1.e><2>The CDU\r\npanel has the incorrect ‘way points’ displayed for\r\nthis route. <2.e><3>The CDU panel has the incorrect\r\n‘way points’ displayed for this route. <3.e><4>The\r\nCDU panel has the incorrect ‘way points’ displayed\r\nfor this route. <e.4>");
            json.Add("cmi.terminate", "true");

            HACP_Logic hacp_logic = new HACP_Logic();
            hacp_logic.ConsumeJSObj(json);
                // use same method as in JS API, since the HACP parser returns a flattened dict with the same data model (and names) as JS API

            string enrollment_id = "71dc1b30-b013-4848-9155-17ae45330ca3";

            using (var ctx = new ComplianceFactorsEntities())
            {
                var enroll = (from en in ctx.e_tb_enrollments
                              where en.e_enroll_system_id_pk == new Guid(enrollment_id)
                              select en).FirstOrDefault();

                int? total_time = enroll.e_enroll_time_spent;

                hacp_logic.ConsumeJSObj(json);

                hacp_logic.Persist(enrollment_id);

                ctx.Refresh(System.Data.Objects.RefreshMode.StoreWins, enroll);

                // TODO: add object id field to audit table for association


            }
        }

        private static void RecurrenceEnrollmentTest()
        {
            // TODO: must create recurring course/delivery and associated enrollment for this test
        }
    }
}