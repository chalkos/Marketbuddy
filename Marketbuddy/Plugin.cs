using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Text;
using Dalamud.Game.Text;
using Dalamud.Hooking;
using System.Threading.Tasks;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI;
using Marketbuddy.Common;
using System.Windows.Forms;

namespace Marketbuddy
{
    public unsafe class Plugin : IDalamudPlugin
    {
        public string Name => "Marketbuddy";

        private const string commandName = "/mbuddy";
        private const string commandNameConfig = "/mbuddyconf";

        internal DalamudPluginInterface Interface { get; private set; }
        internal Configuration Configuration { get; private set; }
        internal PluginUI UI { get; private set; }
        internal Commons Common { get; private set; }

        // __int64 __fastcall Client::UI::AddonItemSearchResult_ReceiveEvent(__int64 a1, __int16 a2, int a3, __int64 a4, __int64* a5)
        private unsafe delegate IntPtr AddonItemSearchResult_ReceiveEvent_Delegate(IntPtr self, UInt16 eventType, UInt32 eventParam, IntPtr eventStruct, AtkResNode* nodeParam);
        private HookWrapper<AddonItemSearchResult_ReceiveEvent_Delegate> AddonItemSearchResult_ReceiveEvent_HW;

        // __int64 __fastcall Client::UI::AddonRetainerSell_OnSetup(__int64 a1, unsigned int a2, __int64 a3)
        private unsafe delegate IntPtr AddonRetainerSell_OnSetup_Delegate(void* addon, uint a2, IntPtr dataPtr);
        private HookWrapper<AddonRetainerSell_OnSetup_Delegate> AddonRetainerSell_OnSetup_HW;

        // __int64 __fastcall Client::UI::AddonItemSearchResult_OnSetup(__int64 a1, __int64 a2, __int64 a3)
        private unsafe delegate IntPtr AddonItemSearchResult_OnSetup_Delegate(void* addon, uint a2, IntPtr dataPtr);
        private HookWrapper<AddonItemSearchResult_OnSetup_Delegate> AddonItemSearchResult_OnSetup_HW;

        // When loaded by LivePluginLoader, the executing assembly will be wrong.
        // Supplying this property allows LivePluginLoader to supply the correct location, so that
        // you have full compatibility when loaded normally and through LPL.
        public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
        private string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.Interface = pluginInterface;

            FFXIVClientStructs.Resolver.Initialize();

            this.Configuration = this.Interface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.Interface);

            this.UI = new PluginUI(this);
            this.Common = new Commons(Interface);

            AddonItemSearchResult_ReceiveEvent_HW = Commons.Hook<AddonItemSearchResult_ReceiveEvent_Delegate>(
                "4C 8B DC 53 56 48 81 EC ?? ?? ?? ?? 49 89 6B 08",
                this.AddonItemSearchResult_ReceiveEvent_Delegate_Detour, true);

            AddonRetainerSell_OnSetup_HW = Commons.Hook<AddonRetainerSell_OnSetup_Delegate>(
                "48 89 5C 24 ?? 55 56 57 48 83 EC 50 4C 89 64 24",
                this.AddonRetainerSell_OnSetup_Delegate_Detour, true);

            AddonItemSearchResult_OnSetup_HW = Commons.Hook<AddonItemSearchResult_OnSetup_Delegate>(
                "40 53 41 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 89 AC 24",
                this.AddonItemSearchResult_OnSetup_Delegate_Detour, true);

            this.Interface.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Does nothing for now."
            });

            this.Interface.CommandManager.AddHandler(commandNameConfig, new CommandInfo(OnCommand)
            {
                HelpMessage = "Shows the configuration window."
            });

            this.Interface.UiBuilder.OnBuildUi += DrawUI;
            this.Interface.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
        }

        private IntPtr AddonRetainerSell_OnSetup_Delegate_Detour(void* addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonRetainerSell.OnSetup");
            var result = AddonRetainerSell_OnSetup_HW.Original(addon, a2, dataPtr);

            if (Configuration.HoldCtrlToPaste && Control.ModifierKeys == Keys.Control)
            {
                string cbValue = Clipboard.GetText();
                if (int.TryParse(cbValue, out var priceValue) && priceValue > 0)
                {
                    SetPrice(priceValue);
                }
                else
                    Interface.Framework.Gui.Chat.Print("Clipboard does not contain a valid price");

            }
            else if (Configuration.AutoOpenComparePrices && (!Configuration.HoldShiftToStop || Control.ModifierKeys != Keys.Shift))
            {
                try
                {
                    //open compare prices list on opening sell price selection
                    AtkComponentNode* comparePrices = ((AddonRetainerSell*)addon)->ComparePrices->AtkComponentBase.OwnerNode;
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
            PluginLog.Debug($"AddonItemSearchResult.OnSetup");
            var result = AddonItemSearchResult_OnSetup_HW.Original(addon, a2, dataPtr);

            if (Configuration.AutoOpenHistory)
            {
                try
                {
                    //open history on opening the list
                    AtkComponentNode* history = ((AddonItemSearchResult*)addon)->History->AtkComponentBase.OwnerNode;
                    //Client::UI::AddonItemSearchResult.ReceiveEvent this=0x1CC2BF42BD0 evt=EventType.CHANGE               a3=23  a4=0x1CCD86C1460 a5=0x90EF96E598
                    Common.SendClick(new IntPtr(addon), EventType.CHANGE, 23, history);
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Houston, we have a problem");
                }
            }

            return result;
        }

        private unsafe IntPtr AddonItemSearchResult_ReceiveEvent_Delegate_Detour(IntPtr self, UInt16 eventType, UInt32 eventParam, IntPtr eventStruct, AtkResNode* nodeParam)
        {
            var result = this.AddonItemSearchResult_ReceiveEvent_HW.Original(self, eventType, eventParam, eventStruct, nodeParam);

            if (Configuration.AutoInputNewPrice || Configuration.SaveToClipboard)
            {
                if (eventType == 35 && nodeParam != null)// && (*eventInfoStruct) != null ) // click
                {
                    try
                    {
                        //AtkUldManager uldManager = (*eventInfoStruct)->UldManager;
                        int price = getPricePerItem(nodeParam);
                        if (price > 0)
                        {
                            Commons.StartSTATask(() => SetPrice(price-1))
                                .ContinueWith(t =>
                                {
                                    var aggException = t.Exception.Flatten();
                                    foreach (var e in aggException.InnerExceptions)
                                    {
                                        if (e is OperationCanceledException)
                                            continue;
                                        PluginLog.Error(e.ToString());
                                    }
                                }, TaskContinuationOptions.OnlyOnFaulted);
                        }
                    }
                    catch (Exception e)
                    {
                        PluginLog.Error(e.ToString());
                    }
                }
            }
            return result;
        }

        private int SetPrice(int newPrice)
        {
            AtkUnitBase* retainerSell = GetUnitBase("RetainerSell", 1);
            if(retainerSell->UldManager.NodeListCount != 23) throw new MarketException("Unexpected fields in addon RetainerSell");

            var componentNumericInput = (AtkComponentNumericInput*)retainerSell->UldManager.NodeList[15]->GetComponent();
            PluginLog.Debug($"componentNumericInput: {new IntPtr(componentNumericInput).ToString("X")}");

            if(Configuration.AutoInputNewPrice)
                componentNumericInput->SetValue(newPrice);
            if(Configuration.SaveToClipboard)
                Clipboard.SetText(newPrice.ToString());

            Interface.Framework.Gui.Chat.Print($"Asking price {newPrice} set and copied to clipboard.");

            if (Configuration.AutoConfirmNewPrice)
            {
                if (!(Configuration.HoldCtrlToPaste && Control.ModifierKeys == Keys.Control))
                {
                    // close results list
                    Task.Delay(50).Wait();
                    // Component::GUI::AtkComponentWindow.ReceiveEvent this=0x1AC801863B0 evt=EventType.CHANGE               a3=2   a4=0x1AC66640090 (src=0x1AC801863B0; tgt=0x1AC98B47EA0) a5=0x4AAAEFE388
                    var addonItemSearchResult = GetUnitBase("ItemSearchResult");
                    Common.SendClick(new IntPtr(addonItemSearchResult->WindowNode->Component), EventType.CHANGE, 2,
                        addonItemSearchResult->WindowNode->Component->UldManager.NodeList[6]->GetComponent()->OwnerNode);
                }

                // confirm new price
                Task.Delay(50).Wait();
                // Client::UI::AddonRetainerSell.ReceiveEvent this=0x214B4D360E0 evt=EventType.CHANGE               a3=21  a4=0x214B920D2E0 (src=0x214B4D360E0; tgt=0x21460686550) a5=0xBB316FE6C8
                var addonRetainerSell = (AddonRetainerSell*)retainerSell;
                Common.SendClick(new IntPtr(addonRetainerSell), EventType.CHANGE, 21, addonRetainerSell->Confirm);
            }
            return 0;
        }

        private int getPricePerItem(AtkResNode* nodeParam)
        {
            // list item renderer component
            AtkComponentBase* x = *(AtkComponentBase**)nodeParam;

            PluginLog.Debug($"component={(UInt64)nodeParam->GetComponent():X}, childCount={nodeParam->ChildCount}, target={*(UInt64*)nodeParam:X}, gotit={(UInt64)x:X}");

            if (x == null) return 0;
            AtkUldManager uldManager = x->UldManager;

            //nodeParam->index;


            var isMarketOpen = GetUnitBase("ItemSearch") != null;
            PluginLog.Debug("1");

            if (isMarketOpen) return 0;
            PluginLog.Debug("2");

            if (uldManager.NodeListCount < 14) return 0;
            PluginLog.Debug("3");

            var singlePriceNode = (AtkTextNode*)uldManager.NodeList[10];

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
                .Replace($"{(char)SeIconChar.Gil}", "")
                .Replace($",", "")
                .Replace(" ", "")
                .Replace($".", "");

            PluginLog.Debug($"priceString: '{priceString}', original: '{Common.ReadSeString(singlePriceNode->NodeText).TextValue}'");

            if (!int.TryParse(priceString, out var priceValue)) return 0;
            return priceValue;
        }

        public void Dispose()
        {
            this.UI.Dispose();
            this.Common.Dispose();
            this.Interface.CommandManager.RemoveHandler(commandName);
            this.Interface.CommandManager.RemoveHandler(commandNameConfig);
            this.Interface.Dispose();
        }

        private unsafe void OnCommand(string command, string args)
        {
            if (command == commandName)
                this.UI.Visible = true;
            else
                this.UI.SettingsVisible = true;
        }

        private unsafe AtkUnitBase* GetUnitBase(string name, int index = 1)
        {
            return (AtkUnitBase*)Interface.Framework.Gui.GetUiObjectByName(name, index);
        }

        private void DrawUI()
        {
            this.UI.Draw();
        }

        private void DrawConfigUI()
        {
            this.UI.SettingsVisible = true;
        }
    }
}
