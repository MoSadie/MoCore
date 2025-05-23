﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MoCore
{
    /**
     * An example implementation of MoHttpHandler,
     * supports a few different paths
     */
    internal class MoCoreHttpHandler : IMoHttpHandler
    {
        private readonly MoCore moCore;

        public MoCoreHttpHandler(MoCore moCore)
        {
            this.moCore = moCore;
        }

        public string GetPrefix()
        {
            return "mocore";
        }

        public System.Net.HttpListenerResponse HandleRequest(System.Net.HttpListenerRequest request, System.Net.HttpListenerResponse response)
        {
            // paths:
            // mocore/version - info on MoCore version
            // mocore/plugins - list of all registered plugins
            // mocore/plugin/{pluginGUID} - info on a specific plugin
            // mocore/variable/parse?string={message} - parse a message with variables

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
                foreach (IMoPlugin plugin in MoCore.GetPlugins())
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
                IMoPlugin plugin = null;
                foreach (IMoPlugin p in MoCore.GetPlugins())
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
            else if (parts[1] == "variable" && parts.Length > 2 && parts[2] == "parse")
            {
                if (!MoCore.IsSafe)
                {
                    response.StatusCode = 503;
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    return response;
                }

                string message = request.QueryString["string"];
                if (message != null)
                {
                    string parsedMessage = VariableHandler.ParseVariables(message);
                    if (parsedMessage == null)
                    {
                        response.StatusCode = 500;
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        return response;
                    }
                    response.StatusCode = 200;
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(parsedMessage);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = 400;
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

            internal JsonPluginInfo(IMoPlugin plugin)
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
