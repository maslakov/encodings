using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings
{
    /// <summary>
    /// Result of page processing
    /// </summary>
    public class ProcessingResult
    {
        /// <summary>
        /// Page address
        /// </summary>
        public String Url { get; set; }

        /// <summary>
        /// Retrieved encodings
        /// </summary>
        public List<String> Encodings { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var en in this.Encodings)
            {
                sb.AppendFormat("{0}\t{1}\n", this.Url, en);
            }

            return sb.ToString();
        }
    }
}
