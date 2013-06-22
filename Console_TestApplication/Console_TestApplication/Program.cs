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
            //Debug.Assert(); //...

            Console.Write("press any key to continue...");
            Console.ReadKey(false);
        }

    }
}