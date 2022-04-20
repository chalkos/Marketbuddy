using System;
using System.Numerics;
using ImGuiNET;

namespace Marketbuddy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private Configuration conf => Configuration.GetOrLoad();

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
            if (!conf.AdjustMaxStackSizeInSellList ||
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
                if (ImGui.Checkbox("Limit stack size to ", ref conf.UseMaxStackSize))
                    conf.Save();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(30);

                if (ImGui.InputInt("items", ref conf.MaximumStackSize, 0))
                    MaximumStackSizeChanged();
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
                if (ImGui.Checkbox("Open current prices list when adjusting a price",
                        ref conf.AutoOpenComparePrices))
                    conf.Save();
                if (ImGui.Checkbox("Holding SHIFT prevents the above", ref conf.HoldShiftToStop)) conf.Save();

                if (ImGui.Checkbox("Holding CTRL pastes a price from the clipboard and confirms it",
                        ref conf.HoldCtrlToPaste))
                    conf.Save();

                if (ImGui.Checkbox("Open price history together with current prices list", ref conf.AutoOpenHistory))
                    conf.Save();

                if (ImGui.Checkbox("Clicking a price sets your price as that price with a",
                        ref conf.AutoInputNewPrice))
                    conf.Save();
                ImGui.SameLine();
                ImGui.SetNextItemWidth(45);
                ImGui.InputInt("gil undercut.", ref conf.UndercutPrice, 0);

                if (ImGui.Checkbox(
                        $"Clicking a price copies that price with a {conf.UndercutPrice}gil undercut to the clipboard.",
                        ref conf.SaveToClipboard))
                    conf.Save();

                if (ImGui.Checkbox(
                        "Closes the list (if open) and confirms the new price after selecting it from the list (or if holding CTRL).",
                        ref conf.AutoConfirmNewPrice))
                    conf.Save();

                if (ImGui.Checkbox("Adjust maximum stack size in retainer sell list addon ",
                        ref conf.AdjustMaxStackSizeInSellList))
                    conf.Save();

                if (conf.AdjustMaxStackSizeInSellList)
                    if (ImGui.DragFloat2("Offset window", ref conf.AdjustMaxStackSizeInSellListOffset, 1f, 1,
                            float.MaxValue, "%.0f"))
                        conf.Save();

                if (ImGui.Checkbox("Limit stack size to ", ref conf.UseMaxStackSize))
                    conf.Save();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(45);
                if (ImGui.InputInt("items", ref conf.MaximumStackSize, 0))
                    MaximumStackSizeChanged();
            }

            ImGui.End();
        }

        private void MaximumStackSizeChanged()
        {
            conf.MaximumStackSize = conf.MaximumStackSize <= 999
                ? conf.MaximumStackSize >= 1 ? conf.MaximumStackSize : 1
                : 999;
            conf.Save();
        }
    }
}