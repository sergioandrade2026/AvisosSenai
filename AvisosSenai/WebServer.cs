using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace AvisosSenai
{
    public class WebServer
    {
        private HttpListener _listener;
        private readonly string _webRoot = @"I:\website\";
        private readonly string _dbPath = @"I:\website\db.json";

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
                var request = context.Request;
                var response = context.Response;

                try
                {
                   
                    if (request.HttpMethod == "POST" && request.RawUrl == "/salvar")
                    {
                        HandleSalvar(request, response);
                    }
                    else
                    {
                        HandleStaticFiles(request, response);
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

        private void HandleSalvar(HttpListenerRequest request, HttpListenerResponse response)
        {
            using (var reader = new StreamReader(request.InputStream))
            {
                string body = reader.ReadToEnd();

             
                string jsonFinal;

                if (File.Exists(_dbPath))
                {
                    string existente = File.ReadAllText(_dbPath);

                    if (existente.Length > 2) 
                        jsonFinal = existente.TrimEnd(']') + "," + body + "]";
                    else
                        jsonFinal = "[" + body + "]";
                }
                else
                {
                    jsonFinal = "[" + body + "]";
                }

                File.WriteAllText(_dbPath, jsonFinal);
            }

            response.StatusCode = 200;
            byte[] ok = Encoding.UTF8.GetBytes("{\"status\":\"ok\"}");
            response.OutputStream.Write(ok, 0, ok.Length);
        }

        private void HandleStaticFiles(HttpListenerRequest request, HttpListenerResponse response)
        {
            string requestUrl = request.RawUrl == "/" ? "index.html" : request.RawUrl.Substring(1);
            string fullPath = _webRoot + requestUrl;

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