using System;
using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Marketbuddy.Common;
using Marketbuddy.Structs;
using static Marketbuddy.Common.Dalamud;

namespace Marketbuddy
{
    public class MarketGuiEventHandler : IDisposable
    {
        #region Sigs, Hooks & Delegates declaration

        private readonly string AddonItemSearchResult_ReceiveEvent_Signature =
            "4C 8B DC 53 56 48 81 EC ?? ?? ?? ?? 49 89 6B 08";

        private readonly string AddonRetainerSell_OnSetup_Signature =
            "48 89 5C 24 ?? 55 56 57 48 83 EC 50 4C 89 64 24";

        private readonly string AddonItemSearchResult_OnSetup_Signature =
            "40 53 41 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 89 AC 24";

        private readonly string AddonRetainerSellList_OnSetup_Signature =
            "40 53 55 56 57 41 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B F9 49 8B F0 49 8D 48 10";

        private readonly string AddonRetainerSellList_OnFinalize_Signature =
            "40 53 48 83 EC 20 80 B9 ?? ?? ?? ?? ?? 48 8B D9 74 0E 45 33 C9";

        private HookWrapper<Addon_ReceiveEvent_Delegate> AddonItemSearchResult_ReceiveEvent_HW;
        private HookWrapper<Addon_OnSetup_Delegate> AddonRetainerSell_OnSetup_HW;
        private HookWrapper<Addon_OnSetup_Delegate> AddonItemSearchResult_OnSetup_HW;
        private HookWrapper<Addon_OnSetup_Delegate> AddonRetainerSellList_OnSetup_HW;
        private HookWrapper<Addon_OnFinalize_Delegate> AddonRetainerSellList_OnFinalize_HW;

        // __int64 __fastcall Client::UI::AddonXXX_ReceiveEvent(__int64 a1, __int16 a2, int a3, __int64 a4, __int64* a5)
        private delegate IntPtr Addon_ReceiveEvent_Delegate(IntPtr self, ushort eventType,
            uint eventParam, IntPtr eventStruct, IntPtr /* AtkResNode* */ nodeParam);

        // __int64 __fastcall Client::UI::AddonXXX_OnSetup(__int64 a1, unsigned int a2, __int64 a3)
        private delegate IntPtr Addon_OnSetup_Delegate(IntPtr addon, uint a2, IntPtr dataPtr);

        // __int64 __fastcall Client::UI::AddonXXX_Finalize(__int64 a1)
        private delegate IntPtr Addon_OnFinalize_Delegate(IntPtr addon);

        #endregion

        internal Configuration conf => Configuration.GetOrLoad();

        private IntPtr AddonRetainerSellList = IntPtr.Zero;

        public MarketGuiEventHandler()
        {
            AddonItemSearchResult_ReceiveEvent_HW =
                Commons.Hook<Addon_ReceiveEvent_Delegate>(
                    AddonItemSearchResult_ReceiveEvent_Signature,
                    AddonItemSearchResult_ReceiveEvent_Delegate_Detour);

            AddonRetainerSell_OnSetup_HW = Commons.Hook<Addon_OnSetup_Delegate>(
                AddonRetainerSell_OnSetup_Signature,
                AddonRetainerSell_OnSetup_Delegate_Detour);

            AddonItemSearchResult_OnSetup_HW = Commons.Hook<Addon_OnSetup_Delegate>(
                AddonItemSearchResult_OnSetup_Signature,
                AddonItemSearchResult_OnSetup_Delegate_Detour);

            AddonRetainerSellList_OnSetup_HW = Commons.Hook<Addon_OnSetup_Delegate>(
                AddonRetainerSellList_OnSetup_Signature,
                AddonRetainerSellList_OnSetup_Delegate_Detour);

            AddonRetainerSellList_OnFinalize_HW = Commons.Hook<Addon_OnFinalize_Delegate>(
                AddonRetainerSellList_OnFinalize_Signature,
                AddonRetainerSellList_OnFinalize_Delegate_Detour);
        }

        internal unsafe bool AddonRetainerSellList_Position(out Vector2 position)
        {
            position = Vector2.One;
            if (AddonRetainerSellList == IntPtr.Zero)
                return false;

            position = new Vector2(
                ((AtkUnitBase*)AddonRetainerSellList)->X + conf.AdjustMaxStackSizeInSellListOffset.X,
                ((AtkUnitBase*)AddonRetainerSellList)->Y + conf.AdjustMaxStackSizeInSellListOffset.Y
            );
            return true;
        }

        private IntPtr AddonRetainerSellList_OnFinalize_Delegate_Detour(IntPtr addon)
        {
            if (addon == AddonRetainerSellList)
                DebugMessage($"AddonRetainerSellList.OnFinalize (known: {addon:X})");
            else
                DebugMessage(
                    $"AddonRetainerSellList.OnFinalize (unk. have {AddonRetainerSellList:X} got {addon:X})");
            AddonRetainerSellList = IntPtr.Zero;
            return AddonRetainerSellList_OnFinalize_HW.Original(addon);
        }

        private IntPtr AddonRetainerSellList_OnSetup_Delegate_Detour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            DebugMessage($"AddonRetainerSellList.OnSetup (got: {addon:X})");
            var result = AddonRetainerSellList_OnSetup_HW.Original(addon, a2, dataPtr);
            AddonRetainerSellList = addon;
            return result;
        }

        public void Dispose()
        {
            AddonItemSearchResult_ReceiveEvent_HW.Dispose();
            AddonRetainerSell_OnSetup_HW.Dispose();
            AddonItemSearchResult_OnSetup_HW.Dispose();
        }

        private unsafe IntPtr AddonRetainerSell_OnSetup_Delegate_Detour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            DebugMessage("AddonRetainerSell.OnSetup");
            var result = AddonRetainerSell_OnSetup_HW.Original(addon, a2, dataPtr);

            if (conf.HoldCtrlToPaste && Keys[VirtualKey.CONTROL])
            {
                var cbValue = ImGui.GetClipboardText() ?? "";
                if (int.TryParse(cbValue, out var priceValue) && priceValue > 0)
                    SetPrice(priceValue);
                else
                    ChatGui.Print("Marketbuddy: Clipboard does not contain a valid price");
            }
            else if (conf.AutoOpenComparePrices && !conf.HoldShiftToStop ||
                     conf.AutoOpenComparePrices && conf.HoldShiftToStop && !Keys[VirtualKey.SHIFT] ||
                     !conf.AutoOpenComparePrices && conf.HoldShiftToStop && Keys[VirtualKey.SHIFT])
            {
                try
                {
                    //open compare prices list on opening sell price selection
                    var comparePrices = ((AddonRetainerSell*)addon)->ComparePrices->AtkComponentBase.OwnerNode;
                    // Client::UI::AddonRetainerSell.ReceiveEvent this=0x214C05CB480 evt=EventType.CHANGE               a3=4   a4=0x2146C18C210 (src=0x214C05CB480; tgt=0x214606863B0) a5=0xBB316FE6C8
                    Commons.SendClick(addon, EventType.CHANGE, 4, comparePrices);
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Houston, we have a problem");
                }
            }


            return result;
        }

        private unsafe IntPtr AddonItemSearchResult_OnSetup_Delegate_Detour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            DebugMessage("AddonItemSearchResult.OnSetup");
            var result = AddonItemSearchResult_OnSetup_HW.Original(addon, a2, dataPtr);

            bool shouldOpenHistory = conf.AutoOpenHistory && !conf.HoldAltHistoryHandling
                                     || conf.AutoOpenHistory && conf.HoldAltHistoryHandling && !Keys[VirtualKey.MENU]
                                     || !conf.AutoOpenHistory && conf.HoldAltHistoryHandling && Keys[VirtualKey.MENU];

            if (shouldOpenHistory)
                try
                {
                    //open history on opening the list
                    var history = ((AddonItemSearchResult*)addon)->History->AtkComponentBase.OwnerNode;
                    //Client::UI::AddonItemSearchResult.ReceiveEvent this=0x1CC2BF42BD0 evt=EventType.CHANGE               a3=23  a4=0x1CCD86C1460 a5=0x90EF96E598
                    Commons.SendClick(addon, EventType.CHANGE, 23, history);
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Houston, we have a problem");
                }

            return result;
        }

        private IntPtr AddonItemSearchResult_ReceiveEvent_Delegate_Detour(IntPtr self, ushort eventType,
            uint eventParam, IntPtr eventStruct, IntPtr /* AtkResNode* */ nodeParam)
        {
            var result =
                AddonItemSearchResult_ReceiveEvent_HW.Original(self, eventType, eventParam, eventStruct, nodeParam);

            if (conf.AutoInputNewPrice || conf.SaveToClipboard)
                if (eventType == 35 && nodeParam != IntPtr.Zero) // && (*eventInfoStruct) != null ) // click
                    try
                    {
                        //AtkUldManager uldManager = (*eventInfoStruct)->UldManager;
                        var price = getPricePerItem(nodeParam);
                        switch (conf.UndercutPrice)
                        {
                            case <= 0:
                                break;
                            case < 1:
                                price = Convert.ToInt32(price * (1 - conf.UndercutPrice));
                                break;
                            default:
                                price = Convert.ToInt32(price - conf.UndercutPrice);
                                break;
                        }
                        price = price - (price % conf.ModuloPrice);


                        price =
                            price < Configuration.MIN_PRICE ? Configuration.MIN_PRICE
                            : price > Configuration.MAX_PRICE ? Configuration.MAX_PRICE
                            : price;

                        SetPrice(price);
                    }
                    catch (Exception e)
                    {
                        ChatGui.PrintError(
                            "[Marketbuddy] Error getting price per item or setting the new price. Use /xllog to see the error and submit it in a github issue");
                        PluginLog.Error(e, "Error getting price per item or setting the new price");
                    }

            return result;
        }

        private unsafe void SetPrice(int newPrice)
        {
            var retainerSell = Commons.GetUnitBase("RetainerSell");
            if (retainerSell == null) return;
            
            if (retainerSell->UldManager.NodeListCount != 23)
                throw new MarketException("Unexpected fields in addon RetainerSell");

            var priceComponentNumericInput =
                (AtkComponentNumericInput*)retainerSell->UldManager.NodeList[15]->GetComponent();
            var quantityComponentNumericInput =
                (AtkComponentNumericInput*)retainerSell->UldManager.NodeList[11]->GetComponent();
            DebugMessage($"componentNumericInput: {new IntPtr(priceComponentNumericInput).ToString("X")}");
            DebugMessage($"componentNumericInput: {new IntPtr(quantityComponentNumericInput).ToString("X")}");

            if (conf.AutoInputNewPrice)
            {
                priceComponentNumericInput->SetValue(newPrice);

                if (conf.UseMaxStackSize)
                {
                    var quantityValueString = Commons.Utf8StringToString(
                        ((AtkComponentNumericInputCustom*)quantityComponentNumericInput)->AtkTextNode->NodeText);
                    DebugMessage($"qty: {quantityValueString}");
                    if (int.TryParse(quantityValueString, out var quantityValue))
                    {
                        if (quantityValue > conf.MaximumStackSize)
                            quantityComponentNumericInput->SetValue(conf.MaximumStackSize);
                    }
                }
            }

            if (conf.SaveToClipboard)
                ImGui.SetClipboardText(newPrice.ToString());

            DebugMessage($"Asking price of {newPrice} gil set and copied to clipboard.");

            if (!conf.AutoConfirmNewPrice) return;

            // close ItemSearchResult
            // Component::GUI::AtkComponentWindow.ReceiveEvent this=0x1AC801863B0 evt=EventType.CHANGE               a3=2   a4=0x1AC66640090 (src=0x1AC801863B0; tgt=0x1AC98B47EA0) a5=0x4AAAEFE388
            var addonItemSearchResult = Commons.GetUnitBase("ItemSearchResult");
            if (addonItemSearchResult != null)
                Commons.SendClick(new IntPtr(addonItemSearchResult->WindowNode->Component), EventType.CHANGE, 2,
                    addonItemSearchResult->WindowNode->Component->UldManager
                        .NodeList[6]->GetComponent()->OwnerNode);

            // click confirm on RetainerSell
            // Client::UI::AddonRetainerSell.ReceiveEvent this=0x214B4D360E0 evt=EventType.CHANGE               a3=21  a4=0x214B920D2E0 (src=0x214B4D360E0; tgt=0x21460686550) a5=0xBB316FE6C8
            var addonRetainerSell = (AddonRetainerSell*)retainerSell;
            Commons.SendClick(new IntPtr(addonRetainerSell), EventType.CHANGE, 21, addonRetainerSell->Confirm);
        }

        private unsafe int getPricePerItem(IntPtr /* AtkResNode* */ nodeParam)
        {
            var listAtkResNode = (AtkResNode*)nodeParam;

            // list item renderer component
            var listAtkComponentBase = *(AtkComponentBase**)nodeParam;

            DebugMessage(
                $"component={(ulong)listAtkResNode->GetComponent():X}, childCount={listAtkResNode->ChildCount}, target={*(ulong*)nodeParam:X}, gotit={(ulong)listAtkComponentBase:X}");

            if (listAtkComponentBase == null) return 0;
            var uldManager = listAtkComponentBase->UldManager;

            var isMarketOpen = Commons.GetUnitBase("ItemSearch") != null;
            DebugMessage("1");

            if (isMarketOpen) return 0;
            DebugMessage("2");

            if (uldManager.NodeListCount < 14) return 0;
            DebugMessage("3");

            var singlePriceNode = (AtkTextNode*)uldManager.NodeList[10];

            if (singlePriceNode == null)
            {
                DebugMessage($"singlePriceNode == null {singlePriceNode == null}");
                return 0;
            }

            var priceString = Commons.Utf8StringToString(singlePriceNode->NodeText)
                .Replace($"{(char)SeIconChar.Gil}", "")
                .Replace(",", "")
                .Replace(" ", "")
                .Replace(".", "");

            DebugMessage(
                $"priceString: '{priceString}', original: '{Commons.Utf8StringToString(singlePriceNode->NodeText)}'");

            if (!int.TryParse(priceString, out var priceValue)) return 0;
            return priceValue;
        }

        private void DebugMessage(string msg)
        {
#if DEBUG
            PluginLog.Debug(msg);
#endif
        }
    }
}