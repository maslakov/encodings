using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Encodings
{
    class UrlProcessorsManager
    {
        #region fields
        
        /// <summary>
        /// List of urls
        /// </summary>
        private List<string> _urlList;

        /// <summary>
        /// work results
        /// </summary>
        private List<ProcessingResult> _results;

        #endregion

        /// <summary>
        /// Logger
        /// </summary>
        Log.ILogger Logger = Log.FileLogger.Instance;

        /// <summary>
        /// Create object, that will manage parsing of pages
        /// </summary>
        /// <param name="inputFileName">input data file name</param>
        public UrlProcessorsManager(string inputFileName)
        {
            _urlList = new List<string>();
            _results = new List<ProcessingResult>();

            try
            {
                //read input file
                using (System.IO.StreamReader sr = new System.IO.StreamReader(inputFileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string url = sr.ReadLine();
                        if (!String.IsNullOrEmpty(url))
                            _urlList.Add(url);
                    }
                }

            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                return;
            }

        }

        /// <summary>
        /// Handler for processed URLs
        /// </summary>
        protected void OnUrlProcessed(object sender, UrlProcessedEventArgs e)
        {
            _results.Add(e.Result);
            
            Logger.Log(String.Format("{0}>> Completed  by thread {1}: {2}", new object[] { e.Time.ToString("u"), e.ThreadName, e.Result.Url }));
            
            Console.WriteLine(e.Result.Url + ":" + String.Join(" ", e.Result.Encodings));
        }

        /// <summary>
        /// Error handler
        /// </summary>
        protected void OnProcessError(object sender, UrlErrorEventArgs e)
        {
            Logger.Log(String.Format("{0}>>Error by thread {1}: {2}", new object[] { e.ErrorTime.ToString("u"), e.ThreadName, e.ErrorMessage }));
            Console.WriteLine("ERROR:{0}", e.ErrorMessage);
        }

        /// <summary>
        /// Start processing of pages in threads
        /// </summary>
        /// <param name="threadsCount">number of threads to use</param>
        public void Process(int threadsCount)
        {
            List<ProcessorParam> opParams = new List<ProcessorParam>();
            
            //number of url on one thread
            int packageCapacity = _urlList.Count / threadsCount;

            if (packageCapacity == 0)
                packageCapacity = _urlList.Count;
            
            int threadNum = 1;
            ProcessorParam newParam = new ProcessorParam();

            //Distribute urls by threads
            for (int i = 0; i < _urlList.Count; i++)
            {
                newParam.UrlsToProcess.Add(_urlList[i]);

                if (i == threadNum * packageCapacity - 1)
                {
                    opParams.Add(newParam);

                    if (threadNum < threadsCount)
                    {
                        threadNum++;
                        newParam = new ProcessorParam();
                    }
                }
            }

            //using countdown sychronizer object
            var finished = new CountdownEvent(0);

            for (int i = 0; i < opParams.Count; i++)
            {
                finished.AddCount(1);
                Thread t = new Thread(
                    delegate(object p)
                    {
                        try
                        {
                            Console.WriteLine("thread "+Thread.CurrentThread.ManagedThreadId+" started");
                            if ((p as ProcessorParam) != null)
                            {
                                //create new processor object
                                UrlProcessor proc = new UrlProcessor(p as ProcessorParam);

                                //completed handler
                                proc.UrlProcessed += OnUrlProcessed;
                                //error handler
                                proc.ProcessingError += OnProcessError;

                                //run processing
                                proc.Process();
                            }
                            Console.WriteLine("thread "+Thread.CurrentThread.ManagedThreadId.ToString() + " END");
                        }
                        catch (Exception e)
                        {
                            OnProcessError(null, new UrlErrorEventArgs() { ErrorMessage = e.Message, ErrorTime = DateTime.Now, ThreadName = Thread.CurrentThread.ManagedThreadId.ToString() });
                        }
                        finally
                        {
                            finished.Signal();
                        }
                    }
                    );
                t.IsBackground = true;
                t.Start(opParams[i]);

            }

            //waiting for all threads are done
            finished.Wait();
        }

        /// <summary>
        /// Save results to the output file
        /// </summary>
        /// <param name="outputFileName">output file name</param>
        public void WriteResultFile(string outputFileName)
        {
            if (!String.IsNullOrEmpty(outputFileName))
            {
                try
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFileName, false))
                    {
                        foreach (var r in _results)
                        {
                            sw.Write(r.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    //log message
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
            else
                throw new ApplicationException("Output file name was not set");
            
        }
    }
}
