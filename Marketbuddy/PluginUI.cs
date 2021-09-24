using System;
using System.Numerics;
using ImGuiNET;

namespace Marketbuddy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private Configuration configuration => Configuration.GetOrLoad();

        private Marketbuddy marketbuddy;

        private bool _settingsVisible;

        // passing in the image here just for simplicity
        public PluginUI(Marketbuddy plugin)
        {
            marketbuddy = plugin;
            SettingsVisible = false;
        }

        public bool SettingsVisible
        {
            get => _settingsVisible;
            set => _settingsVisible = value;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            DrawSettingsWindow();
            DrawOverlayWindow();
        }

        private void DrawOverlayWindow()
        {
            if (!configuration.AdjustMaxStackSizeInSellList ||
                !marketbuddy.MarketGuiEventHandler.AddonRetainerSellList_Position(out Vector2 position)) return;
            
            var windowVisible = true;
            ImGui.SetNextWindowPos(position);

            var hSpace = new Vector2(1, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, hSpace);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, hSpace);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, hSpace);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.One);
            if (ImGui.Begin("Marketbuddy_stacklimit", ref windowVisible,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground))
            {
                var MaximumStackSize = configuration.MaximumStackSize;
                var UseMaxStackSize = configuration.UseMaxStackSize;

                bool changed =
                    ImGui.Checkbox("Limit stack size to ", ref UseMaxStackSize);

                ImGui.SameLine();
                ImGui.SetNextItemWidth(30);

                changed |= ImGui.InputInt("items", ref MaximumStackSize, 0);

                ImGui.SameLine();
                
                if (changed)
                {
                    configuration.UseMaxStackSize = UseMaxStackSize;
                    configuration.MaximumStackSize = MaximumStackSize <= 9999
                        ? MaximumStackSize >= 1 ? MaximumStackSize : 1
                        : 9999;
                    configuration.Save();
                }
            }

            ImGui.PopStyleVar(5);
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible) return;

            if (ImGui.Begin("Marketbuddy config", ref _settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                var AutoOpenComparePrices = configuration.AutoOpenComparePrices;
                var HoldShiftToStop = configuration.HoldShiftToStop;
                var HoldCtrlToPaste = configuration.HoldCtrlToPaste;
                var AutoOpenHistory = configuration.AutoOpenHistory;
                var AutoInputNewPrice = configuration.AutoInputNewPrice;
                var SaveToClipboard = configuration.SaveToClipboard;
                var AutoConfirmNewPrice = configuration.AutoConfirmNewPrice;
                var AdjustMaxStackSizeInSellList = configuration.AdjustMaxStackSizeInSellList;
                var AdjustMaxStackSizeInSellListOffset = configuration.AdjustMaxStackSizeInSellListOffset;
                var MaximumStackSize = configuration.MaximumStackSize;
                var UseMaxStackSize = configuration.UseMaxStackSize;

                ImGui.Checkbox("Open current prices list when adjusting a price",
                    ref AutoOpenComparePrices);
                ImGui.Checkbox("Holding SHIFT prevents the above", ref HoldShiftToStop);

                ImGui.Checkbox("Holding CTRL pastes a price from the clipboard and confirms it", ref HoldCtrlToPaste);

                ImGui.Checkbox("Open price history together with current prices list", ref AutoOpenHistory);

                ImGui.Checkbox("Clicking a price sets your price as that price with a 1gil undercut.",
                    ref AutoInputNewPrice);

                ImGui.Checkbox("Clicking a price copies that price with a 1gil undercut to the clipboard.",
                    ref SaveToClipboard);

                ImGui.Checkbox(
                    "Closes the list (if open) and confirms the new price after selecting it from the list (or if holding CTRL).",
                    ref AutoConfirmNewPrice);

                ImGui.Checkbox("Adjust maximum stack size in retainer sell list addon ",
                    ref AdjustMaxStackSizeInSellList);

                if (AdjustMaxStackSizeInSellList)
                    ImGui.DragFloat2("Offset window", ref AdjustMaxStackSizeInSellListOffset, 1f, 1, float.MaxValue,
                        "%.0f");

                ImGui.Checkbox("Limit stack size to ", ref UseMaxStackSize);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(45);
                ImGui.InputInt("items", ref MaximumStackSize, 0);


                bool changed =
                    AutoOpenComparePrices != configuration.AutoOpenComparePrices
                    || HoldShiftToStop != configuration.HoldShiftToStop
                    || HoldCtrlToPaste != configuration.HoldCtrlToPaste
                    || AutoOpenHistory != configuration.AutoOpenHistory
                    || AutoInputNewPrice != configuration.AutoInputNewPrice
                    || SaveToClipboard != configuration.SaveToClipboard
                    || AutoConfirmNewPrice != configuration.AutoConfirmNewPrice
                    || AdjustMaxStackSizeInSellList != configuration.AdjustMaxStackSizeInSellList
                    || AdjustMaxStackSizeInSellListOffset != configuration.AdjustMaxStackSizeInSellListOffset
                    || MaximumStackSize != configuration.MaximumStackSize
                    || UseMaxStackSize != configuration.UseMaxStackSize;

                if (changed)
                {
                    configuration.AutoOpenComparePrices = AutoOpenComparePrices;
                    configuration.HoldShiftToStop = HoldShiftToStop;
                    configuration.HoldCtrlToPaste = HoldCtrlToPaste;
                    configuration.AutoOpenHistory = AutoOpenHistory;
                    configuration.AutoInputNewPrice = AutoInputNewPrice;
                    configuration.SaveToClipboard = SaveToClipboard;
                    configuration.AutoConfirmNewPrice = AutoConfirmNewPrice;
                    configuration.AdjustMaxStackSizeInSellList = AdjustMaxStackSizeInSellList;
                    configuration.AdjustMaxStackSizeInSellListOffset = AdjustMaxStackSizeInSellListOffset;
                    configuration.UseMaxStackSize = UseMaxStackSize;
                    configuration.MaximumStackSize =
                        MaximumStackSize <= 9999 ? MaximumStackSize >= 1 ? MaximumStackSize : 1 : 9999;
                    configuration.Save();
                }
            }

            ImGui.End();
        }
    }
}