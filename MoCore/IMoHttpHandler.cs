using System.Net;

namespace MoCore
{
    public interface IMoHttpHandler
    {
        /**
         * The Prefix used in http requests to this handler.
         * For example the prefix slipinfo would translate to a path like localhost:8001/slipinfo/whatever
         */
        public string GetPrefix();

        /**
         * The method that will be called when a request is made to this handler.
         * 
         * @param request The request that was made
         * @param response The response that will be sent back
         * 
         * @returns The response, after the handler is done writing to it. 
         */
        public HttpListenerResponse HandleRequest(HttpListenerRequest request, HttpListenerResponse response);
    }
}