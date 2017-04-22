using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings
{
    /// <summary>
    /// Application run parameters
    /// </summary>
    public class RunParam
    {
        /// <summary>
        /// File with initial data
        /// </summary>
        public String InputFileName { get; set; }

        /// <summary>
        /// Number of threads
        /// </summary>
        public Int32 ThreadsNumber { get; set; }


        /// <summary>
        /// File with result data
        /// </summary>
        public String OutputFileName { get; set; }
    }
}
