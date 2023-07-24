using System;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using ImGuiNET;
using Marketbuddy.Common;
using Marketbuddy.Structs;
using static Marketbuddy.Common.Dalamud;

namespace Marketbuddy
{
    public unsafe class Marketbuddy : IDalamudPlugin
    {
        private const string commandName = "/mbuddy";

        internal PluginUI PluginUi { get; private set; }

        internal MarketGuiEventHandler MarketGuiEventHandler { get; private set; }

        // Assembly compatible with dev & published versions 
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;
        public string Name => "Marketbuddy";

        public Marketbuddy(DalamudPluginInterface pluginInterface)
        {
            try
            {
                DalamudInitialize(pluginInterface);
                Resolver.GetInstance.SetupSearchSpace(SigScanner.SearchBase);
                Resolver.GetInstance.Resolve();
                
                MarketGuiEventHandler = new MarketGuiEventHandler();

                PluginUi = new PluginUI(this);

                Common.Dalamud.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
                {
                    HelpMessage = "Show plugin configuration window."
                });

                PluginInterface.UiBuilder.Draw += DrawUi;
                PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
                IPCManager.Init();

            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                    PluginLog.Error(e, "Error loading plugin");
            }
        }

        public void Dispose()
        {
            IPCManager.Shutdown();
            PluginInterface.UiBuilder.Draw -= DrawUi;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;
            Common.Dalamud.CommandManager.RemoveHandler(commandName);
            PluginUi.Dispose();
            MarketGuiEventHandler.Dispose();
            Commons.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            if (command == commandName)
                PluginUi.SettingsVisible = !PluginUi.SettingsVisible;
        }

        private void DrawUi()
        {
            PluginUi.Draw();
        }

        private void DrawConfigUi()
        {
            PluginUi.SettingsVisible = true;
        }
    }
}