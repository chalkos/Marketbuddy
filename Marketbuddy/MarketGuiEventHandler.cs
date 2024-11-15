using System;
using System.Numerics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Marketbuddy.Common;
using Marketbuddy.Structs;
using static Marketbuddy.Common.Dalamud;

namespace Marketbuddy
{
    public unsafe class MarketGuiEventHandler : IDisposable
    {
        internal Configuration conf => Configuration.GetOrLoad();

        private IntPtr AddonRetainerSellList = IntPtr.Zero;

        public MarketGuiEventHandler()
        {
            AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "ItemSearchResult", OnItemSearchResultReceiveEvent);

            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerSell", OnRetainerSellSetup);
            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ItemSearchResult", OnItemSearchResultSetup);
            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "RetainerSellList", OnRetainerSellListSetup);
            AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "RetainerSellList", OnRetainerSellListFinalize);

        }

        private void OnRetainerSellListFinalize(AddonEvent type, AddonArgs args)
        {
            var addon = args.Addon;
            if (addon == AddonRetainerSellList)
                DebugMessage($"AddonRetainerSellList.OnFinalize (known: {addon:X})");
            else
                DebugMessage(
                    $"AddonRetainerSellList.OnFinalize (unk. have {AddonRetainerSellList:X} got {addon:X})");
            AddonRetainerSellList = IntPtr.Zero;
        }

        private void OnRetainerSellListSetup(AddonEvent type, AddonArgs args)
        {
            var addon = args.Addon;
            DebugMessage($"AddonRetainerSellList.OnSetup (got: {addon:X})");
            AddonRetainerSellList = addon;
        }

        private void OnItemSearchResultSetup(AddonEvent type, AddonArgs args)
        {
            DebugMessage("AddonItemSearchResult.OnSetup");
            var addon = args.Addon;

            if (!IPCManager.IsLocked)
            {
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
                        Log.Error(ex, "Houston, we have a problem");
                    }
            }
        }

        private void OnRetainerSellSetup(AddonEvent type, AddonArgs args)
        {
            DebugMessage("AddonRetainerSell.OnSetup");
            var addon = args.Addon;

            if (!IPCManager.IsLocked)
            {
                if (conf.HoldCtrlToPaste && Keys[VirtualKey.CONTROL])
                {
                    var cbValue = ImGuiEx.GetClipboardText();
                    if (int.TryParse(cbValue, out var priceValue) && priceValue > 0)
                        SetPrice(priceValue);
                    else
                        ChatGui.PrintError("[Marketbuddy] Clipboard does not contain a valid price");
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
                        Log.Error(ex, "Houston, we have a problem");
                    }
                }
            }
        }

        private void OnItemSearchResultReceiveEvent(AddonEvent type, AddonArgs args)
        {
            var eventArgs = (AddonReceiveEventArgs)args;
            var eventType = eventArgs.AtkEventType;
            var nodeParam = eventArgs.Data;
            if (!IPCManager.IsLocked)
            {
                if (conf.AutoInputNewPrice || conf.SaveToClipboard)
                    if (eventType == 35 && nodeParam != IntPtr.Zero) // && (*eventInfoStruct) != null ) // click
                        try
                        {
                            //AtkUldManager uldManager = (*eventInfoStruct)->UldManager;
#pragma warning disable IDE0004
                            //casts are necessary
                            var price = conf.UndercutUsePercent ? (int)((float)getPricePerItem(nodeParam) * (1f - (float)conf.UndercutPercent / 100f)) : getPricePerItem(nodeParam) - conf.UndercutPrice;
#pragma warning restore IDE0004
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
                            Log.Error(e, "Error getting price per item or setting the new price");
                        }
            }
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

        public void Dispose()
        {
            AddonLifecycle.UnregisterListener(AddonEvent.PostReceiveEvent, "ItemSearchResult", OnItemSearchResultReceiveEvent);

            AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerSell", OnRetainerSellSetup);
            AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "ItemSearchResult", OnItemSearchResultSetup);
            AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "RetainerSellList", OnRetainerSellListSetup);
            AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, "RetainerSellList", OnRetainerSellListFinalize);
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
                        .NodeList[7]->GetComponent()->OwnerNode);

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
            Log.Debug(msg);
#endif
        }
    }
}
