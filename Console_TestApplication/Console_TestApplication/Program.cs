using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using LMS_Prototype_1;
using System.Diagnostics;


namespace Console_TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            HACPParserTest();
            BaseLogicTest();
        }

        static void BaseLogicTest()
        {
            Dictionary<string, string> json = new Dictionary<string,string>();
            json.Add("cmi.core.lesson_location", "end");
            json.Add("cmi.core.credit", "credit");
            json.Add("cmi.core.lesson_status", "attempted");
            json.Add("cmi.core.score.raw", "8.5");
            json.Add("cmi.core.score.min", "0");
            json.Add("cmi.core.score.max", "10");
            json.Add("cmi.core.session_time", "47:00");
            json.Add("cmi.core.total_time", "1:23:00");
            json.Add("cmi.core.lesson_mode", "normal");
            json.Add("cmi.student_data.mastery_score", "8");

            AICC_CMI.BaseLogic baselogic = new AICC_CMI.BaseLogic();
            baselogic.ConsumeJSObj(json);

            Debug.Assert(((string)baselogic.GetValue("cmi.core.lesson_location") == "end"));
            Debug.Assert(((string)baselogic.GetValue("cmi.core.credit") == "credit"));
            Debug.Assert(((string)baselogic.GetValue("cmi.core.lesson_status") == "passed"));
            Debug.Assert(((double)baselogic.GetValue("cmi.core.score.raw") == 8.5d));
            Debug.Assert(((double)baselogic.GetValue("cmi.core.score.min") == 0d));
            Debug.Assert(((double)baselogic.GetValue("cmi.core.score.max") == 10d));
            //Debug.Assert(((TimeSpan)baselogic.GetValue("cmi.core.session_time") == new TimeSpan(0, 47, 00)));
            //Debug.Assert(((string)baselogic.GetValue("cmi.core.total_time", "1:23:00");
            Debug.Assert(((string)baselogic.GetValue("cmi.core.lesson_mode") == "normal"));
            Debug.Assert(((double)baselogic.GetValue("cmi.student_data.mastery_score") == 8d));
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

            Console.Write("press any key to continue...");
            Console.ReadKey(false);
        }

    }
}