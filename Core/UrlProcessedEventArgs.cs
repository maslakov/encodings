using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings
{
    /// <summary>
    /// Completed notification argument
    /// </summary>
    public class UrlProcessedEventArgs : EventArgs
    {
        /// <summary>
        /// Completed time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Thread identity
        /// </summary>
        public String ThreadName { get; set; }

        ProcessingResult _result;

        /// <summary>
        /// Result of URL processing
        /// </summary>
        public ProcessingResult Result { get { return _result; } }

        public UrlProcessedEventArgs(ProcessingResult result)
        {
            _result = result;
        }
    }
}
