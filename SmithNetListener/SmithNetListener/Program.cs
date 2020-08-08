using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AlexaAPI;
using AlexaAPI.Request;
using AlexaAPI.Response;
using Amazon.Lambda.Core;
using Jishi.Intel.SonosUPnP;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SmithNetListener
{
    class Program
    {
        // For handling Kodi Window State
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        static void Main(string[] args)
        {
            //run in admin cmd: netsh http add urlacl url="http://+:1234/" user=Everyone listen=yes
            //netsh http add urlacl url="http://+:10788/" user=Everyone listen=yes
            //netsh http delete urlacl url=http://+:8080/
            //https://stackoverflow.com/questions/9735461/httplistener-not-working-over-network
            //https://stackoverflow.com/questions/26412602/httplistener-server-returns-an-error-503-service-unavailable
            using (var listener = new HttpListener())
            {
                //var prefixes = new List<string>() { "http://*:1234/" };

                //listener.Prefixes.Add("http://*:10788/");
                listener.Prefixes.Add("http://+:10788/");

                // Create a listener.
                //HttpListener listener = new HttpListener();
                // Add the prefixes.
                //foreach (string s in prefixes)
                //{
                //    listener.Prefixes.Add(s);
                //}
                listener.Start();

                //var d = new SonosDiscovery();
                //d.StartScan();

                Console.WriteLine("Listening...");
                while (true)
                { 
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;

                    string message;
                    using (var reader = new StreamReader(request.InputStream,
                                                         request.ContentEncoding))
                    {
                        message = reader.ReadToEnd();
                        Console.WriteLine(DateTime.Now.ToString() + message);

                        dynamic alexaReq = JsonConvert.DeserializeObject(message);

                        if (alexaReq.intent == "KodiOpen")
                        {
                            Process kodi = Process.Start("C:\\Program Files (x86)\\Kodi\\Kodi.exe");
                            // Bring to Front
                            IntPtr handle = kodi.MainWindowHandle;
                            // check minimized state
                            if (IsIconic(handle))
                                ShowWindow(handle, 9); // restore

                            SetForegroundWindow(handle);
                        }
                        else if (alexaReq.intent == "KodiClose")
                        {
                            Process.Start("taskkill", "/F /IM Kodi.exe");
                            //foreach (var process in Process.GetProcessesByName("Kodi.exe"))
                            //{
                            //    process.
                            //    process.Kill();
                            //}
                        }
                        //SONOS
                        //https://github.com/jishi/Jishi.Intel.SonosUPnP
                        //https://en.community.sonos.com/advanced-setups-229000/fully-managed-c-library-32999
                        //else if (alexaReq.intent == "TVSurroundOn")
                        //{
                        //    var discovery = new SonosDiscovery();
                        //    discovery.StartScan();
                        //}
                        //else if (alexaReq.intent == "TVSurroundOff")
                        //{
//
   //                     }
                    }

                    // TODO: read and parse the JSON data from 'request.InputStream'

                    using (HttpListenerResponse response = context.Response)
                    {
                        // returning some test results

                        // TODO: return the results in JSON format

                        string responseString = "<HTML><BODY>Hello, world!</BODY></HTML>";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        using (var output = response.OutputStream)
                        {
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }
                    }
                }
            }
        }
    }
}
