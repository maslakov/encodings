using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings
{
    /// <summary>
    /// Error notify argument
    /// </summary>
    public class UrlErrorEventArgs : EventArgs
    {
        #region fields
        private string _tread;
        private string _error;
        private DateTime _time;

        #endregion

        /// <summary>
        /// Thread identity
        /// </summary>
        public string ThreadName
        {
            get { return _tread; }
            set { _tread = value; }
        }

        /// <summary>
        /// Time when error occured
        /// </summary>
        public DateTime ErrorTime
        {
            get { return _time; }
            set { _time = value; }
        }

        /// <summary>
        /// Error message text
        /// </summary>
        public string ErrorMessage
        {
            get { return _error; }
            set { _error = value; }
        }

    }
}
