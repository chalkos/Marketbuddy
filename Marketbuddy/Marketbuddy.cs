using System;
using System.Reflection;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Marketbuddy.Common;

namespace Marketbuddy
{
    public unsafe class Marketbuddy : IDalamudPlugin
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ICommandManager         CommandManager        { get; private set; } = null!;
        [PluginService] internal static ISigScanner             SigScanner      { get; private set; } = null!;
        [PluginService] internal static IChatGui ChatGui            { get; private set; } = null!;
        [PluginService] internal static IKeyState Keys        { get; private set; } = null!;
        [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
        [PluginService] internal static IPluginLog Log { get; private set; } = null!;
        [PluginService] internal static IGameInteropProvider Hook { get; private set; } = null!;
        
        private const string commandName = "/mbuddy";
        
        internal PluginUI PluginUi { get; private set; }

        internal MarketGuiEventHandler MarketGuiEventHandler { get; private set; }

        // Assembly compatible with dev & published versions 
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;
        public string Name => "Marketbuddy";

        public Marketbuddy()
        {
            try
            {
                
                
                
                // DalamudInitialize(pluginInterface);
                // Resolver.GetInstance.SetupSearchSpace(SigScanner.SearchBase);
                // Resolver.GetInstance.Resolve();
                
                MarketGuiEventHandler = new MarketGuiEventHandler();

                PluginUi = new PluginUI(this);
                
                CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
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
                    Log.Error(e, "Error loading plugin");
            }
        }

        public void Dispose()
        {
            IPCManager.Shutdown();
            PluginInterface.UiBuilder.Draw -= DrawUi;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;
            CommandManager.RemoveHandler(commandName);
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