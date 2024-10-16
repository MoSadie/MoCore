using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoCore
{
    public interface MoPlugin
    {
        public String GetCompatibleGameVersion();
        public String GetVersionCheckUrl();

        public BaseUnityPlugin GetPluginObject();
    }
}
