using System.Collections.Generic;
using System.Text;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Marketbuddy.Common
{
    internal static class CloseButtons
    {
        private static readonly Dictionary<string, int> closeButtonIndexes = new()
        {
            {"ItemSearchResult", 6}
        };

        internal static unsafe AtkComponentNode* getCloseButton(AtkUnitBase* addon)
        {
            int index;
            var name = Encoding.UTF8.GetString(addon->Name, 16);
            if (name != "ItemSearchResult" || !closeButtonIndexes.TryGetValue(name, out index))
                return null;

            var uldMgr = addon->WindowNode->Component->UldManager;
            var ret = uldMgr.NodeListCount > index ? uldMgr.NodeList[index]->GetComponent()->OwnerNode : null;
            PluginLog.Information(
                $"Got button for {name}({(ulong) addon:X}) from nodes[{index}] with addr: {(ulong) ret:X}");
            return ret;
        }
    }
}