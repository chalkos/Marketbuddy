using System;
using ImGuiNET;

namespace Marketbuddy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private readonly Configuration configuration;
        private Plugin plugin;

        private bool settingsVisible;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible;

        // passing in the image here just for simplicity
        public PluginUI(Plugin plugin)
        {
            this.plugin = plugin;
            configuration = plugin.Configuration;
        }

        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible) return;

            if (ImGui.Begin("Marketbuddy", ref visible,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Nothing to show here at this time.");
                ImGui.End();
            }
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible) return;

            if (ImGui.Begin("Marketbuddy config", ref settingsVisible,
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
                var MaximumStackSize = configuration.MaximumStackSize;
                var UseMaxStackSize = configuration.UseMaxStackSize;

                bool changed =
                    ImGui.Checkbox("Open current prices list when adjusting a price",
                        ref AutoOpenComparePrices);
                changed |= ImGui.Checkbox("Holding SHIFT prevents the above", ref HoldShiftToStop);
                
                changed |= ImGui.Checkbox("Holding CTRL pastes a price from the clipboard and confirms it",
                    ref HoldCtrlToPaste);
                
                changed |= ImGui.Checkbox("Open price history together with current prices list",
                    ref AutoOpenHistory);
                
                changed |= ImGui.Checkbox("Clicking a price sets your price as that price with a 1gil undercut.",
                    ref AutoInputNewPrice);
                
                changed |=
                    ImGui.Checkbox("Clicking a price copies that price with a 1gil undercut to the clipboard.",
                        ref SaveToClipboard);
                
                changed |=
                    ImGui.Checkbox(
                        "Closes the list (if open) and confirms the new price after selecting it from the list (or if holding CTRL).",
                        ref AutoConfirmNewPrice);

                changed |=
                    ImGui.Checkbox("Limit stack size to ", ref UseMaxStackSize);
                
                ImGui.SameLine();
                ImGui.SetNextItemWidth(45);
                changed |= ImGui.InputInt("items", ref MaximumStackSize, 0);

                if (changed)
                {
                    configuration.AutoOpenComparePrices = AutoOpenComparePrices;
                    configuration.HoldShiftToStop = HoldShiftToStop;
                    configuration.HoldCtrlToPaste = HoldCtrlToPaste;
                    configuration.AutoOpenHistory = AutoOpenHistory;
                    configuration.AutoInputNewPrice = AutoInputNewPrice;
                    configuration.SaveToClipboard = SaveToClipboard;
                    configuration.AutoConfirmNewPrice = AutoConfirmNewPrice;
                    configuration.UseMaxStackSize = UseMaxStackSize;
                    configuration.MaximumStackSize = MaximumStackSize <= 9999 ? MaximumStackSize >= 1 ? MaximumStackSize : 1 : 9999;
                    configuration.Save();
                }
            }

            ImGui.End();
        }
    }
}