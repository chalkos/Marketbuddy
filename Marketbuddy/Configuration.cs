using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace Marketbuddy
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        // the below exist just to make saving less cumbersome

        [NonSerialized] private DalamudPluginInterface pluginInterface;

        public bool HoldShiftToStop { get; set; } = true;
        public bool AutoOpenComparePrices { get; set; } = true;
        public bool AutoOpenHistory { get; set; } = true;
        public bool SaveToClipboard { get; set; } = true;
        public bool AutoInputNewPrice { get; set; } = true;
        public bool AutoConfirmNewPrice { get; set; } = true;
        public bool HoldCtrlToPaste { get; set; } = true;
        
        public bool UseMaxStackSize { get; set; } = false;
        public int MaximumStackSize { get; set; } = 99;
        
        public int Version { get; set; } = 0;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface.SavePluginConfig(this);
        }
    }
}