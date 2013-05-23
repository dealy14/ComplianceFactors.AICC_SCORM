using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMS_Prototype_1
{
    public class StringTokenizer
    {
        private string[] tokens;
        private int count=0,index=0;
        private char delim;
        
	    /**
	     * Constructs a string tokenizer for the specified string
	     * @param string $str String to tokenize
	     * @param string $delim The set of delimiters (the characters that separate tokens)
	     * specified at creation time, default to ' '
	     */
	    public StringTokenizer(string s, char d=' ')
	    {
		    this.tokens = s.Split(d);
            this.count = tokens.Length;
            this.index = 0;
	    }


	    /**
	     * Tests if there are more tokens available from this tokenizer's string. It
	     * does not move the internal pointer in any way. To move the internal pointer
	     * to the next element call nextToken()
	     * @return boolean - true if has more tokens, false otherwise
	     */
	    public bool hasMoreTokens()
	    {
		    return (index < count);
	    }

	    /**
	     * Returns the next token from this string tokenizer and advances the internal
	     * pointer by one.
	     * @return string - next element in the tokenized string
	     */
	    public string nextToken()
	    {
		    index++; //increment position
            if (index >= count)
                return "";
            else
                return tokens[this.index-1];
	    }

    }
}