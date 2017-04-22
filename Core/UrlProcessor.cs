using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using Encodings.Parser;
using System.Diagnostics;

namespace Encodings
{
    /// <summary>
    /// Page retriever and encoding resolver
    /// </summary>
    public class UrlProcessor
    {
        #region Events
        
        /// <summary>
        /// Event handler type for error notification
        /// </summary>
        public delegate void ErrorEventHandler(object sender, UrlErrorEventArgs e);
        
        /// <summary>
        /// Error occurred
        /// </summary>
        public event ErrorEventHandler ProcessingError;

        /// <summary>
        /// Event handler type for work done notification
        /// </summary>
        public delegate void UrlProcessedEventHandler(object sender, UrlProcessedEventArgs e);

        /// <summary>
        /// All urls are processed
        /// </summary>
        public event UrlProcessedEventHandler UrlProcessed;

        protected void OnUrlProcessed(object sender, UrlProcessedEventArgs e)
        {
            UrlProcessedEventHandler copy = UrlProcessed;
            if (copy != null) copy(this, e);
        }

        protected void OnProcessingError(object sender, UrlErrorEventArgs e)
        {
            ErrorEventHandler copy = ProcessingError;
            if (copy != null) copy(this, e);
        }

        #endregion

        /// <summary>
        /// URLs list to process
        /// </summary>
        List<string> _urls;

        /// <summary>
        /// create new processor object
        /// </summary>
        /// <param name="param">URL to process</param>
        public UrlProcessor(ProcessorParam param)
        {
            _urls = new List<string>();
            _urls.AddRange(param.UrlsToProcess);
        }

        /// <summary>
        /// Parse html-page source. Get meta tags.
        /// </summary>
        /// <param name="htmldata">html-page source</param>
        /// <returns></returns>
        private List<HtmlMeta> Parse(string htmldata)
        {
            Regex metaregex = 
                new Regex(@"<\s*meta\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'" +
                            @"[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>", 
                            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
   
            List<HtmlMeta> MetaList = new List<HtmlMeta>();
            foreach (Match metamatch in metaregex.Matches(htmldata))
            {
                HtmlMeta mymeta = new HtmlMeta();
                   
                Regex submetaregex = 
                    new Regex(@"(?<name>\b(\w|-)+\b)\s*=\s*(""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^""'<> ]+)\s*)+",
                                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
  
                foreach (Match submetamatch in submetaregex.Matches(metamatch.Value.ToString()))
                {
                    if ("http-equiv" == submetamatch.Groups["name"].ToString().ToLower())
                        mymeta.HttpEquiv = submetamatch.Groups["value"].ToString();
   
                    if (("name" == submetamatch.Groups["name"].ToString().ToLower())
                        && (mymeta.HttpEquiv == String.Empty))
                    {
                        // if already set, HTTP-EQUIV overrides NAME
                        mymeta.Name = submetamatch.Groups["value"].ToString();
                    }
                    if ("content" == submetamatch.Groups["name"].ToString().ToLower())
                    {
                        mymeta.Content = submetamatch.Groups["value"].ToString();
                    }
                    
                }
                MetaList.Add(mymeta);
            }
            return MetaList;
        }

        /// <summary>
        /// retrieve charset from meta tags
        /// </summary>
        /// <param name="meta">meta tag</param>
        /// <returns></returns>
        private List<String> ParseEncodings(HtmlMeta meta)
        {
            List<String> results = new List<string>();

            Regex csregex =
                new Regex(@"\s*charset\s*=\s*(""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^""'<> ]+)\s*)+",
                                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            foreach (Match submetamatch in csregex.Matches(meta.Content))
            {
                var value = submetamatch.Groups["value"].ToString();
                if (!String.IsNullOrEmpty(value))
                    results.Add(value);
            }

            foreach (Match submetamatch in csregex.Matches(meta.HttpEquiv))
            {
                var value = submetamatch.Groups["value"].ToString();
                if (!String.IsNullOrEmpty(value))
                    results.Add(value);
            }

            return results;
        }

        /// <summary>
        /// Optimized method for getting encoding using custom tag parser
        /// </summary>
        /// <param name="url">page address</param>
        /// <returns>list of encodings</returns>
        private List<string> GetPageEncodings(string url)
        {
            //get page content
            String page = GetPage(url);

            Stopwatch stop = Stopwatch.StartNew();

            List<String> results = new List<string>();

            //create tag parser
            SimpleTagSearcher parser = new SimpleTagSearcher(page);
            //retrieve target tags markup (indexes)
            List<TagMarkup> tags = parser.SearchTag("meta");
            List<String> searchedEncList = new List<string>();

            //in each tag inspect properties
            foreach (var tag in tags)
            {
                foreach (var prop in tag.Props)
                {
                    //if property == content
                    if (prop.KeyMarkup.GetFromString(page).ToLower() == "content")
                    {
                        //parse property: try to retrieve key=value pairs from there
                        List<Prop> metaprops = MetaContentPropertyParser.GetContentProperties(page, prop.ValMarkup.Start, prop.ValMarkup.End);
                        foreach (var metaprop in metaprops)
                        {
                            //if key=charset  -> get encoding
                            if (metaprop.KeyMarkup.GetFromString(page).ToLower() == "charset")
                            {
                                searchedEncList.Add(metaprop.ValMarkup.GetFromString(page));
                            }
                        }

                    }
                }
            }

            foreach (var s_enc in searchedEncList)
            {
                Encoding enc;
                try
                {
                    enc = Encoding.GetEncoding(s_enc);
                }
                catch (Exception)
                {
                    String lookuped;
                    if (!Util.EncodingTable.Instance.TryFindEncoding(s_enc, out lookuped))
                    {
                        enc = Encoding.Default;
                    }
                    else
                    {
                        try
                        {
                            enc = Encoding.GetEncoding(lookuped);
                        }
                        catch (Exception)
                        {
                            //bad lookup
                            enc = Encoding.Default;
                        }
                    }
                }

                results.Add(enc.EncodingName);
            }
            stop.Stop();
            //Console.WriteLine("Custom parser {0}", stop.ElapsedTicks);

            return results;
        }

        /// <summary>
        /// extract encodings from html-page
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<string> GetRegexPageEncodings(string url)
        {
            String page = GetPage(url);

            Stopwatch stop = Stopwatch.StartNew();

            List<HtmlMeta> metas = Parse(page);
            List<String> results = new List<string>();
            foreach (var meta in metas)
            {
                var searchedEncList = ParseEncodings(meta);

                foreach (var s_enc in searchedEncList)
                {
                    Encoding enc;
                    try
                    {
                        enc = Encoding.GetEncoding(s_enc);
                    }
                    catch (Exception)
                    {
                        String lookuped;
                        if (!Util.EncodingTable.Instance.TryFindEncoding(s_enc, out lookuped))
                        {
                            enc = Encoding.Default;
                        }
                        else
                        {
                            try
                            {
                                enc = Encoding.GetEncoding(lookuped);
                            }
                            catch (Exception)
                            {
                                //bad lookup
                                enc = Encoding.Default;
                            }
                        }
                    }

                    results.Add(enc.EncodingName);
                }
                
            }
            stop.Stop();
            //Console.WriteLine("regex {0}", stop.ElapsedTicks);
            return results;
        }

        /// <summary>
        /// Get HTML
        /// </summary>
        /// <param name="url">address</param>
        /// <returns></returns>
        private String GetPage(string url)
        {
	        StringBuilder sb  = new StringBuilder();

	        // used on each read operation
	        byte[] buf = new byte[8192];

            HttpWebRequest  request  = null;
	        HttpWebResponse response = null;
            Stream resStream = null;

            try 
	        {	        
	            request  = (HttpWebRequest)WebRequest.Create(url);
                request.UseDefaultCredentials = true;
                request.Method = "GET";
	            response = (HttpWebResponse)request.GetResponse();

	            // we will read data via the response stream
	            resStream = response.GetResponseStream();

       	        string tempString = null;
	            int    count      = 0;

                Encoding enc;
                try
                {
                    enc = Encoding.GetEncoding(response.CharacterSet);
                }
                catch (Exception)
                {
                    enc = Encoding.Default;
                }

                if (request.HaveResponse == true && response != null)
	                do
	                {
		                // fill the buffer with data
		                count = resStream.Read(buf, 0, buf.Length);

		                if (count != 0)
		                {

			                // translate from bytes to encoding from header
			                tempString = enc.GetString(buf, 0, count);

			                // continue building the string
			                sb.Append(tempString);
		                }
	                }
	                while (count > 0); // any more data to read?

	        }
	        catch (Exception e)
	        {
                OnProcessingError(this, new UrlErrorEventArgs() { ErrorMessage = url+"\n"+e.Message, ErrorTime = DateTime.Now, ThreadName = Thread.CurrentThread.ManagedThreadId.ToString() });
	        }
            finally
            {
                if(resStream!=null ) resStream.Close();
                if(response!=null ) response.Close();
            }

            return sb.ToString();            
        }

        /// <summary>
        /// Process passed URLs
        /// </summary>
        public void Process()
        {
          
            foreach (string url in _urls)
            {
                try
                {
                    
                    //List<string> Encodings = GetRegexPageEncodings(url);
                    
                    List<string> Encodings = GetPageEncodings(url);

                    //notify about completing
                    OnUrlProcessed(this,
                        new UrlProcessedEventArgs(new ProcessingResult() { Url = url, Encodings = Encodings })
                                {
                                    ThreadName = Thread.CurrentThread.ManagedThreadId.ToString(),
                                    Time = DateTime.Now
                                }
                        );

                }
                catch (Exception e)
                {
                    var message = e.Message;
                    if (e.InnerException != null)
                        message += "\n"+e.InnerException.Message;
                    OnProcessingError(this, new UrlErrorEventArgs() { ErrorMessage = message, ErrorTime = DateTime.Now, ThreadName = Thread.CurrentThread.ManagedThreadId.ToString() });
                }
            }
        }
    }
}
