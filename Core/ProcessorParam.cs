using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings
{
    /// <summary>
    /// Parameter to pass in Thread
    /// </summary>
    public class ProcessorParam
    {
        List<string> _urls;

        /// <summary>
        /// List of URLs to processing
        /// </summary>
        public List<string> UrlsToProcess 
        {
            get { return _urls; }
            set { _urls = value; }
        }

        public ProcessorParam()
        {
            _urls = new List<string>();
        }
    }
}
