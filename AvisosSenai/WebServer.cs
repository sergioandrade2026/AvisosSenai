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
                if (context == null) continue;

                var request = context.Request;
                var response = context.Response;

                try
                {
                    string url = request.RawUrl;
                    int qMark = url.IndexOf('?');
                    if (qMark > 0) url = url.Substring(0, qMark);

                    
                    if (request.HttpMethod == "POST" && url == "/api/adicionar")
                    {
                        AddAviso(request, response);
                    }
                    else if (request.HttpMethod == "POST" && url == "/deletar")
                    {
                        DeleteAviso(request, response);
                    }
                    else if (url == "/avisos")
                    {
                        ServeFile("db.json", response);
                    }
                    else
                    {
                        string fileName = (url == "/" || url == "") ? "index.html" : url.TrimStart('/');
                        ServeFile(fileName, response);
                    }
                }
                catch
                {
                    if (response != null) response.StatusCode = 500;
                }
                finally
                {
                    if (response != null) response.Close();
                }
            }
        }

        private void ServeFile(string fileName, HttpListenerResponse response)
        {
            string fullPath = fileName.Contains(":") ? fileName : _webRoot + fileName;
            if (File.Exists(fullPath))
            {
                byte[] content = File.ReadAllBytes(fullPath);
                response.ContentType = GetMimeType(fileName);
                response.ContentLength64 = content.Length;
                response.OutputStream.Write(content, 0, content.Length);
            }
            else
            {
                response.StatusCode = 404;
            }
        }

        private void AddAviso(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string novoItem = ReadRequestBody(request).Trim();

                if (novoItem.Length > 0 && novoItem[0] == '{')
                {
                    lock (_dbPath)
                    {
                        string conteudo = File.Exists(_dbPath) ? File.ReadAllText(_dbPath).Trim() : "[]";
                        string resultado;

                        if (conteudo == "[]" || string.IsNullOrEmpty(conteudo))
                        {
                            resultado = "[" + novoItem + "]";
                        }
                        else
                        {
                            int lastBracket = conteudo.LastIndexOf(']');
                            resultado = conteudo.Substring(0, lastBracket) + "," + novoItem + "]";
                        }
                        File.WriteAllText(_dbPath, resultado);
                    }
                }
                SendJsonResponse(response, "{\"s\":\"ok\"}");
            }
            catch { response.StatusCode = 500; }
        }

        private void DeleteAviso(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                int indexToDelete = int.Parse(ReadRequestBody(request));

                lock (_dbPath)
                {
                    if (File.Exists(_dbPath))
                    {
                        string conteudo = File.ReadAllText(_dbPath).Trim();
                        
                        if (conteudo.StartsWith("[") && conteudo.EndsWith("]"))
                        {
                            string interior = conteudo.Substring(1, conteudo.Length - 2);
                            string[] itens = SplitJsonArray(interior);

                            if (indexToDelete >= 0 && indexToDelete < itens.Length)
                            {
                                string novoJson = "[";
                                bool primeiro = true;
                                for (int i = 0; i < itens.Length; i++)
                                {
                                    if (i == indexToDelete) continue;
                                    if (!primeiro) novoJson += ",";
                                    novoJson += itens[i];
                                    primeiro = false;
                                }
                                novoJson += "]";
                                File.WriteAllText(_dbPath, novoJson);
                            }
                        }
                    }
                }
                SendJsonResponse(response, "{\"s\":\"deleted\"}");
            }
            catch { response.StatusCode = 500; }
        }

        private string[] SplitJsonArray(string interior)
        {
            
            var list = new System.Collections.ArrayList();
            int level = 0;
            int start = 0;
            for (int i = 0; i < interior.Length; i++)
            {
                if (interior[i] == '{') level++;
                else if (interior[i] == '}')
                {
                    level--;
                    if (level == 0)
                    {
                        list.Add(interior.Substring(start, i - start + 1).Trim().TrimStart(','));
                        start = i + 1;
                    }
                }
            }
            string[] res = new string[list.Count];
            for (int i = 0; i < list.Count; i++) res[i] = (string)list[i];
            return res;
        }

        private string ReadRequestBody(HttpListenerRequest request)
        {
            int length = (int)request.ContentLength64;
            byte[] buffer = new byte[length];
            request.InputStream.Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }

        private void SendJsonResponse(HttpListenerResponse response, string json)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Flush();
        }

        private string GetMimeType(string file)
        {
            string f = file.ToLower();
            if (f.EndsWith(".html")) return "text/html";
            if (f.EndsWith(".js")) return "application/javascript";
            if (f.EndsWith(".css")) return "text/css";
            if (f.EndsWith(".json")) return "application/json";
            return "application/octet-stream";
        }
    }
}