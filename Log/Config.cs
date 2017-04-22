using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Encodings.Log
{
    /// <summary>
    /// Logger config section in *.config 
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// section name
        /// </summary>
        public static string LoggerSection
        {
            get { return "logger"; }
        }
    }


    public class LoggerSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("LogOutput",
            IsRequired = true)]
        public FileNameElement OutputFile
        {
            get { return (FileNameElement)this["LogOutput"]; }
            set { this["LogOutput"] = value; }
        }

       

    }


    public class FileNameElement : System.Configuration.ConfigurationElement
    {
        /// <summary>
        /// file name to save log items
        /// </summary>
        [ConfigurationProperty("fileName",
            DefaultValue = "",
            IsRequired = true)]
        public string FileName
        {
            get { return (string)this["fileName"]; }
            set { this["fileName"] = value; }
        }

        /// <summary>
        /// Timer quantity in milliseconds to save cache in file
        /// </summary>
        [ConfigurationProperty("savetimer",
            IsRequired = false)]
        public Int32 TimerQty
        {
            get { return (Int32)this["savetimer"]; }
            set { this["savetimer"] = value; }
        }

    }
}
