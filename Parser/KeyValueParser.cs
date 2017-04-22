using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings.Parser
{
    /// <summary>
    /// parser for strings like key=value
    /// </summary>
    public static class KeyValueParser
    {
        //states of determ. automata
        private enum kvStates {
            Start = 0, 
            Key = 1,
            Delimiter = 2,
            DQuotaValue = 3, 
            QuotaValue = 4, 
            Value = 5, 
            End = 6, 
            Space = 7, 
            Error = -1};

        //automata transitions
        private static int[,] DA = new int[8,5] 
        {
            { 1,0,-1,-1,-1},
            { 1,7, 2,-1,-1},
            { 5,2, 2, 3, 4},
            { 3,3, 3, 6, 3},
            { 4,4, 4, 4, 6},
            { 5,6,-1,-1,-1},
            { 6,6, 6, 6, 6},
            {-1,7, 2,-1,-1}
        };
        
        //key value possible delimiters
        public static char[] KeyValueDelimiters = {'='};
        
        //input signals
        private static int GetInputIndex(char c)
        {
            if (Char.IsWhiteSpace(c)) return 1;
            if (c == ';') return 1;
            if (Array.IndexOf(KeyValueDelimiters, c) >= 0) return 2;
            if (c == '"') return 3;
            if (c == '\'') return 4;
            if (Char.IsSeparator(c)) return 1;
            if (c == '\n') return 1;
            if (c == '\r') return 1;

            //if (Char.IsLetterOrDigit(c)) return 0;

            return 0;
        }

        /// <summary>
        /// parse string, that represented key-value pair
        /// </summary>
        /// <param name="stringToParse">key=value string</param>
        /// <param name="key">key string</param>
        /// <param name="value">value string</param>
        /// <returns>is successful</returns>
        public static bool TryParse(string stringToParse, out string key, out string value)
        {
            return OuterTryParse(stringToParse, 0, stringToParse.Length - 1, out key, out value);
        }


        /// <summary>
        /// parse string, that represented key-value pair
        /// </summary>
        /// <param name="stringToParse">key=value string</param>
        /// <param name="key">key string</param>
        /// <param name="value">value string</param>
        /// <returns>is successful</returns>
        public static bool OuterTryParse(string stringToParse, int start, int end, out string key, out string value)
        {
            Markup keyMarkup;
            Markup valueMarkup;
            int endIdx = 0;
            bool result = OuterTryParse(stringToParse, 0, stringToParse.Length - 1, out keyMarkup, out valueMarkup, out endIdx);
            if (result)
            {
                key = stringToParse.Substring(keyMarkup.Start, keyMarkup.End - keyMarkup.Start + 1);
                value = stringToParse.Substring(valueMarkup.Start, valueMarkup.End - valueMarkup.Start + 1);
            }
            else
            {
                key = String.Empty;
                value = String.Empty;
            }
            return result;
        }


        /// <summary>
        /// parse string, that represents key-value pair
        /// </summary>
        /// <param name="stringToParse">key=value string</param>
        /// <param name="key">key string</param>
        /// <param name="value">value string</param>
        /// <returns>is successful</returns>
        public static bool OuterTryParse(string stringToParse, int start, int end, out Markup keyMarkup, out Markup valueMarkup, out int ExitIndex)
        {
            //init state
            int keyStart = 0, keyEnd = 0, valStart = 0, valEnd = 0;
            kvStates State = kvStates.Start;
            kvStates prevState = kvStates.Start;
            keyMarkup = new Markup { Start = -1, End = -1 };
            valueMarkup = new Markup { Start = -1, End = -1 };
            ExitIndex = start;

            int i = start;
            while (i <= end && State != kvStates.End && State != kvStates.Error)
            {
                //fire automata
                prevState = State;
                int signal = GetInputIndex(stringToParse[i]);
                if (signal < 0) { State = kvStates.Error; break; }
                State = (kvStates)DA[(int)State, signal];

                //was switch - move coordinates to the memory
                if (prevState != State)
                {
                    if (State == kvStates.Key) keyStart = i;
                    else
                        if (prevState == kvStates.Key) keyEnd = i - 1;
                        else
                            if (State == kvStates.DQuotaValue
                                || State == kvStates.QuotaValue
                                || State == kvStates.Value)
                                valStart = i;
                            else
                                if (State == kvStates.End)
                                {
                                    if(prevState != kvStates.Value) valStart++;
                                    valEnd = i-1;
                                    ExitIndex = i;
                                }
                }

                i++;
            }

            if (ExitIndex == start)
                ExitIndex = i-1;

            //abnormal end
            if (State != kvStates.End && State!= kvStates.Value)
                    return false;
            
            //last symbol was only swithcer? not part of value
            if (State == kvStates.Value)
                valEnd = i-1;


            keyMarkup.Start =  keyStart;
            keyMarkup.End = keyEnd;
            valueMarkup.Start = valStart;
            valueMarkup.End = valEnd;

            return true;
        }

    }
}
