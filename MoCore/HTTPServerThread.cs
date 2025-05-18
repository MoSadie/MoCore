using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace MoCore
{
     /**
      * <summary>
      * A simple implementation of HttpListener to handle HTTP requests.
      * 
      * Checks the path and attempts to pass the request to the appropriate handler (registered via MoCore).
      * </summary>
      */
    internal class HTTPServerThread
    {
        private HttpListener httpListener;

        private Thread thread = null;

        /**
         * <summary>
         * Prepare a new instance of HTTPServerThread.
         * You will need to call <see cref="StartListening"/> to start the server.
         * </summary>
         * <param name="port">The port to listen on.</param>
         */
        internal HTTPServerThread(int port)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://localhost:{port}/");
            httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }

         /**
          * <summary>
          * Starts the HTTP server thread.
          * </summary>
          */
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

        /**
         * <summary>
         * Stops the HTTP server thread.
         * </summary>
         */
        internal void StopListening()
        {
            try
            {
                if (httpListener == null || thread == null)
                {
                    MoCore.Log.LogError("HTTP server thread is not running.");
                    return;
                }
                httpListener.Stop();
            }
            catch (Exception e)
            {
                MoCore.Log.LogError($"Failed to stop HTTP server thread: {e.Message}");
            }
        }

        /**
         * <summary>
         * The main loop of the HTTP server thread.
         * ONLY CALLED FROM THE NEW SERVER THREAD.
         * </summary>
         * <param name="httpListener">The HttpListener instance to use.</param>
         */
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
