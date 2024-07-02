using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Marketbuddy.Common
{
    public class Dalamud
    {
        public static void DalamudInitialize(IDalamudPluginInterface pluginInterface)
            => pluginInterface.Create<Dalamud>();

        // @formatter:off
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ICommandManager         CommandManager        { get; private set; } = null!;
        [PluginService] public static ISigScanner             SigScanner      { get; private set; } = null!;
        //[PluginService] public static DataManager            _DataManager        { get; private set; } = null!;
        //[PluginService] public static ClientState            _ClientState     { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui            { get; private set; } = null!;
        //[PluginService] public static SeStringManager        _SeStrings       { get; private set; } = null!;
        //[PluginService] public static ChatHandlers           _ChatHandlers    { get; private set; } = null!;
        //[PluginService] public static Framework              _Framework       { get; private set; } = null!;
        //[PluginService] public static GameNetwork            _Network         { get; private set; } = null!;
        //[PluginService] public static Condition              _Conditions      { get; private set; } = null!;
        [PluginService] public static IKeyState Keys        { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;
        [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
        //[PluginService] public static FlyTextGui             _FlyTexts        { get; private set; } = null!;
        //[PluginService] public static ToastGui               _Toasts          { get; private set; } = null!;
        //[PluginService] public static JobGauges              _Gauges          { get; private set; } = null!;
        //[PluginService] public static PartyFinderGui         _PartyFinder     { get; private set; } = null!;
        //[PluginService] public static BuddyList              _Buddies         { get; private set; } = null!;
        //[PluginService] public static PartyList              _Party           { get; private set; } = null!;
        //[PluginService] public static TargetManager          _Targets         { get; private set; } = null!;
        //[PluginService] public static ObjectTable            _Objects         { get; private set; } = null!;
        //[PluginService] public static FateTable              _Fates           { get; private set; } = null!;
        //[PluginService] public static LibcFunction           _LibC            { get; private set; } = null!;
        // @formatter:on
    }
}