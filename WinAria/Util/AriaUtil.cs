using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonRPC;
using Newtonsoft.Json.Linq;

namespace WinAria.Util
{
    public delegate void JsonRPCResponseEventHandler(JsonRPCResponse result);
    public class BitTorrentItem
    {
        //public List<string> announceList { get; set; }
        public Dictionary<string, string> info { get; set; }
        public string mode { get; set; }
    }
    public class MissionUri
    {
        public string status { get; set; }
        public string uri { get; set; }
    }
    public class MissionFile
    {
        public string completedLength { get; set; }
        public string index { get; set; }
        public string length { get; set; }
        public string path { get; set; }
        public string select { get; set; }
        public List<MissionUri> uris { get; set; }
    }
    public class MissionItem
    {
        public BitTorrentItem bittorrent { get; set; }
        public string completedLength { get; set; }
        public string connections { get; set; }
        public string dir { get; set; }
        public string downloadSpeed { get; set; }
        public string status { get; set; }
        public string totalLengthSize { get { return AriaUtil.GetFileSize(long.Parse(totalLength)); } }
        public string totalLength { get; set; }
        public string uploadLength { get; set; }
        public string uploadSpeed { get; set; }
        public string downloadSpeedSize
        {
            get
            {
                return AriaUtil.GetFileSize(int.Parse(downloadSpeed)) + "/s";
            }
        }
        public string gid { get; set; }
        public string progress
        {
            get
            {
                if (long.Parse(totalLength) == 0)
                {
                    return "0%";
                }
                double pst = (double.Parse(completedLength) / double.Parse(totalLength));
                return Math.Round((pst * 100), 2) + "%";
            }
        }
        public List<MissionFile> files { get; set; }
        public string DisplayName
        {
            get
            {
                string displayName = files[0].path;
                if (bittorrent != null && bittorrent.info != null && bittorrent.info.ContainsKey("name"))
                {
                    return bittorrent.info["name"];
                }
                else
                {
                    if (displayName.IndexOf("/") > 0)
                    {
                        displayName = displayName.Substring(displayName.LastIndexOf("/") + 1);
                    }
                }
                return displayName;
            }
        }

    }
    public class JsonRPCResponse
    {
        public JToken Result { get; set; }
    }
    public class AriaUtil
    {
		public static bool IsRunning()
        {
            Process[] processes = Process.GetProcessesByName("aria2c");
            return processes.Length == 0 ? false : true;
        }
        public static Dictionary<string,string> LoadConfig()
        {
            string binaryPath = AppDomain.CurrentDomain.BaseDirectory + @"binary\";
            Dictionary<string, string> config = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(binaryPath + "aria2.conf");
            foreach(string line in lines)
            {
                string[] kv = line.Trim().Split('=');
                config.Add(kv[0], kv[1]);
            }
            return config;
        }
        public static void SaveConfig(Dictionary<string,string> config)
        {
            string buffer = "";
            foreach(string k in config.Keys)
            {
                buffer += k + "=" + config[k] + "\r\n";
            }
            string binaryPath = AppDomain.CurrentDomain.BaseDirectory + @"binary\";
            File.WriteAllText(binaryPath + "aria2.conf", buffer);
        }
		public static void Start()
        { 
            string binaryPath = AppDomain.CurrentDomain.BaseDirectory + @"binary\";
            ProcessStartInfo startInfo = new ProcessStartInfo(binaryPath + "aria2c.exe", "--enable-rpc=true --save-session=" + binaryPath + "aria2.session --input-file=" + binaryPath + "aria2.session --conf-path=" + binaryPath + "aria2.conf");
            startInfo.WorkingDirectory = binaryPath;
            //startInfo.UseShellExecute = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(startInfo);
        }

        public static void JsonRequestAsync(string method,JToken param,JsonRPCResponseEventHandler eventHandler)
        {
            try
            {
                using (Client rpcClient = new Client("http://127.0.0.1:10086/jsonrpc"))
                {
                    Request req = rpcClient.NewRequest(method, param);
                    GenericResponse response = rpcClient.Rpc(req);
                    JToken result = null;
                    if (response.Result != null)
                    {
                        result = response.Result;
                    }
                    eventHandler(new JsonRPCResponse() { Result = result });
                }
            }
            catch (Exception ex)
            {
                eventHandler(new JsonRPCResponse() { Result = null });
            }

        }


        public static string GetFileSize(long size)
        {
            var num = 1024.00; //byte

            if (size < num)
                return size + "B";
            if (size < Math.Pow(num, 2))
                return (size / num).ToString("f2") + "K"; //kb
            if (size < Math.Pow(num, 3))
                return (size / Math.Pow(num, 2)).ToString("f2") + "M"; //M
            if (size < Math.Pow(num, 4))
                return (size / Math.Pow(num, 3)).ToString("f2") + "G"; //G

            return (size / Math.Pow(num, 4)).ToString("f2") + "T"; //T
        }
    }
}
