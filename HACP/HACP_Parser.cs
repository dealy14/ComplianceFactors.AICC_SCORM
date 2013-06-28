using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using Parsers;

namespace LMS_Prototype_1
{
    public class HACP_Parser
    {
        private static readonly string cmi_core_score_raw = "cmi.core.score.raw";
        private static readonly string cmi_core_score_max = "cmi.core.score.max";
        
        protected Dictionary<string, string> dMapping = new Dictionary<string, string>();

        public HACP_Parser()
        {
            initializeMapping();
        }

        public Dictionary<string,string> parsePutParam(string sess_id, string data)
        {
            List<string> sBlockElements = new List<string> { "core_lesson", "core_vendor", "comments" }; //add all block-data sections
            //string sKeyValueElements = "core"; //all key-value data sections (might not need--> if !block, then key-value)

            ArrayList arTokens = new ArrayList();
            Dictionary<string, string> blocks = new Dictionary<string, string>();

            Parser.Parse(data, arTokens, blocks, true);

            Dictionary<string, string> dKeyValuePairs = new Dictionary<string, string>();

            foreach (var block in blocks)
            {

                if (sBlockElements.Contains(block.Key)) // is a block of data (contiguous string)
                {
                    dKeyValuePairs.Add(dMapping[block.Key.ToLower().Trim()], block.Value.Trim(new char[] { '\r', '\n', '\t', ' ' }));
                }
                else // is a key-value pair section
                {
                    //parse key-value pairs
                    arTokens = new ArrayList();

                    // separate each key value pair (by line)
                    string[] keyValuePairs = block.Value.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] kvPair; //temp


                    foreach (var keyValuePair in keyValuePairs)
                    {
                        kvPair = keyValuePair.Split('='); //parse key and value
                        kvPair[0] = kvPair[0].ToLower().Trim();
                        // Not using ToLower on kvPair[1] because some elements must maintain original case

                        //Console.WriteLine("[" + block.Key.ToUpper() + "]" +
                        //                "\tKEY: " + kvPair[0]
                        //                + "\t\tVALUE: " + kvPair[1]);

                        if ("score" == kvPair[0])
                        {
                            string[] scoreParts = kvPair[1].ToLower().Split(',');
                            int i = 0;
                            foreach (var scorePart in scoreParts)
                            {
                                if (0 == i)
                                    dKeyValuePairs.Add(cmi_core_score_raw, scorePart.Trim());
                                else if (1 == i)
                                    dKeyValuePairs.Add(cmi_core_score_max, scorePart.Trim());
                                else
                                    dKeyValuePairs.Add("cmi.core.score.min", scorePart.Trim());

                                i++;
                            }
                        }
                        else if ("lesson_status" == kvPair[0])
                        {
                            string[] lesson_statuses = kvPair[1].ToLower().Split(',');
                            dKeyValuePairs.Add("cmi.core.lesson_status", lesson_statuses[0].Trim());
                            if (lesson_statuses.Length > 1)
                            {
                                dKeyValuePairs.Add("cmi.core.exit", lesson_statuses[1].Trim());
                            }
                        }
                        else if ("lesson_location" == kvPair[0])
                        {
                            dKeyValuePairs.Add(dMapping[kvPair[0]], kvPair[1].Trim());
                        }
                        else
                        {
                            kvPair[1] = kvPair[1].ToLower().Trim();
                            dKeyValuePairs.Add(dMapping[kvPair[0]], kvPair[1]); //insert key and value into dictionary
                        }
                    }

                }
            }

            return dKeyValuePairs;

        }


        private void initializeMapping()
        {
            // initialize the dictionary mapping
            dMapping.Add("lesson_location", "cmi.core.lesson_location");
            dMapping.Add("credit", "cmi.core.credit");
            dMapping.Add("lesson_mode", "cmi.core.lesson_mode");
            
            // l, t, s, a, r OR ""
            // l, t, s => core.exit {logout,timeout,suspend, ""} 
            // a, r => core.entry  {ab-initio,resume,""}

            // If received from the course (in HACP, PutParam), then second value is core.exit
            // It is never received as core.entry---this is only an outgoing value to the course via GetParam
            // In the JS API, the values have distinct keys.
            dMapping.Add("lesson_status", "cmi.core.lesson_status"); // lesson_status, [core.exit OR core.entry] (split by ',')

            //dMapping.Add("score", "-");
            //dMapping.Add("-", cmi_core_score_raw);
            //dMapping.Add("-", "cmi.core.score.min");
            //dMapping.Add("-", "cmi.core.score.max");

            dMapping.Add("time", "cmi.core.session_time");
            //dMapping.Add("-", "cmi.core.session_time");
            //dMapping.Add("-", "cmi.core.total_time");

            dMapping.Add("core_lesson", "cmi.suspend_data");
            dMapping.Add("core_vendor", "cmi.launch_data");
            //dMapping.Add("-", "cmi.core.exit");
            //dMapping.Add("-", "cmi.core.entry");

            dMapping.Add("comments", "cmi.comments");

        }


    }
}