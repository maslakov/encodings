using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;

namespace Encodings.Log
{
    /// <summary>
    /// Logger. Use file to save log items.
    /// </summary>
    /// <remarks>log items stored in cache and saved in file by timer</remarks>
    public class FileLogger : ILogger, IDisposable
    {
        private List<LogItem> _logItems;
        private string _logFileName;
        Object lock_obj = new Object();
        Timer timer = null;

        public String FileName { get { return _logFileName; } }

        /// <summary>
        /// Create new file-based logger
        /// </summary>
        /// <param name="logFileName">log-file name</param>
        private FileLogger(string logFileName, int timerQty)
        {
            _logFileName = logFileName;
            _logItems = new List<LogItem>();
            try
            {
                System.IO.File.Delete(logFileName);
            }
            catch (Exception)
            {
                Console.WriteLine("Log error. Cannot delete old log file");                
            }

            //cache saver
            timer = new Timer(TimerBufferSave, null, 0, timerQty);
        }

        /// <summary>
        /// save unsaved log-items to file using timer
        /// </summary>
        /// <param name="state"></param>
        private void TimerBufferSave(Object state)
        {
            SaveLogsToFile();
        }

        /// <summary>
        /// singleton implementation
        /// </summary>
        public static FileLogger Instance { get { return InstanceHolder._instance; } }

        /// <summary>
        /// singleton handler
        /// </summary>
        private class InstanceHolder
        {
            //default log filename
            static string defaultLogFileName = "log.txt";
            static int defaultTimerQtt = 500;
            static string logFileName;

            static InstanceHolder() {
                //read log file name from config
                logFileName = ConfigurationManager.AppSettings["logFileName"];
                LoggerSection config = (LoggerSection)ConfigurationManager.GetSection(Settings.LoggerSection);
                //timer qtty. default 500
                int qtt = defaultTimerQtt;

                if (config != null)
                {
                    ///read config
                    logFileName = config.OutputFile.FileName;
                    qtt = config.OutputFile.TimerQty > 0 ? config.OutputFile.TimerQty : defaultTimerQtt;
                }
                else
                    logFileName = ConfigurationManager.AppSettings["logFileName"];

                if (String.IsNullOrEmpty(logFileName))
                    logFileName = defaultLogFileName;

                

                _instance = new FileLogger(logFileName, qtt);
            }

            internal static readonly FileLogger _instance;
        }


        /// <summary>
        /// Save log message
        /// </summary>
        /// <param name="message">message to log</param>
        public void Log(string message)
        {
            _logItems.Add(new LogItem() { Message = message, Saved = false });
        }


        /// <summary>
        /// write unsaved logs to log file
        /// </summary>
        private void SaveLogsToFile()
        {
            lock(_logItems)
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(_logFileName, true))
                {
                    foreach (var log in _logItems.Where(l => !l.Saved))
                    {
                        sw.WriteLine(log.Message);
                        log.Saved = true;
                    }
                }
            }
        }


        #region cleaning

        bool _disposed = false;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    lock (lock_obj)
                    {
                        //free timer
                        timer.Dispose();
                        
                        //save unsaved
                        SaveLogsToFile();
                    }
                }

                _disposed = true;
            }
        }


        #endregion    

    }
}
