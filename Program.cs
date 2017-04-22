using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Encodings.Parser;

namespace Encodings
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //logger
            Log.ILogger Logger = Log.FileLogger.Instance;

            //loading params
            RunParam runParams = null;

            try
            {
                UrlProcessorsManager processor = null;

                //get loading params
                runParams = ParamsHelper.GetParams(args);

                //create processor object and read data from file
                processor = new UrlProcessorsManager(runParams.InputFileName);
                    
                //process data with number of threads
                processor.Process(runParams.ThreadsNumber);

                //save result to the file
                processor.WriteResultFile(runParams.OutputFileName);

                Logger.Log(String.Format("{0}>> Processing done", DateTime.Now.ToString("u")));

                Console.WriteLine("done.... See {0} for results", runParams.OutputFileName);
                Console.WriteLine("See log in {0}", ((Log.FileLogger)Logger).FileName);
                Console.WriteLine("Press ENTER");
                Console.ReadLine();

            }
            catch (Exception e)
            {
                Logger.Log(e.Message);

                Console.WriteLine("Runtime Error: {0}",e.Message);
                Console.WriteLine("Press ENTER");
                Console.ReadLine();
            }

            Logger.Log(String.Format("{0}>> Application Stop", DateTime.Now.ToString("u")));

            Logger.Dispose();

        }
    }
}
