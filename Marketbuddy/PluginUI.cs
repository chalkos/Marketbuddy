using ImGuiNET;
using System;

namespace Marketbuddy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;
        private Plugin plugin;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Plugin plugin)
        {
            this.plugin = plugin;
            this.configuration = plugin.Configuration;
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
            if (!Visible)
            {
                return;
            }

            if (ImGui.Begin("Marketbuddy", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Nothing to show here at this time.");
                ImGui.End();
            }
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            if (ImGui.Begin("Marketbuddy config", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize))
            {


                var AutoOpenComparePrices = this.configuration.AutoOpenComparePrices;
                if (ImGui.Checkbox("Open current prices list when adjusting a price", ref AutoOpenComparePrices))
                {
                    this.configuration.AutoOpenComparePrices = AutoOpenComparePrices;
                    this.configuration.Save();
                }

                var HoldShiftToStop = this.configuration.HoldShiftToStop;
                if (ImGui.Checkbox("Holding SHIFT prevents the above", ref HoldShiftToStop))
                {
                    this.configuration.HoldShiftToStop = HoldShiftToStop;
                    this.configuration.Save();
                }

                var HoldCtrlToPaste = this.configuration.HoldCtrlToPaste;
                if (ImGui.Checkbox("Holding CTRL pastes a price from the clipboard and confirms it", ref HoldCtrlToPaste))
                {
                    this.configuration.HoldCtrlToPaste = HoldCtrlToPaste;
                    this.configuration.Save();
                }

                var AutoOpenHistory = this.configuration.AutoOpenHistory;
                if (ImGui.Checkbox("Open price history together with current prices list", ref AutoOpenHistory))
                {
                    this.configuration.AutoOpenHistory = AutoOpenHistory;
                    this.configuration.Save();
                }

                var AutoInputNewPrice = this.configuration.AutoInputNewPrice;
                if (ImGui.Checkbox("Clicking a price sets your price as that price with a 1gil undercut.", ref AutoInputNewPrice))
                {
                    this.configuration.AutoInputNewPrice = AutoInputNewPrice;
                    this.configuration.Save();
                }

                var SaveToClipboard = this.configuration.SaveToClipboard;
                if (ImGui.Checkbox("Clicking a price copies that price with a 1gil undercut to the clipboard.", ref SaveToClipboard))
                {
                    this.configuration.SaveToClipboard = SaveToClipboard;
                    this.configuration.Save();
                }
                
                var AutoConfirmNewPrice = this.configuration.AutoConfirmNewPrice;
                if (ImGui.Checkbox("Closes the list (if open) and confirms the new price after selecting it from the list (or if holding CTRL).", ref AutoConfirmNewPrice))
                {
                    this.configuration.AutoConfirmNewPrice = AutoConfirmNewPrice;
                    this.configuration.Save();
                }
            }
            ImGui.End();
        }
    }
}
