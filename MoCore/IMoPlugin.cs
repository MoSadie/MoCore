using BepInEx;
using System;

namespace MoCore
{
    public interface IMoPlugin
    {
        /**
         * <summary>
         * The version of Slipstream: Rogue Space your plugin was compiled for.
         * (You can find this in the log file, MoCore prints it to the console during launch)
         * 
         * This is used in case we are unable to reach the version json file from <see cref="GetVersionCheckUrl"/>.
         * You can return null to disable local version checking.
         * </summary>
         */
        public string GetCompatibleGameVersion();

        /**
         * <summary>
         * The URL to a json file that contains the version of Slipstream: Rogue Space your plugin is compatible with.
         * 
         * Return null if you do not want to use online version checking.
         * 
         * The plugin version is what is provided in the BepInEx plugin info.
         *
         * The json file should be in the following format:
         * <code>
         * {
         *     "(plugin version 1)": [
         *         "(game version 1)",
         *         "(game version 2)"
         *     ],
         *     "(plugin version 2)": [
         *         "(game version 1)",
         *         "(game version 2)"
         *     ]
         * }
         * </code>
         * </summary>
         */
        public string GetVersionCheckUrl();

        /**
         * <summary>
         * If your plugin wants to use the HTTP server, return an object implementing <see cref="IMoHttpHandler"/>.
         * (Can be implemented by the plugin class itself or a separate class)
         * 
         * Return null if you do not need this feature.
         *
         * Multiple calls should result in the same object each time.
         * </summary>
         */
        public IMoHttpHandler GetHttpHandler();

        /**
         * <summary>
         * This is your plugin's object. Most plugins should just be able to return using the this keyword and be done.
         * </summary>
         */
        public BaseUnityPlugin GetPluginObject();
    }
}
