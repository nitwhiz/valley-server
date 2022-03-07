using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using StardewModdingAPI;

namespace ValleyServer.server
{
    public class HttpServer
    {
        private TcpListener _server;

        private readonly IMonitor _monitor;

        private readonly Dictionary<string, Func<string>> _resources = new();

        public HttpServer(IMonitor monitor)
        {
            _monitor = monitor;
        }

        private void HandleClientStream(NetworkStream stream)
        {
            byte[] buf = new byte[256];
            string rawRequest = "";
            int readBytes;

            while ((readBytes = stream.Read(buf, 0, buf.Length)) != 0)
            {
                rawRequest += Encoding.ASCII.GetString(buf, 0, readBytes);

                if (readBytes < buf.Length)
                {
                    break;
                }
            }

            var requestParts = rawRequest.Split("\r\n\r\n");

            var requestPartCount = requestParts.Length;

            var rawHeaders = requestPartCount > 0 ? requestParts[0] : "";
            var rawBody = requestPartCount > 1 ? requestParts[1] : "";

            var requestLine = "";

            foreach (var headerLine in rawHeaders.Split("\r\n"))
            {
                if (headerLine != "")
                {
                    if (requestLine == "")
                    {
                        requestLine = headerLine;
                    }
                }
            }

            var requestLineSegments = requestLine.Split(" ");

            if (requestLineSegments.Length < 3)
            {
                return;
            }

            // HTTP METHOD is ignored
            
            var requestUri = requestLineSegments[1];
            var httpVersion = requestLineSegments[2];

            if (httpVersion != "HTTP/1.1")
            {
                return;
            }

            string responseBody = null;
            
            foreach (var resource in _resources)
            {
                if (requestUri.EndsWith(resource.Key))
                {
                    responseBody = resource.Value();
                    break;
                }
            }
            
            var responseStatusLine = "HTTP/1.1 404 Not Found";
            var responseHeaders = new[]
            {
                "Access-Control-Allow-Origin: *",
                "Content-Length: 0"
            };

            if (responseBody != null)
            {
                responseStatusLine = "HTTP/1.1 200 OK";
                responseHeaders = new[]
                {
                    "Access-Control-Allow-Origin: *",
                    "Content-Length: " + responseBody.Length
                };
            }
            else
            {
                responseBody = "";
            }
            
            var responseData = responseStatusLine
                               + "\r\n"
                               + string.Join("\r\n", responseHeaders)
                               + "\r\n\r\n"
                               + responseBody;

            stream.Write(Encoding.ASCII.GetBytes(responseData));

            stream.Close();
            stream.Dispose();
        }

        private void ListenLoop()
        {
            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();

                HandleClientStream(client.GetStream());

                client.Close();
            }
        }

        private void InitServer()
        {
            try
            {
                Int32 port = 3000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                _server = new TcpListener(localAddr, port);
                _server.Start();

                ListenLoop();
            }
            catch (Exception e)
            {
                _monitor.Log($"server exception: {e.Message}", LogLevel.Info);
            }
            finally
            {
                _server.Stop();
            }
        }

        public void AddResource(string path, Func<string> callback)
        {
            _resources.Add(path, callback);
        }
        
        public void Start()
        {
            new Thread(InitServer).Start();
        }
    }
}