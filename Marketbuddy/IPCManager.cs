using Marketbuddy.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svc = Marketbuddy.Common.Dalamud;

namespace Marketbuddy
{
    internal static class IPCManager
    {
        internal static HashSet<string> Locks = new();
        internal static bool IsLocked => Locks.Count > 0;
        
        internal static void Init()
        {
            Svc.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.Lock").RegisterFunc(Locks.Add);
            Svc.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.Unlock").RegisterFunc(Locks.Remove);
            Svc.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.IsLocked").RegisterFunc((str) => str == null?IsLocked:Locks.Contains(str));
        }

        internal static void Shutdown()
        {
            Svc.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.Lock").UnregisterFunc();
            Svc.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.Unlock").UnregisterFunc();
            Svc.PluginInterface.GetIpcProvider<string, bool>("Marketbuddy.IsLocked").UnregisterFunc();
        }
    }
}
