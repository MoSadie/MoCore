using System.Net;

namespace MoCore
{
    /**
     * <summary>
     * An interface for handling HTTP requests directed for plugins.
     * An object inheriting this interface should be returned from <see cref="IMoPlugin.GetHttpHandler()"/>.
     * </summary>
     */
    public interface IMoHttpHandler
    {
        /**
         * <summary>
         * The Prefix used in http requests to this handler.
         * For example the prefix <c>slipinfo</c> would translate to a path like <c>localhost:8001/slipinfo/whatever</c>
         * </summary>
         */
        public string GetPrefix();

        /**
         * <summary>
         * The method that will be called when a request is made to this handler.
         * </summary>
         * 
         * <param name="request">The request that was made.</param>
         * <param name="response">The response that will be sent back. You do not need to close the response object, that will be handled for you.</param>
         * 
         * <returns>The response, after the handler is done writing to it. </returns>
         */
        public HttpListenerResponse HandleRequest(HttpListenerRequest request, HttpListenerResponse response);
    }
}