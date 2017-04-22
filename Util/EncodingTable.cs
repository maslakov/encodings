using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Encodings.Util
{
    /// <summary>
    /// encodings lookup table
    /// </summary>
    public class EncodingTable
    {
        /// <summary>
        /// file to use
        /// </summary>
        private string _tableFileName;

        /// <summary>
        /// local encodings cache
        /// </summary>
        Dictionary<String, String> _encodings;

        /// <summary>
        /// create singletone lookup table for encodings lookup
        /// </summary>
        /// <param name="fileName">file name</param>
        public EncodingTable(String fileName)
        {
            _tableFileName = fileName;
            _encodings = new Dictionary<string,string>();

            try
            {
                //read lookup table from file
                using (System.IO.StreamReader sr = new System.IO.StreamReader(_tableFileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string fileContents = sr.ReadLine();
                        if (!String.IsNullOrEmpty(fileContents))
                        {
                            //add to dictionary
                            string[] parts = fileContents.Split(new char[] { '\t' });
                            if (parts.Length >= 2)
                            {
                                _encodings[parts[0].Trim()] = parts[1].Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //logging is on level-up
                throw;
            }
        }

        /// <summary>
        /// singleton implementation
        /// </summary>
        public static EncodingTable Instance { get { return InstanceHolder._instance; } }

        /// <summary>
        /// singleton handler
        /// </summary>
        private class InstanceHolder
        {
            //default filename
            static string defaultTableFileName = "encodings.txt";

            static InstanceHolder()
            {
                //read log file name from config
                string tableFileName = ConfigurationManager.AppSettings["tableFileName"];

                if (String.IsNullOrEmpty(tableFileName))
                    tableFileName = defaultTableFileName;

                _instance = new EncodingTable(tableFileName);
            }

            internal static readonly EncodingTable _instance;
        }

        /// <summary>
        /// Try to find right encoding name in lookup table
        /// </summary>
        /// <param name="badEncoding">bad encoding name</param>
        /// <param name="rightEncoding">right encoding name</param>
        /// <returns>was found or not</returns>
        public bool TryFindEncoding(string badEncoding, out string rightEncoding)
        {
            if (_encodings.TryGetValue(badEncoding, out rightEncoding))
                return true;
            return false;
        }
    }
}
