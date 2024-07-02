using System.Collections.Generic;

namespace Marketbuddy
{
    internal static class IPCManager
    {
        internal static HashSet<string> Locks = new();
        internal static bool IsLocked => Locks.Count > 0;
        
        internal static void Init()
        {
            Marketbuddy.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.Lock").RegisterFunc(Locks.Add);
            Marketbuddy.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.Unlock").RegisterFunc(Locks.Remove);
            Marketbuddy.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.IsLocked").RegisterFunc((str) => str == null?IsLocked:Locks.Contains(str));
        }

        internal static void Shutdown()
        {
            Marketbuddy.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.Lock").UnregisterFunc();
            Marketbuddy.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.Unlock").UnregisterFunc();
            Marketbuddy.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.IsLocked").UnregisterFunc();
        }
    }
}
