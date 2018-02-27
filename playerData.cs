using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;

namespace Project.Scala_API
{
    class Players
    {
        //  Access Classes:
        Internal_Classes.DataSets.ScalaPlayers.players sPlayers = new Internal_Classes.DataSets.ScalaPlayers.players();
        MsSQL.addDevices addDevice = new MsSQL.addDevices();
        Scala_API.Authorization Auth = new Scala_API.Authorization();

        // API Variables: 
        private const string players_URL = "/api/rest/players";
        

        public void grabInitialData() // TODO: Rename to something else. 
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(@Auth.contentManager_URL + players_URL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Token:" + Scala_API.Authorization.token);
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd(); // Store the returned JSON data.
                dynamic jsonObj = JsonConvert.DeserializeObject(result);
                foreach (var obj in jsonObj.list) // I can grab any Value: 
                {
                    // Extract more information: 
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(Auth.contentManager_URL + "/api/rest/players/" + obj.id + "/state");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpWebRequest.Headers.Add("Token:" + Scala_API.Authorization.token);
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader2 = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result2 = streamReader2.ReadToEnd();
                        dynamic jsonObj2 = JsonConvert.DeserializeObject(result2);
                        //Console.WriteLine();
                        Console.WriteLine(obj.name + " " + obj.id + " " + obj.uuid + " " + jsonObj2.ip);

                        foreach (Form frm in Application.OpenForms)
                        {
                            if (frm.GetType() == typeof(Form1))
                            {
                                if (jsonObj2.ip == null || jsonObj2.ip == String.Empty)
                                {
                                    jsonObj2.ip = "UNKNOWN";
                                }
                                if (obj.name == null || obj.name == String.Empty)
                                {
                                    continue; // Skip
                                }

                                // Create strings:
                                string id = obj.id;
                                string uuid = obj.uuid;
                                string name = obj.name;
                                string ip = jsonObj2.ip;
                                string mac = obj.mac;
                                string enabled = obj.enabled;
                                string active = jsonObj2.active;
                                string state = jsonObj2.state;
                                string lastBooted = jsonObj2.lastBooted;
                                string releaseString = jsonObj2.releaseString;

                                // ADD to database:
                               addDevice.addScalaDevice(id, uuid, name, ip, mac, enabled, active, state, lastBooted, releaseString);
                                // DEBUG ::
                                 Console.WriteLine("DEBUG"); 
                            }
                        }
                    }
                }
            }
        }
        public void reGetData() // TODO: Rename to something else. 
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Auth.contentManager_URL + players_URL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Token:" + Scala_API.Authorization.token);
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd(); // Store the returned JSON data.
                dynamic jsonObj = JsonConvert.DeserializeObject(result);
                foreach (var obj in jsonObj.list) // I can grab any Value: 
                {
                    // Extract more information: 
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(Auth.contentManager_URL + "/api/rest/players/" + obj.id + "/state");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpWebRequest.Headers.Add("Token:" + Scala_API.Authorization.token);
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader2 = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result2 = streamReader2.ReadToEnd();
                        dynamic jsonObj2 = JsonConvert.DeserializeObject(result2);

                      //  Console.WriteLine(obj.name + " " + obj.id + " " + obj.uuid + " " + jsonObj2.ip);

                        foreach (Form frm in Application.OpenForms)
                        {
                            if (frm.GetType() == typeof(Form1))
                            {
                                if (jsonObj2.ip == null || jsonObj2.ip == String.Empty)
                                {
                                    jsonObj2.ip = "UNKNOWN";
                                }
                                if (obj.name == null || obj.name == String.Empty)
                                {
                                    continue; // Skip
                                }

                                // ADD MY DATA: First run ONLY 

                                // Form1.playerTable.Rows.Add(obj.id, obj.uuid, obj.name, jsonObj2.ip, obj.mac, obj.enabled, jsonObj2.active, jsonObj2.state, jsonObj2.lastBooted, jsonObj2.releaseString);
                                // Console.WriteLine("Debug Breakpoint");


                                foreach (DataRow dr in Internal_Classes.DataSets.ScalaPlayers.players.playerTable.Rows) // search whole table
                                {
                                    string x = obj.id;
                                    if (dr["id"].Equals(x))
                                    {
                                        dr["id"] = obj.id;
                                        dr["uuid"] = obj.uuid;
                                        dr["name"] = obj.name;
                                        dr["ip"] = jsonObj2.ip;
                                        dr["enabled"] = obj.enabled;
                                        dr["active"] = jsonObj2.active;
                                        dr["state"] = jsonObj2.state;
                                        dr["lastBooted"] = jsonObj2.lastBooted;
                                        dr["releaseString"] = jsonObj2.releaseString;
                                      // Console.WriteLine("Debug Breakpoint");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        // Pass in an ID and the value you are trying to get back.
        public dynamic playerIDstate(string id, string value)
        {
            string apiUrl = Auth.contentManager_URL + "/api/rest/players/" + id + "/state";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(apiUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Token:" + Scala_API.Authorization.token);

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd(); // Store the returned JSON data.
                dynamic jsonObj = JsonConvert.DeserializeObject(result);
                return jsonObj; // Treturn the entire object, let the monitor parse it. 
            }
            
        }
 }


    public class AdvancedPlayerInfo // Stores all player related data during get requests. 
    {
        public string id { get; set; }
        public string state { get; set; }
        public string active { get; set; }
        public string lastReported { get; set; }
        public string lastBooted { get; set; }
        public string lastReportedTimestamp { get; set; }
        public string lastBootedTimestamp { get; set; }
        public string planState { get; set; }
        public string inventoryCompleted { get; set; }
        public string ip { get; set; }
        public string host { get; set; }
        public string mac { get; set; }
        public string releaseString { get; set; }
        public string bannerName { get; set; }
        public string buildDate { get; set; }
        public string buildLanguage { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
        public string previewPlayer { get; set; }
        public string enabled { get; set; }
        public string type { get; set; }
        public string requestLogs { get; set; }
        public string downloadThreads { get; set; }
        public string unusedFilesCache { get; set; }
        public string planDeliveryMethod { get; set; }
        public string pollingInterval { get; set; }
        public string pollingUnit { get; set; }
        public string logLevel { get; set; }
        public string lastModified { get; set; }
        public string timezoneOffset { get; set; }
    }

    public class DistributionServer
    {
        public int id { get; set; }
        public string name { get; set; }
        public string driver { get; set; }
    }

    public class Channel
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class PlayerDisplay
    {
        public int id { get; set; }
        public string name { get; set; }
        public Channel channel { get; set; }
        public int screenCounter { get; set; }
    }

    public class List
    {
        public int id { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
        public bool previewPlayer { get; set; }
        public bool enabled { get; set; }
        public string mac { get; set; }
        public string type { get; set; }
        public DistributionServer distributionServer { get; set; }
        public List<PlayerDisplay> playerDisplays { get; set; }
        public bool requestLogs { get; set; }
        public int downloadThreads { get; set; }
        public int unusedFilesCache { get; set; }
        public string planDeliveryMethod { get; set; }
        public int pollingInterval { get; set; }
        public string pollingUnit { get; set; }
        public string logLevel { get; set; }
        public string active { get; set; }
        public string lastModified { get; set; }
        public string timezoneOffset { get; set; }
    }

    public class RootObject
    {
        public List<List> list { get; set; }
        public int offset { get; set; }
        public int count { get; set; }
        public int id { get; set; }
        public string state { get; set; }
        public string active { get; set; }
        public string lastReported { get; set; }
        public string lastBooted { get; set; }
        public string lastReportedTimestamp { get; set; }
        public string lastBootedTimestamp { get; set; }
        public string planState { get; set; }
        public bool inventoryCompleted { get; set; }
        public string ip { get; set; }
        public string host { get; set; }
        public string mac { get; set; }
        public string releaseString { get; set; }
        public string bannerName { get; set; }
        public string buildDate { get; set; }
        public string buildLanguage { get; set; }
    }

}


