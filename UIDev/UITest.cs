using System.Numerics;
using ImGuiNET;
using ImGuiScene;

namespace UIDev
{
    internal class UITest : IPluginUIMock
    {
        private SimpleImGuiScene scene;

        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here


            scene.OnBuildUI += Draw;

            Visible = true;

            // saving this only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            this.scene = scene;
        }

        public void Dispose()
        {
        }

        public static void Main(string[] args)
        {
            UIBootstrap.Inititalize(new UITest());
        }

        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
        // don't want to imply that is easy or the best way to go usually, so it's not done here either
        private void Draw()
        {
            DrawMainWindow();
            DrawSettingsWindow();

            if (!Visible) scene.ShouldQuit = true;
        }

        #region Nearly a copy/paste of PluginUI

        private bool visible;

        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        private bool settingsVisible;

        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        // this is where you'd have to start mocking objects if you really want to match
        // but for simple UI creation purposes, just hardcoding values works
        private bool fakeConfigBool = true;

        public void DrawMainWindow()
        {
            if (!Visible) return;

            if (ImGui.Begin("Marketbuddy", ref visible,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)) ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible) return;

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
            if (ImGui.Begin("A Wonderful Configuration Window", ref settingsVisible,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
                if (ImGui.Checkbox("Random Config Bool", ref fakeConfigBool))
                {
                    // nothing to do in a fake ui!
                }

            ImGui.End();
        }

        #endregion
    }
}