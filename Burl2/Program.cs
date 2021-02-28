using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Burl2
{
    class Program
    {
        static String[] tabUrl;
        static String[] tabFile;
        static List<String[]> tabExt = new List<String[]>();
        static ManualResetEvent[] mre;

        static String LOG_FILE = "log.txt";
        static String URL_FILE = "urls.txt";
        static String FILES_FILE = "files.txt";
        
        static Dictionary<String, HttpStatusCode> dicLog = new Dictionary<String, HttpStatusCode>();


        /// <summary>
        /// Obvious entry point. Check needed files, call InitConfig(), start to fetch urls
        /// </summary>
        static void Main()
        {
            if (!File.Exists(URL_FILE) || !File.Exists(FILES_FILE) || !File.Exists("Burl2.exe.config"))
            {
                Console.WriteLine("Needed files : "+ URL_FILE +","+ FILES_FILE +", Burl2.exe.config");
                return;
            }

            //read .config and put it in struct Config
            InitConfig();

            //Start
            DateTime dt_debut = DateTime.Now;
            Log("Starting new session - " + dt_debut.ToShortDateString() + " " + dt_debut.ToShortTimeString());
            Console.WriteLine("Starting new session - " + dt_debut.ToShortDateString() + " " + dt_debut.ToShortTimeString());
            
            //1 url = 1 thread
            mre = new ManualResetEvent[tabUrl.Length];

            for (int i = 0, n = tabUrl.Length; i < n; i++)
            {
                mre[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(FetchUrls), i);
            }

            WaitHandle.WaitAll(mre);


            //stop, log time
            TimeSpan ts = DateTime.Now - dt_debut;
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<String, HttpStatusCode> pair in dicLog)
                sb.AppendLine(pair.Key + " : " + pair.Value);

            Log(sb.ToString());

            Log("Session finished : " + (ts.Minutes + "m " + ts.Seconds + "s") + Environment.NewLine);
            Console.WriteLine("Session finished : " + (ts.Minutes + "m " + ts.Seconds + "s"));
        }


        /// <summary>
        /// Read variables from the app.config file and control them
        /// </summary>
        static void InitConfig()
        {
            tabFile = File.ReadAllLines(FILES_FILE);
            tabUrl = File.ReadAllLines(URL_FILE);

            Config.proxy = Config.StringToBool(ConfigurationManager.AppSettings["proxy"]);
            Config.proxyUser = ConfigurationManager.AppSettings["proxyUser"];
            Config.proxyPassword = ConfigurationManager.AppSettings["proxyPassword"];
            Config.proxyHost = ConfigurationManager.AppSettings["proxyHost"];

            Config.timeout = Config.StringToInt(ConfigurationManager.AppSettings["timeout"]);
            Config.method = ConfigurationManager.AppSettings["method"];
            Config.userAgent = ConfigurationManager.AppSettings["userAgent"];

            Config.waitDelay = Config.StringToInt(ConfigurationManager.AppSettings["waitDelay"]);

            Config.outputVerbose = Config.StringToInt(ConfigurationManager.AppSettings["outputVerbose"]);
            if (Config.outputVerbose < 1) Config.outputVerbose = 1;
            else if (Config.outputVerbose > 3) Config.outputVerbose = 3;

            String listExt = ConfigurationManager.AppSettings["extensions"];
            String[] tab = listExt.Split('\n');

            for (int i = 0, n = tab.Length; i < n; i++) tabExt.Add(tab[i].Trim().Split(','));

            Config.searchIntensity = Config.StringToInt(ConfigurationManager.AppSettings["searchIntensity"]);
            if (Config.searchIntensity < 1) Config.searchIntensity = 1;
            else if (Config.searchIntensity > tabExt.Count) Config.searchIntensity = tabExt.Count -1;
        }



        /// <summary>
        /// Fetch files.txt and test every entry with extension for a given URL
        /// </summary>
        static void FetchUrls(object o)
        {
            int searchIntensity, idxUrl = (int)o;
            String page, url = tabUrl[idxUrl];

            if (!url.StartsWith("http")) url = "http://" + url;
            if (!url.EndsWith("/")) url += "/";

            //start with the base URL
            CheckUrl(url);
            
            for (int i = 0, n = tabFile.Length; i < n; i++)
            {
                if (String.IsNullOrWhiteSpace(tabFile[i]))
                    continue;

                //custom searchIntensity
                String[] tabInfo = tabFile[i].Split('|');
                page = tabInfo[0];

                if (tabInfo.Length == 1) searchIntensity = Config.searchIntensity;
                else if (!int.TryParse(tabInfo[1], out searchIntensity)) searchIntensity = Config.searchIntensity;
                //

                if (page.StartsWith("%")) //single test
                {
                    CheckUrl(url + page.Substring(1));
                }
                else if (!page.StartsWith("#")) //ignore comments
                {
                    for (int j = 0; j < searchIntensity; j++)
                    {
                        for (int k = 0, l = tabExt[j].Length; k < l; k++)
                        {
                            CheckUrl(url + page + tabExt[j][k]);
                        }
                    }
                }

                Thread.Sleep(Config.waitDelay);
            }

            //We are done fetching, end the thread
            mre[idxUrl].Set();
        }


        /// <summary>
        /// Intermediate function which handle an URL, check it and log the result
        /// </summary>
        /// <param name="url"></param>
        static void CheckUrl(String url)
        {
            HttpStatusCode result = HttpRequest(url);

            //Log it
            if ((Config.outputVerbose == 1 && result == HttpStatusCode.OK)
                || (Config.outputVerbose == 2 && result != HttpStatusCode.NotFound && result != HttpStatusCode.RequestTimeout)
                || (Config.outputVerbose == 3))
            {
                Console.WriteLine(url + " : " + result);
                dicLog.Add(url, result);
            }

        }


        /// <summary>
        /// Do the HTTP query.
        /// </summary>
        /// <param name="url">Complete URL for the query</param>
        /// <returns>Http Status Code</returns>
        static HttpStatusCode HttpRequest(String url)
        {
            //Configure the HTTP request
            HttpWebRequest hreq = (HttpWebRequest)HttpWebRequest.Create(url);
            hreq.Method = Config.method;
            hreq.Timeout = Config.timeout;
            hreq.KeepAlive = false;
            hreq.UserAgent = Config.userAgent;
            hreq.Accept = "Accept:text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            //Stop, proxy time
            if (Config.proxy)
            {
                WebProxy myProxy;

                if (!String.IsNullOrEmpty(Config.proxyHost)) myProxy = new WebProxy(Config.proxyHost);
                else myProxy = WebProxy.GetDefaultProxy();

                if (!String.IsNullOrEmpty(Config.proxyUser)) myProxy.Credentials = new NetworkCredential(Config.proxyUser, Config.proxyPassword);
                else myProxy.UseDefaultCredentials = true;

                hreq.Proxy = myProxy;
            }

            //Get the HTTP response
            HttpWebResponse hresp;
            HttpStatusCode status;
            try
            {
                hresp = (HttpWebResponse)hreq.GetResponse();
                status = hresp.StatusCode;
                hresp.Close();
            }
            catch (WebException e)
            {
                hresp = (HttpWebResponse)e.Response;
                
                if (hresp == null) status = HttpStatusCode.RequestTimeout;
                else
                {
                    status = hresp.StatusCode;
                    hresp.Close();
                }
            }
            
            return status;
        }
      
        /// <summary>
        /// Write queries results
        /// </summary>
        static void Log(String txt)
        {
            using (StreamWriter sw = new StreamWriter(LOG_FILE, true))
            {
                sw.WriteLine(txt);
            }
        }


        /// <summary>
        /// Contains all cheangeable variables in the app.config file and methods to get them properly
        /// </summary>
        struct Config
        {
            public static Boolean proxy = false;
            public static String proxyUser = "";
            public static String proxyPassword = "";
            public static String proxyHost = "";

            public static Int32 timeout = 0; //in millisecond
            public static String method = ""; //HTTP method.Protip : use inexistent method to (try to) bypass .htaccess that are using <limit>
            public static String userAgent = ""; //Act real to bypass bot filters

            public static Int32 searchIntensity = 1;
            public static Int32 outputVerbose = 1;
            public static Int32 waitDelay = 0;

            public static bool StringToBool(String value)
            {
                return (value.ToLower() == "true");
            }

            public static int StringToInt(String value)
            {
                int result;
                if (!Int32.TryParse(value, out result)) result = 0;
                return result;
            }
        }
    }
}