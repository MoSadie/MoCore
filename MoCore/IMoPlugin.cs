using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoCore
{
    public interface IMoPlugin
    {
        public String GetCompatibleGameVersion();
        public String GetVersionCheckUrl();

        /**
         * If your plugin wants to use the HTTP server, return an object implementing MoHttpHandler.
         * Return null if you do not need this feature.
         * 
         * Multiple calls should result in the same object each time.
         */
        public IMoHttpHandler GetHttpHandler();

        public BaseUnityPlugin GetPluginObject();
    }
}
