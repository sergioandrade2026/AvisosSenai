using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AvisosSenai
{
    public class WebServer
    {
        private HttpListener _listener;
     
        private readonly string _webRoot = @"I:\website\";

        public void Start(int port = 80)
        {
            _listener = new HttpListener("http", port);
            _listener.Start();
            new Thread(Listen).Start();
        }

        private void Listen()
        {
            while (true)
            {
                var context = _listener.GetContext();
                var response = context.Response;

              
                string requestUrl = context.Request.RawUrl == "/" ? "index.html" : context.Request.RawUrl.Substring(1);
                string fullPath = _webRoot + requestUrl;

                try
                {
                    if (File.Exists(fullPath))
                    {
                        byte[] buffer = File.ReadAllBytes(fullPath);
                        response.ContentType = GetMimeType(requestUrl);
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        response.StatusCode = 404;
                    }
                }
                catch
                {
                    response.StatusCode = 500;
                }
                finally
                {
                    response.Close();
                }
            }
        }

        private string GetMimeType(string url)
        {
            if (url.EndsWith(".html")) return "text/html";
            if (url.EndsWith(".js")) return "application/javascript";
            if (url.EndsWith(".css")) return "text/css";
            if (url.EndsWith(".json")) return "application/json";
            return "text/plain";
        }
    }
}