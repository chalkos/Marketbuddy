using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Marketbuddy
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool HoldShiftToStop { get; set; } = true;
        public bool AutoOpenComparePrices { get; set; } = true;
        public bool AutoOpenHistory { get; set; } = true;
        public bool SaveToClipboard { get; set; } = true;
        public bool AutoInputNewPrice { get; set; } = true;
        public bool AutoConfirmNewPrice { get; set; } = true;
        public bool HoldCtrlToPaste { get; set; } = true;

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
