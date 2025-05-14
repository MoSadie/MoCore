using BepInEx;
using BepInEx.Configuration;
using System.Net.Http;
using BepInEx.Logging;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MoCore
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Slipstream_Win.exe")]
    public class MoCore : BaseUnityPlugin
    {
        private static ConfigEntry<bool> overrideVersionCheck;

        private static ConfigEntry<int> httpServerPort;

        private static HttpClient httpClient = new HttpClient();

        private static List<MoPlugin> plugins = new List<MoPlugin>();

        private static Dictionary<string, MoHttpHandler> httpHandlers = new Dictionary<string, MoHttpHandler>();

        internal static ManualLogSource Log;
        private void Awake()
        {
            try
            {
                Log = base.Logger;

                Log.LogInfo($"Game version: {Application.version}");

                overrideVersionCheck = Config.Bind("BE CAREFUL", "Override Version Check", false, "This will allow my plugins to run on any version of Slipstream, skipping the version checker. Use at your own risk.");

                httpServerPort = Config.Bind("HTTP", "HTTP Server Port", 8001, "The port to use for the HTTP server. This is used for any web requests plugins wish to accept.");

                httpClient.Timeout = TimeSpan.FromSeconds(5);

                // Plugin startup logic
                Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            }
            catch (Exception e)
            {
                Log.LogError($"An error occurred during plugin startup: {e.Message}");
            }

        }

        /**
         * Register a plugin with MoCore, also checks if the plugin is compatible with the current game version.
         * 
         * @param plugin The plugin to register
         * @returns true if the plugin should continue to load, false otherwise.
         */
        public static bool RegisterPlugin(MoPlugin plugin, MoHttpHandler httpHandler = null)
        {
            Log.LogInfo($"Registering plugin {PluginName(plugin)} ({PluginId(plugin)})");
            Log.LogInfo($"Version: {PluginVersion(plugin)}");

            if (overrideVersionCheck.Value)
            {
                Log.LogInfo("Version check override is enabled. Skipping version check.");
                return httpHandler != null ? RegisterHttpHandler(httpHandler, plugin) : true;
            }

            if (plugin.GetVersionCheckUrl() != null)
            {
                if (!VersionCheck(plugin, Application.version))
                {
                    return false;
                }
            }


            Log.LogInfo($"Plugin {PluginName(plugin)} ({PluginId(plugin)}) is compatible with this version of the game ({Application.version}).");
            plugins.Add(plugin);
            return httpHandler != null ? RegisterHttpHandler(httpHandler, plugin) : true;
        }

        /**
         * Register a MoHttpHandler with MoCore.
         * 
         * @param httpHandler The MoHttpHandler to register
         * @param plugin The plugin that this handler is for
         * 
         * @returns true if the handler was registered successfully, false otherwise.
         */
        private static bool RegisterHttpHandler(MoHttpHandler httpHandler, MoPlugin plugin)
        {
            Log.LogInfo($"Attempting to register http handler {httpHandler.getPrefix()} for plugin {PluginName(plugin)} ({PluginId(plugin)})");
            if (httpHandler == null)
            {
                Log.LogDebug($"HttpHandler is null. Skipping");
                return true;
            }

            if (plugin == null)
            {
                Log.LogError($"Plugin is null. Skipping");
                return false;
            }

            if (httpHandler.getPrefix() == null)
            {
                Log.LogError($"HttpHandler prefix is null. Skipping");
                return false;
            }

            if (httpHandler.getPrefix().Length == 0)
            {
                Log.LogError($"HttpHandler prefix is empty. Skipping");
                return false;
            }

            if (httpHandler.getPrefix().Contains(" "))
            {
                Log.LogError($"HttpHandler prefix contains spaces. Skipping");
                return false;
            }

            if (httpHandler.getPrefix().Contains("/"))
            {
                Log.LogError($"HttpHandler prefix contains slashes. Skipping");
                return false;
            }

            if (httpHandlers.ContainsKey(httpHandler.getPrefix()))
            {
                Log.LogError($"HttpHandler prefix {httpHandler.getPrefix()} is already registered. Skipping");
                return false;
            }

            Log.LogInfo($"Registering http handler {httpHandler.getPrefix()} for plugin {PluginName(plugin)} ({PluginId(plugin)})");
            httpHandlers.Add(httpHandler.getPrefix(), httpHandler);
            return true;
        }

        public static bool isRegisteredPlugin(MoPlugin plugin)
        {
            return plugins.Contains(plugin);
        }

        public static List<MoPlugin> GetPlugins()
        {
            return new List<MoPlugin>(plugins);
        }

        internal static Dictionary<string, MoHttpHandler> GetHttpHandlers()
        {
            return httpHandlers;
        }

        private static bool VersionCheck(MoPlugin plugin, string gameVersion)
        {
            try
            {
                // Retrieve the JSON map of plugin versions to compatible game versions
                string jsonMap = httpClient.GetStringAsync(plugin.GetVersionCheckUrl()).GetAwaiter().GetResult();

                // Parse the JSON map into a dictionary
                Dictionary<string, HashSet<string>> versionMap = JsonConvert.DeserializeObject<Dictionary<string, HashSet<string>>>(jsonMap);

                // Check if the plugin version is compatible with the game version
                if (versionMap.ContainsKey(PluginVersion(plugin)) && versionMap[PluginVersion(plugin)].Contains(Application.version))
                {
                    return true;
                }
                else
                {
                    if (versionMap.ContainsKey(PluginVersion(plugin)))
                    {
                        Log.LogError($"Version {PluginVersion(plugin)} of {PluginName(plugin)} ({PluginId(plugin)}) is not compatible with this version of the game ({Application.version}). Please check for updates.");
                        return false;
                    }
                    else
                    {
                        Log.LogError($"Version {PluginVersion(plugin)} of {PluginName(plugin)} ({PluginId(plugin)}) is not listed in the version check file. Please contact the plugin's creator.");
                        return false;
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Log.LogError($"Version check timed out. Falling back to hardcoded version check.");
                return plugin.GetCompatibleGameVersion().Equals(gameVersion);
            }
            catch (Exception e)
            {
                Log.LogError($"An error occurred during remote version check, falling back to hardcoded version check: {e.Message}");
                return plugin.GetCompatibleGameVersion().Equals(gameVersion);
            }
        }

        private static String PluginName(MoPlugin plugin)
        {
            return plugin.GetPluginObject().Info.Metadata.Name;
        }

        private static String PluginId(MoPlugin plugin)
        {
            return plugin.GetPluginObject().Info.Metadata.GUID;
        }

        private static String PluginVersion(MoPlugin plugin)
        {
            return plugin.GetPluginObject().Info.Metadata.Version.ToString();
        }
    }
}
