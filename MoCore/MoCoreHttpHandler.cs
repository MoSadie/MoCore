using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MoCore
{
    /**
     * An example implementation of MoHttpHandler,
     * supports a few different paths
     */
    internal class MoCoreHttpHandler : MoHttpHandler
    {
        private MoCore moCore;

        public MoCoreHttpHandler(MoCore moCore)
        {
            this.moCore = moCore;
        }

        public string getPrefix()
        {
            return "mocore";
        }

        public System.Net.HttpListenerResponse handleRequest(System.Net.HttpListenerRequest request, System.Net.HttpListenerResponse response)
        {
            // paths:
            // mocore/version
            // mocore/plugins
            // mocore/plugin/{pluginGUID}

            string path = request.Url.AbsolutePath.Trim('/');
            string[] parts = path.Split('/');

            if (parts.Length < 2 || parts[0] != "mocore")
            {
                response.StatusCode = 404;
                return response;
            }

            if (parts[1] == "version")
            {
                response.StatusCode = 200;
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(moCore.GetPluginObject().Info.Metadata.Version.ToString());
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
                return response;
            }
            else if (parts[1] == "plugins")
            {
                List<JsonPluginInfo> list = new List<JsonPluginInfo>();
                foreach (MoPlugin plugin in MoCore.GetPlugins())
                {
                    list.Add(new JsonPluginInfo(plugin));
                }

                string json = JsonConvert.SerializeObject(list, Formatting.Indented);

                response.StatusCode = 200;
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.ContentType = "application/json";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                return response;
            }
            else if (parts[1] == "plugin" && parts.Length > 2)
            {
                MoPlugin plugin = null;
                foreach (MoPlugin p in MoCore.GetPlugins())
                {
                    if (p.GetPluginObject().Info.Metadata.GUID == parts[2])
                    {
                        plugin = p;
                        break;
                    }
                }

                if (plugin != null)
                {
                    JsonPluginInfo info = new JsonPluginInfo(plugin);
                    string json = JsonConvert.SerializeObject(info, Formatting.Indented);

                    response.StatusCode = 200;
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.ContentType = "application/json";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = 404;
                }
            }
            else
            {
                response.StatusCode = 404;
            }
            return response;
        }

        class JsonPluginInfo
        {

            public string Name { get; set; }
            public string Version { get; set; }
            public string GUID { get; set; }

            public string VersionCheckUrl { get; set; }

            public string FallbackGameVersion { get; set; }

            internal JsonPluginInfo(MoPlugin plugin)
            {
                Name = plugin.GetPluginObject().Info.Metadata.Name;
                Version = plugin.GetPluginObject().Info.Metadata.Version.ToString();
                GUID = plugin.GetPluginObject().Info.Metadata.GUID;
                VersionCheckUrl = plugin.GetVersionCheckUrl();
                FallbackGameVersion = plugin.GetCompatibleGameVersion();
            }
        }
    }
}
