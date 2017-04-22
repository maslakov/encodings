using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings.Parser
{
    /// <summary>
    /// structure to save the data about text regions
    /// </summary>
    public struct Markup
    {
        public int Start { get; set; }
        public int End { get; set; }
        public bool IsValid { get { return End >= 0 && Start >= 0 && Start <= End; } }

        public string GetFromString(String source)
        {
            if (source.Length >= this.Start && source.Length >= this.End)
                return source.Substring(this.Start, this.End - this.Start + 1);
            else return String.Empty;
        }
    }

    /// <summary>
    /// Bindings for property parts: key and value
    /// </summary>
    public struct Prop
    {
        public Markup KeyMarkup { get; set; }
        public Markup ValMarkup { get; set; }
    }

    /// <summary>
    /// Tag scope data
    /// </summary>
    public class TagMarkup
    {
        /// <summary>
        /// tag bounds
        /// </summary>
        public Markup TagMarks { get; set; }

        /// <summary>
        /// inner tag properties bounds
        /// </summary>
        public List<Prop> Props { get; set; }


        public TagMarkup()
        {
            Props = new List<Prop>();
        }
    }

}
