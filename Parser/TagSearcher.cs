using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings.Parser
{
    /// <summary>
    /// open tag with parameters searcher
    /// </summary>
    public class SimpleTagSearcher
    {
        // DA states
        private enum TagStates
        {
            Outside = 0, StartTag = 1, Space = 2, TagName = 3,
            Property = 4, EndTag = 5, EndTagSlash = 6, SpaceDelimiter = 7, EndSpace = 8,
            Error = -1
        };

        //DA transitions
        private int[,] DA = new int[9, 6] 
        {
            {0,0, 1, 0, 0, 0},
            {3,2,-1,-1,-1,-1},
            {3,2,-1,-1,-1,-1},
            {3,7,-1, 6, 5,-1},
            {4, 7,-1, 6,5,-1},
            {0, 0, 1, 0,0, 0},
            {-1,8,-1,-1,5,-1},
            {4 ,7,-1, 6,5,-1},
            {-1,8,-1,-1,5,-1}
        };

        private string _html;
        private int startScope = 0;
        private int endScope = 0;

        //input signals
        private static int GetInputIndex(char c)
        {
            if (Char.IsLetterOrDigit(c)) return 0;
            if (Char.IsWhiteSpace(c)) return 1;
            if (c == '<') return 2;
            if (c == '/') return 3;
            if (c == '>') return 4;
            if(Char.IsSeparator(c)) return 1;
            if (c == '\n') return 1;
            if (c == '\r') return 1;
            return 5;
        }

        public SimpleTagSearcher(string html)
        {
            _html = html;
            startScope = 0;
            endScope = html.Length - 1;
        }

        /// <summary>
        /// Search simple open tags in text
        /// </summary>
        /// <param name="tagName">tag name</param>
        /// <returns>list of text coordinates</returns>
        public List<TagMarkup> SearchTag(string tagName)
        {
            List<TagMarkup> results = new List<TagMarkup>();
            //init state
            TagStates State = TagStates.Outside;
            TagStates prevState = TagStates.Outside;

            int tagStart = startScope, tagEnd = startScope;
            int propStart = startScope, propEnd = startScope;

            TagMarkup currentTag = null;

            int i = startScope;
            while (i <= endScope)
            {
                prevState = State;
                //fire automata
                int signal = GetInputIndex(_html[i]);
                if (signal < 0) break;
                State = (TagStates)DA[(int)State, signal];

                //state was changed - can get some data: tags or properties
                if (prevState != State)
                {
                    //if space - can get tag
                    if (State == TagStates.SpaceDelimiter)
                    {
                        if (prevState == TagStates.TagName)
                        {
                            tagEnd = i-1;
                            //save tag
                            if (currentTag != null)
                                results.Add(currentTag);
                            //save only target tags
                            if (_html.Substring(tagStart, tagEnd - tagStart + 1).ToLower() != tagName.ToLower())
                            {
                                currentTag = null;
                                //move to next open tag
                                while (i <= endScope && _html[i] != '<' ) i++;
                                State = TagStates.Outside;
                                prevState = TagStates.Outside;
                                continue;
                            }
                            else
                            {
                                currentTag = new TagMarkup();
                                currentTag.TagMarks = new Markup { Start = tagStart, End = tagEnd };
                            }
                        }
                    }else
                        if (State == TagStates.TagName)
                        {
                            tagStart = i;
                        }else
                            //property has started - try red it like key=value pair
                            if (State == TagStates.Property)
                            {
                                propStart = i;
                                Markup keyMarkup = new Markup { Start = -1, End = -1 };
                                Markup valueMarkup = new Markup { Start = -1, End = -1 }; 
                                int endIdx = 0;
                                //using of keyvalueparser common form
                                bool propResult = KeyValueParser.OuterTryParse(_html, i, endScope, out keyMarkup, out valueMarkup, out endIdx);
                                if (propResult)
                                {
                                    if (currentTag != null)
                                    {
                                        currentTag.Props.Add(new Prop { KeyMarkup = keyMarkup, ValMarkup = valueMarkup });
                                    }

                                    i = endIdx+1;
                                    prevState = State;
                                    State = TagStates.SpaceDelimiter;
                                    continue;
                                }
                                else
                                    State = TagStates.Error;
                            }

                    //go to next open tag
                    if (State == TagStates.Error)
                    {
                        while (i <= endScope && _html[i] != '<' ) i++;
                        State = TagStates.Outside;
                        prevState = TagStates.Outside;

                        continue;
                    }

                }

                i++;
            }

            if (currentTag != null)
                results.Add(currentTag);

            return results;
        }

    }
}
