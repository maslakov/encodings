using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings.Log
{
    /// <summary>
    /// Log local cache item
    /// </summary>
    public class LogItem
    {
        /// <summary>
        /// log text representation
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// flag: saved item to file or not
        /// </summary>
        public bool Saved { get; set; }
    }
}
