using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Marketbuddy.Common;

namespace Marketbuddy
{
    public unsafe class Plugin : IDalamudPlugin
    {
        private const string commandName = "/mbuddy";
        private const string commandNameConfig = "/mbuddyconf";
        private HookWrapper<AddonItemSearchResult_OnSetup_Delegate> AddonItemSearchResult_OnSetup_HW;
        private HookWrapper<AddonItemSearchResult_ReceiveEvent_Delegate> AddonItemSearchResult_ReceiveEvent_HW;
        private HookWrapper<AddonRetainerSell_OnSetup_Delegate> AddonRetainerSell_OnSetup_HW;

        internal DalamudPluginInterface Interface { get; private set; }
        internal Configuration Configuration { get; private set; }
        internal PluginUI UI { get; private set; }
        internal Commons Common { get; private set; }

        // When loaded by LivePluginLoader, the executing assembly will be wrong.
        // Supplying this property allows LivePluginLoader to supply the correct location, so that
        // you have full compatibility when loaded normally and through LPL.
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;
        public string Name => "Marketbuddy";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface;

            Resolver.Initialize();

            Configuration = Interface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(Interface);

            UI = new PluginUI(this);
            Common = new Commons(Interface);

            AddonItemSearchResult_ReceiveEvent_HW = Commons.Hook<AddonItemSearchResult_ReceiveEvent_Delegate>(
                "4C 8B DC 53 56 48 81 EC ?? ?? ?? ?? 49 89 6B 08",
                AddonItemSearchResult_ReceiveEvent_Delegate_Detour);

            AddonRetainerSell_OnSetup_HW = Commons.Hook<AddonRetainerSell_OnSetup_Delegate>(
                "48 89 5C 24 ?? 55 56 57 48 83 EC 50 4C 89 64 24",
                AddonRetainerSell_OnSetup_Delegate_Detour);

            AddonItemSearchResult_OnSetup_HW = Commons.Hook<AddonItemSearchResult_OnSetup_Delegate>(
                "40 53 41 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 89 AC 24",
                AddonItemSearchResult_OnSetup_Delegate_Detour);

            Interface.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Does nothing for now."
            });

            Interface.CommandManager.AddHandler(commandNameConfig, new CommandInfo(OnCommand)
            {
                HelpMessage = "Shows the configuration window."
            });

            Interface.UiBuilder.OnBuildUi += DrawUI;
            Interface.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
        }

        public void Dispose()
        {
            UI.Dispose();
            Common.Dispose();
            Interface.CommandManager.RemoveHandler(commandName);
            Interface.CommandManager.RemoveHandler(commandNameConfig);
            Interface.Dispose();
        }

        private IntPtr AddonRetainerSell_OnSetup_Delegate_Detour(void* addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonRetainerSell.OnSetup");
            var result = AddonRetainerSell_OnSetup_HW.Original(addon, a2, dataPtr);

            if (Configuration.HoldCtrlToPaste && Control.ModifierKeys == Keys.Control)
            {
                var cbValue = Clipboard.GetText();
                if (int.TryParse(cbValue, out var priceValue) && priceValue > 0)
                    SetPrice(priceValue);
                else
                    Interface.Framework.Gui.Chat.Print("Clipboard does not contain a valid price");
            }
            else if (Configuration.AutoOpenComparePrices &&
                     (!Configuration.HoldShiftToStop || Control.ModifierKeys != Keys.Shift))
            {
                try
                {
                    //open compare prices list on opening sell price selection
                    var comparePrices = ((AddonRetainerSell*) addon)->ComparePrices->AtkComponentBase.OwnerNode;
                    // Client::UI::AddonRetainerSell.ReceiveEvent this=0x214C05CB480 evt=EventType.CHANGE               a3=4   a4=0x2146C18C210 (src=0x214C05CB480; tgt=0x214606863B0) a5=0xBB316FE6C8
                    Common.SendClick(new IntPtr(addon), EventType.CHANGE, 4, comparePrices);
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Houston, we have a problem");
                }
            }


            return result;
        }

        private IntPtr AddonItemSearchResult_OnSetup_Delegate_Detour(void* addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonItemSearchResult.OnSetup");
            var result = AddonItemSearchResult_OnSetup_HW.Original(addon, a2, dataPtr);

            if (Configuration.AutoOpenHistory)
                try
                {
                    //open history on opening the list
                    var history = ((AddonItemSearchResult*) addon)->History->AtkComponentBase.OwnerNode;
                    //Client::UI::AddonItemSearchResult.ReceiveEvent this=0x1CC2BF42BD0 evt=EventType.CHANGE               a3=23  a4=0x1CCD86C1460 a5=0x90EF96E598
                    Common.SendClick(new IntPtr(addon), EventType.CHANGE, 23, history);
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Houston, we have a problem");
                }

            return result;
        }

        private IntPtr AddonItemSearchResult_ReceiveEvent_Delegate_Detour(IntPtr self, ushort eventType,
            uint eventParam, IntPtr eventStruct, AtkResNode* nodeParam)
        {
            var result =
                AddonItemSearchResult_ReceiveEvent_HW.Original(self, eventType, eventParam, eventStruct, nodeParam);

            if (Configuration.AutoInputNewPrice || Configuration.SaveToClipboard)
                if (eventType == 35 && nodeParam != null) // && (*eventInfoStruct) != null ) // click
                    try
                    {
                        //AtkUldManager uldManager = (*eventInfoStruct)->UldManager;
                        var price = getPricePerItem(nodeParam);
                        if (price > 0)
                            SetPrice(price - 1);
                    }
                    catch (Exception e)
                    {
                        PluginLog.Error(e.ToString());
                    }

            return result;
        }

        private int SetPrice(int newPrice)
        {
            var retainerSell = GetUnitBase("RetainerSell");
            if (retainerSell->UldManager.NodeListCount != 23)
                throw new MarketException("Unexpected fields in addon RetainerSell");

            var componentNumericInput =
                (AtkComponentNumericInput*) retainerSell->UldManager.NodeList[15]->GetComponent();
            PluginLog.Debug($"componentNumericInput: {new IntPtr(componentNumericInput).ToString("X")}");

            if (Configuration.AutoInputNewPrice)
                componentNumericInput->SetValue(newPrice);
            if (Configuration.SaveToClipboard)
                Clipboard.SetText(newPrice.ToString());

            Interface.Framework.Gui.Chat.Print($"Asking price {newPrice} set and copied to clipboard.");

            if (Configuration.AutoConfirmNewPrice)
            {
                if (!(Configuration.HoldCtrlToPaste && Control.ModifierKeys == Keys.Control))
                {
                    // Component::GUI::AtkComponentWindow.ReceiveEvent this=0x1AC801863B0 evt=EventType.CHANGE               a3=2   a4=0x1AC66640090 (src=0x1AC801863B0; tgt=0x1AC98B47EA0) a5=0x4AAAEFE388
                    var addonItemSearchResult = GetUnitBase("ItemSearchResult");
                    Common.SendClick(new IntPtr(addonItemSearchResult->WindowNode->Component), EventType.CHANGE, 2,
                        addonItemSearchResult->WindowNode->Component->UldManager
                            .NodeList[6]->GetComponent()->OwnerNode);
                }

                // Client::UI::AddonRetainerSell.ReceiveEvent this=0x214B4D360E0 evt=EventType.CHANGE               a3=21  a4=0x214B920D2E0 (src=0x214B4D360E0; tgt=0x21460686550) a5=0xBB316FE6C8
                var addonRetainerSell = (AddonRetainerSell*) retainerSell;
                Common.SendClick(new IntPtr(addonRetainerSell), EventType.CHANGE, 21, addonRetainerSell->Confirm);
            }

            return 0;
        }

        private int getPricePerItem(AtkResNode* nodeParam)
        {
            // list item renderer component
            var x = *(AtkComponentBase**) nodeParam;

            PluginLog.Debug(
                $"component={(ulong) nodeParam->GetComponent():X}, childCount={nodeParam->ChildCount}, target={*(ulong*) nodeParam:X}, gotit={(ulong) x:X}");

            if (x == null) return 0;
            var uldManager = x->UldManager;

            //nodeParam->index;


            var isMarketOpen = GetUnitBase("ItemSearch") != null;
            PluginLog.Debug("1");

            if (isMarketOpen) return 0;
            PluginLog.Debug("2");

            if (uldManager.NodeListCount < 14) return 0;
            PluginLog.Debug("3");

            var singlePriceNode = (AtkTextNode*) uldManager.NodeList[10];

            if (singlePriceNode == null)
            {
                PluginLog.Debug($"singlePriceNode == null {singlePriceNode == null}");
                return 0;
            }

            /* dunno what this does, i think simpletweaks uses this to check if the text has already been changed
            if (singlePriceNode->NodeText.StringPtr[0] == 0x20)
            {
                PluginLog.Information($"singlePriceNode {singlePriceNode->NodeText.StringPtr[0] == 0x20}; totalTextNode {totalTextNode->NodeText.StringPtr[0] == 0x20}");
            }*/

            var priceString = Common.ReadSeString(singlePriceNode->NodeText).TextValue
                .Replace($"{(char) SeIconChar.Gil}", "")
                .Replace(",", "")
                .Replace(" ", "")
                .Replace(".", "");

            PluginLog.Debug(
                $"priceString: '{priceString}', original: '{Common.ReadSeString(singlePriceNode->NodeText).TextValue}'");

            if (!int.TryParse(priceString, out var priceValue)) return 0;
            return priceValue;
        }

        private void OnCommand(string command, string args)
        {
            if (command == commandName)
                UI.Visible = true;
            else
                UI.SettingsVisible = true;
        }

        private AtkUnitBase* GetUnitBase(string name, int index = 1)
        {
            return (AtkUnitBase*) Interface.Framework.Gui.GetUiObjectByName(name, index);
        }

        private void DrawUI()
        {
            UI.Draw();
        }

        private void DrawConfigUI()
        {
            UI.SettingsVisible = true;
        }

        // __int64 __fastcall Client::UI::AddonItemSearchResult_ReceiveEvent(__int64 a1, __int16 a2, int a3, __int64 a4, __int64* a5)
        private delegate IntPtr AddonItemSearchResult_ReceiveEvent_Delegate(IntPtr self, ushort eventType,
            uint eventParam, IntPtr eventStruct, AtkResNode* nodeParam);

        // __int64 __fastcall Client::UI::AddonRetainerSell_OnSetup(__int64 a1, unsigned int a2, __int64 a3)
        private delegate IntPtr AddonRetainerSell_OnSetup_Delegate(void* addon, uint a2, IntPtr dataPtr);

        // __int64 __fastcall Client::UI::AddonItemSearchResult_OnSetup(__int64 a1, __int64 a2, __int64 a3)
        private delegate IntPtr AddonItemSearchResult_OnSetup_Delegate(void* addon, uint a2, IntPtr dataPtr);
    }
}