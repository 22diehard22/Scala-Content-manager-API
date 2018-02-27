using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Scala_API
{
    class Authorization
    {
        #region variables
        #region publicVariables
        // Public Variables: 
        static public string token;
        static public string apiToken;
        static public string apiLicenseToken;
        // ---------------------

        public string contentManager_URL = Properties.Settings.Default.ScalaServer; 
        
        #endregion// Public Variables

        #region private Variables
        private string login_URL = "/api/rest/auth/login";
        private string ping_URL = "/api/rest/auth/ping";

        // Authentication: 
        string username = ""; // TODO: Configurable VIA interface -- Retrieve info instead of hardset.
        private string password = ""; // TODO: Configurable VIA interface -- Retrieve info instead of hardset. 



        #endregion // Private Variables
        #endregion // Variables

        public string AuthorizeAPI(string server, string user, string password)
        {
            // Verify Scala URL is correct: -- This will error out if URI is bad before the code runs. 
            if (Uri.IsWellFormedUriString(server, UriKind.Absolute)) 
            {
                try
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(server + login_URL);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "{\"username\":\"" + user + "\"," +
                                      "\"password\":\"" + password + "\"," +
                                      "\"rememberMe\":\"true\"}";
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        //Parse Data:
                        string front = "\""; string back = "\":"; // JSON Added values. 
                        string tokenSearch = (front + "token" + back);
                        string apiTokenSearch = (front + "apiToken" + back);
                        string apiLicenseTokenSearch = (front + "apiLicenseToken" + back);

                        String pattern = @",";
                        String[] elements = Regex.Split(result, pattern);
                        foreach (var element in elements)
                        {
                            if (element.Contains(tokenSearch))
                            {
                                token = getValue(element);
                                // Console.WriteLine(token);
                            }
                            if (element.Contains(apiTokenSearch))
                            {
                                apiToken = getValue(element);
                                // Console.WriteLine(apiToken);
                            }
                            if (element.Contains(apiLicenseTokenSearch))
                            {
                                apiLicenseToken = getValue(element);
                                // Console.WriteLine(apiLicenseToken);
                            }
                        }
                        Thread keepAlive = new Thread(() => keepAPIalive());
                        keepAlive.IsBackground = true;
                        keepAlive.Start();
                    }
                }
                catch (WebException ex)
                {
                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        Console.WriteLine("Error code: {0}", httpResponse.StatusCode);

                        if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            // Note: Scala API will lock user out for a minute in the event too many failed login attempts happen. 
                            return "UNAUTHORIZED";
                        }
                    }
                }
                return "SUCCESS";
            }
        

            else
            {
                return "INVALIDURL";
               
            }
        
               
        }

        private void keepAPIalive() // Send a ping request every X Amount of seconds. 
        {
            int keepAliveInternal = 20000;
        
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(contentManager_URL + ping_URL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Token:" + token);
            while (true) // Every X  seconds ping server. -- This task is meant to run as a thread. 
            {
                try
                {
                    System.Threading.Thread.Sleep(keepAliveInternal);
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    if (httpResponse.ToString() == "Unauthorized" )
                    {
                        AuthorizeAPI(Properties.Settings.Default.ScalaServer, Properties.Settings.Default.ScalaUser, Properties.Settings.Default.ScalaPassword);
                        return;
                    }
                {
                 //   var result = streamReader.ReadToEnd();
                }
                } // END Try
                catch (IOException e)
                {
                    Console.WriteLine("Exception: {0}", e.Source);
                }
            }
        }

        public void getAllPlayers()
        {
            string players_URL = "/api/rest/players";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(contentManager_URL + players_URL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Token:" + token);
            //Console.WriteLine(token); // DEBUG USE ONLY.

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                //Console.WriteLine(result); // DEBUG USE ONLY.
            }
        }

        private string getValue(string element)
        {
            element = element.Split(':').Last();
            element = element.Trim('"');
            return (element);
        }


        public void testScalaAuthorization()
        {
            string API = "/api/rest/auth/get";   // Does not change: 
            // Attempt to open URL: 
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(contentManager_URL + API);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                // If URL Fails output error to console handle silently. 

                try // Capture output, If unauthorized it will toss an error, so we are catching the error and will need to act on it. 
                {
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        Console.WriteLine(result); // DEBUG USE ONLY.
                    }
                }
                catch (WebException e)
                {
                    using (WebResponse response = e.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        Console.WriteLine("Error code: {0}", httpResponse.StatusCode);

                        if (httpResponse.StatusCode ==  HttpStatusCode.Unauthorized)
                        {
                            AuthorizeAPI(Properties.Settings.Default.ScalaServer, Properties.Settings.Default.ScalaUser, Properties.Settings.Default.ScalaPassword); // System failed to authorize attempting to try again. 
                            Console.WriteLine("Failed to authorize Scala API attempting to authorizie. ");
                        }
                    }
                }

            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);

                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        AuthorizeAPI(Properties.Settings.Default.ScalaServer, Properties.Settings.Default.ScalaUser, Properties.Settings.Default.ScalaPassword); // System failed to authorize attempting to try again. 
                    }

                    else
                    {
                        Console.WriteLine("Invalid ContentManager URL" + e);
                    }
                }
            }

            //httpWebRequest.Headers.Add("Token:" + token);

        }


        public int StatusCode { get; set; }

    }

}
