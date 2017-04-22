using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings.Parser
{
    /// <summary>
    /// class for parsing of the content property in meta tag
    /// </summary>
    public static class MetaContentPropertyParser
    {
        /// <summary>
        /// parse property in meta tag
        /// </summary>
        /// <param name="stringToParse">page</param>
        /// <param name="start">text begin point</param>
        /// <param name="end">text end point</param>
        /// <returns>list of properties markup points</returns>
        public static List<Prop> GetContentProperties(string stringToParse, int start, int end)
        {
            List<Prop> result = new List<Prop>();
            int i = start;
            int prev = -1;
            //on every step need to move forward. if no move - nothing to parse
            while (i < end && prev < i)
            {
                Markup keyMarkup = new Markup { Start = -1, End = -1 };
                Markup valueMarkup = new Markup { Start = -1, End = -1 }; 
                int endIdx = 0;
                //try get key-value pair
                bool propResult = KeyValueParser.OuterTryParse(stringToParse, i, end, out keyMarkup, out valueMarkup, out endIdx);
                if (propResult)
                {
                    //save result
                    result.Add(new Prop { KeyMarkup = keyMarkup, ValMarkup = valueMarkup });
                }
                //moved by one step
                prev = i;
                i = endIdx;
            }

            return result;
        
        }
    }



}
