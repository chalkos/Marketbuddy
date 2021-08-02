using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketbuddy.Common
{
    internal static class CloseButtons
    {
        private static Dictionary<string, int> closeButtonIndexes = new Dictionary<string, int>()
        {
            {"ItemSearchResult", 6 },
        };

        internal static unsafe AtkComponentNode* getCloseButton(AtkUnitBase* addon)
        {
            int index;
            string name = Encoding.UTF8.GetString(addon->Name, 16);
            if (name != "ItemSearchResult" || !closeButtonIndexes.TryGetValue(name, out index))
                return null;

            AtkUldManager uldMgr = addon->WindowNode->Component->UldManager;
            var ret = (uldMgr.NodeListCount > index ? uldMgr.NodeList[index]->GetComponent()->OwnerNode : null);
            PluginLog.Information($"Got button for {name}({(UInt64)addon:X}) from nodes[{index}] with addr: {(UInt64)ret:X}");
            return ret;
        }
    }
}
