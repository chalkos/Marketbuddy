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
        public static void DalamudInitialize(DalamudPluginInterface pluginInterface)
            => pluginInterface.Create<Dalamud>();

        // @formatter:off
        [PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static ICommandManager         CommandManager        { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static ISigScanner             SigScanner      { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static DataManager            _DataManager        { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static ClientState            _ClientState     { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IChatGui ChatGui            { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static SeStringManager        _SeStrings       { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static ChatHandlers           _ChatHandlers    { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static Framework              _Framework       { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static GameNetwork            _Network         { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static Condition              _Conditions      { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IKeyState Keys        { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IPluginLog Log { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IGameInteropProvider Hook { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static FlyTextGui             _FlyTexts        { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static ToastGui               _Toasts          { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static JobGauges              _Gauges          { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static PartyFinderGui         _PartyFinder     { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static BuddyList              _Buddies         { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static PartyList              _Party           { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static TargetManager          _Targets         { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static ObjectTable            _Objects         { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static FateTable              _Fates           { get; private set; } = null!;
        //[PluginService][RequiredVersion("1.0")] public static LibcFunction           _LibC            { get; private set; } = null!;
        // @formatter:on
    }
}