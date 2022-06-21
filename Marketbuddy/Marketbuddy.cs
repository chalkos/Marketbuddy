using System;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
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

        private bool isDisposed = false;

        public Marketbuddy(DalamudPluginInterface pluginInterface)
        {
            Task.Run(Resolver.Initialize)
                .ContinueWith((_) =>
                {
                    if (isDisposed) return;

                    DalamudInitialize(pluginInterface);

                    MarketGuiEventHandler = new MarketGuiEventHandler();

                    PluginUi = new PluginUI(this);

                    Common.Dalamud.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
                    {
                        HelpMessage = "Show plugin configuration window."
                    });

                    PluginInterface.UiBuilder.Draw += DrawUi;
                    PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
                }).ContinueWith(t =>
                {
                    var aggException = t.Exception!.Flatten();
                    foreach (var e in aggException.InnerExceptions)
                    {
                        if (e is OperationCanceledException)
                            continue;
                        PluginLog.Error(e, "Error loading plugin");
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Dispose()
        {
            isDisposed = true;
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