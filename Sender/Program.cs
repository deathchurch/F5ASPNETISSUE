using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string proxyServer = null;
            int proxyPort = 0;
            int httpTimeout = 10;
            string destination = "https://localhost:5001";
            string eventsFile = "Events.json";
            bool useH2 = true;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

                if (arg == "--proxy")
                {
                    proxyServer = args[i + 1];
                }

                if (arg == "--port")
                {
                    proxyPort = int.Parse(args[i + 1]);
                }

                if (arg == "--timeout")
                {
                    httpTimeout = int.Parse(args[i + 1]);
                }

                if (arg == "--destination")
                {
                    destination = args[i + 1];
                }

                if (arg == "--file")
                {
                    eventsFile = args[i + 1];
                }

                if (arg == "--http1.1")
                {
                    useH2 = false;
                }
            }

            await SendData(proxyServer, proxyPort, httpTimeout, destination, eventsFile, useH2);
        }

        static async Task SendData(string proxy, int proxyPort, int timeout, string destination, string dataFile, bool h2)
        {
            Stopwatch sw = new Stopwatch();

            Console.WriteLine("Proxy URL: {0}", proxy ?? "None");
            Console.WriteLine("Proxy Port: {0}", proxyPort);
            Console.WriteLine("Timout: {0}", timeout);
            Console.WriteLine("Destination: {0}", destination ?? "None");
            Console.WriteLine("File: {0}", dataFile ?? "None");

            try
            {
                Console.WriteLine("{0} - Reading Events file", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                string events = System.IO.File.ReadAllText(dataFile);

                using HttpClientHandler httpClientHandler = new HttpClientHandler();

                if (proxy is object)
                {
                    httpClientHandler.Proxy = new WebProxy(proxy, proxyPort);
                }

                //Added to enable Kestrel to use in house dev cert
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };

                Uri u = new Uri(destination);

                using HttpClient client = new HttpClient(httpClientHandler)
                {
                    Timeout = TimeSpan.FromSeconds(timeout)
                };

                HttpContent content = new StringContent(events, Encoding.UTF8, "application/json");

                using HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = u,
                    Content = content,
                };
                if (h2)
                {
                    request.Version = new Version(2, 0);
                }

                Console.WriteLine("{0} - Sending Events file", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sw.Reset(); sw.Start();
                using HttpResponseMessage res = await client.SendAsync(request);
                sw.Stop();

                Console.WriteLine("{0} - Sent Events file in {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), sw.Elapsed.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} - {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), ex.Message);
            }
        }
    }
}
