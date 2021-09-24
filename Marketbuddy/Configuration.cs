using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;
using static Marketbuddy.Common.Dalamud;

namespace Marketbuddy
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public bool HoldShiftToStop { get; set; } = true;
        public bool AutoOpenComparePrices { get; set; } = true;
        public bool AutoOpenHistory { get; set; } = true;
        public bool SaveToClipboard { get; set; } = true;
        public bool AutoInputNewPrice { get; set; } = true;
        public bool AutoConfirmNewPrice { get; set; } = true;
        public bool HoldCtrlToPaste { get; set; } = true;
        
        public bool AdjustMaxStackSizeInSellList { get; set; } = true;
        public Vector2 AdjustMaxStackSizeInSellListOffset { get; set; } = new Vector2(77,10);
        public bool UseMaxStackSize { get; set; } = false;
        public int MaximumStackSize { get; set; } = 99;
        
        public int Version { get; set; } = 0;

        // the below exist just to make saving/loading less cumbersome
        [NonSerialized]
        private static Configuration? _cachedConfig;

        public void Save()
        {
            PluginInterface.SavePluginConfig(this);
        }

        public static Configuration GetOrLoad()
        {
            if (_cachedConfig != null)
                return _cachedConfig;
            
            if (PluginInterface.GetPluginConfig() is not Configuration config)
            {
                config = new Configuration();
                config.Save();
            }
            
            _cachedConfig = config;
            return _cachedConfig;
        }
    }
}