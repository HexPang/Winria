using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonRPC;
using Newtonsoft.Json.Linq;

namespace WinAria.Util
{
    public delegate void JsonRPCResponseEventHandler(JsonRPCResponse result);
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

		public static void Start()
        { 
            string binaryPath = AppDomain.CurrentDomain.BaseDirectory + @"binary\";
            ProcessStartInfo startInfo = new ProcessStartInfo(binaryPath + "aria2c.exe", "--enable-rpc=true --input-file=" + binaryPath + "aria2.session --conf-path=" + binaryPath + "aria2.conf");
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
