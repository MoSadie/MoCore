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
    public class MoCore : BaseUnityPlugin, IMoPlugin
    {
        private static ConfigEntry<bool> overrideVersionCheck;

        private static ConfigEntry<int> httpServerPort;

        private static HttpClient httpClient = new HttpClient();

        private static List<IMoPlugin> plugins = new List<IMoPlugin>();

        private static Dictionary<string, IMoHttpHandler> httpHandlers = new Dictionary<string, IMoHttpHandler>();

        private static HTTPServerThread httpServerThread = null;

        internal static ManualLogSource Log;

        // Information for registering ourselves
        public static readonly string COMPATIBLE_GAME_VERSION = "4.1595";
        public static readonly string GAME_VERSION_URL = "https://raw.githubusercontent.com/MoSadie/MoCore/refs/heads/main/versions.json";

        // Used for HttpHandler
        private MoCoreHttpHandler moCoreHttpHandler;

        public static bool IsSafe { get; private set; } = false;

        private void Awake()
        {
            try
            {
                Log = base.Logger;

                Log.LogInfo($"Game version: {Application.version}");

                overrideVersionCheck = Config.Bind("BE CAREFUL", "Override Version Check", false, "This will allow my plugins to run on any version of Slipstream, skipping the version checker. Use at your own risk.");

                httpServerPort = Config.Bind("HTTP", "HTTP Server Port", 8001, "The port to use for the HTTP server. This is used for any web requests plugins wish to accept.");

                httpClient.Timeout = TimeSpan.FromSeconds(5);

                httpServerThread = new HTTPServerThread(httpServerPort.Value);

                httpServerThread.StartListening();

                moCoreHttpHandler = new MoCoreHttpHandler(this);

                // Register ourselves! (Note: Your plugin should do this first thing in Awake, but MoCore does this later since we need to setup the registration system first)
                IsSafe = RegisterPlugin(this);

                // Plugin startup logic
                Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            }
            catch (Exception e)
            {
                Log.LogError($"An error occurred during plugin startup: {e.Message}");
            }

        }

        /**
         * Register a plugin with MoCore, and does the following:
         * - (If not skipped) Check plugin version acceptable versions against game version
         * - (If enabled) Register the plugin's HTTP handler
         * 
         * @param plugin The plugin to register
         * @param skipVersionCheck If true, skip the version check.
         * @returns true if plugin registered correctly, false otherwise. (If false: continuing to load plugin may lead to issues)
         */
        public static bool RegisterPlugin(IMoPlugin plugin, bool skipVersionCheck = false)
        {
            Log.LogInfo($"Registering plugin {PluginName(plugin)} ({PluginId(plugin)})");
            Log.LogInfo($"Version: {PluginVersion(plugin)}");

            if (overrideVersionCheck.Value || skipVersionCheck)
            {
                Log.LogWarning("Version check override is enabled. Skipping version check.");
                plugins.Add(plugin);
                return RegisterHttpHandler(plugin);
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
            return RegisterHttpHandler(plugin);
        }

        /**
         * Register a MoHttpHandler with MoCore.
         * Will automatically skip if the plugin does not request the feature.
         * 
         * @param plugin The plugin which can provide the handler.
         * 
         * @returns true if the handler was registered successfully, false otherwise.
         */
        private static bool RegisterHttpHandler(IMoPlugin plugin)
        {
            if (plugin == null)
            {
                Log.LogError("Attempted to RegisterHttpHandler with a null plugin!!!");
                return false;
            }

            IMoHttpHandler httpHandler = plugin.GetHttpHandler();

            if (httpHandler == null)
            {
                Log.LogDebug($"HttpHandler is null for plugin {PluginName(plugin)} ({PluginId(plugin)}). Skipping (this is normal, null means does not need feature)");
                return true;
            }

            Log.LogInfo($"Attempting to register http handler {httpHandler.GetPrefix()} for plugin {PluginName(plugin)} ({PluginId(plugin)})");

            if (httpHandler.GetPrefix() == null)
            {
                Log.LogError($"HttpHandler prefix is null. Skipping");
                return false;
            }

            if (httpHandler.GetPrefix().Length == 0)
            {
                Log.LogError($"HttpHandler prefix is empty. Skipping");
                return false;
            }

            if (httpHandler.GetPrefix().Contains(" "))
            {
                Log.LogError($"HttpHandler prefix contains spaces. Skipping");
                return false;
            }

            if (httpHandler.GetPrefix().Contains("/"))
            {
                Log.LogError($"HttpHandler prefix contains slashes. Skipping");
                return false;
            }

            if (httpHandlers.ContainsKey(httpHandler.GetPrefix()))
            {
                Log.LogError($"HttpHandler prefix {httpHandler.GetPrefix()} is already registered. Skipping");
                return false;
            }

            Log.LogInfo($"Registering http handler {httpHandler.GetPrefix()} for plugin {PluginName(plugin)} ({PluginId(plugin)})");
            httpHandlers.Add(httpHandler.GetPrefix(), httpHandler);
            return true;
        }

        public static bool IsRegisteredPlugin(IMoPlugin plugin)
        {
            return plugins.Contains(plugin);
        }

        public static List<IMoPlugin> GetPlugins()
        {
            return new List<IMoPlugin>(plugins);
        }

        internal static Dictionary<string, IMoHttpHandler> GetHttpHandlers()
        {
            return httpHandlers;
        }

        private static bool VersionCheck(IMoPlugin plugin, string gameVersion)
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

        private static String PluginName(IMoPlugin plugin)
        {
            return plugin.GetPluginObject().Info.Metadata.Name;
        }

        private static String PluginId(IMoPlugin plugin)
        {
            return plugin.GetPluginObject().Info.Metadata.GUID;
        }

        private static String PluginVersion(IMoPlugin plugin)
        {
            return plugin.GetPluginObject().Info.Metadata.Version.ToString();
        }

        // From MoPlugin
        public String GetCompatibleGameVersion()
        {
            return COMPATIBLE_GAME_VERSION;
        }

        public String GetVersionCheckUrl()
        {
            return GAME_VERSION_URL;
        }

        public BaseUnityPlugin GetPluginObject()
        {
            return this;
        }

        public IMoHttpHandler GetHttpHandler()
        {
            return moCoreHttpHandler;
        }
    }
}
