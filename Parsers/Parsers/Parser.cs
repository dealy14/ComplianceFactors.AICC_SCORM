using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Parsers
{
    public class Parser
    {
        public Parser()
        {
        }

        public static void Parse(string strInput, ArrayList arTokens, Dictionary<string,string> Blocks,
                             bool bUnEsc)
        {
            char cEsc = '\\';
            //ArrayList arTokens = new ArrayList();
            StringBuilder sbToken = new StringBuilder();
            bool bValue = false;

            for (int i = 0; i < strInput.Length; ++i)
            {
                // If the character indicates opening of a block []
                if ('[' == strInput[i])
                {
                    if (true == bValue) //indicates end of a previous block, so terminate token
                    {
                        arTokens.Add(sbToken.ToString().Trim('\r', '\n', ' '));
                        sbToken.Length = 0;
                    }

                    bValue = false;
                }//if

                // If it is the close bracket
                else if (']' == strInput[i])
                {
                    bValue = true;
                    arTokens.Add(sbToken.ToString().Trim().ToLower());
                    sbToken.Length = 0;
                } //else if

                else
                {
                    sbToken.Append(strInput[i]);
                }//else
                    
            }//for

            arTokens.Add(sbToken.ToString().Trim('\r','\n',' '));

            for (int i = 0; i < arTokens.Count; i = i + 2)
            {
                Blocks.Add(arTokens[i].ToString(), arTokens[i + 1].ToString());
                // key, value
            }
        }
    }
}