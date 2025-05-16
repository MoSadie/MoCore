using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace MoCore
{
    internal class HTTPServerThread
    {
        private HttpListener httpListener;

        private Thread thread = null;

        internal HTTPServerThread(int port)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://localhost:{port}/");
            httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }

        internal void StartListening()
        {
            try
            {

                if (thread != null)
                {
                    MoCore.Log.LogError("HTTP server thread is already running.");
                    return;
                }
                thread = new Thread(() => StartThread(httpListener));
                thread.Start();
            }
            catch (Exception e)
            {
                MoCore.Log.LogError($"Failed to start HTTP server thread: {e.Message}");
            }
        }

        private void StartThread(HttpListener httpListener)
        {
            try
            {
                httpListener.Start();
                MoCore.Log.LogInfo("HTTP server started.");
                while (httpListener.IsListening)
                {
                    HttpListenerContext context = httpListener.GetContext();

                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    string path = request.Url.AbsolutePath.Trim('/');

                    string prefix = path.Split('/')[0];

                    MoCore.Log.LogInfo($"HTTP request received: Prefix:{prefix} {path}");

                    Dictionary<string, IMoHttpHandler> handlers = MoCore.GetHttpHandlers();

                    if (handlers.ContainsKey(prefix))
                    {
                        IMoHttpHandler handler = handlers[prefix];
                        MoCore.Log.LogInfo($"HTTP request handled by {handler.GetPrefix()}");
                        response = handler.HandleRequest(request, response);
                    }
                    else
                    {
                        MoCore.Log.LogDebug($"HTTP request not handled");
                        response.StatusCode = 404;
                        byte[] buffer = Encoding.UTF8.GetBytes("Not Found");
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    response.Close();
                }
            }
            catch (Exception e)
            {
                MoCore.Log.LogError($"HTTP server error: {e.Message}");
            }
        }

    }
}
