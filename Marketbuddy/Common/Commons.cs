using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Marketbuddy.Common
{
    // based on https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Helper/Common.cs
    // and https://github.com/daemitus/ClickLib/blob/master/ClickLib/ClickBase.cs
    internal static class Commons
    {
        // private
        private static List<IHookWrapper> HookList = new();

        public static void Dispose()
        {
            try
            {
                DisposeHooks();
            }
            catch (Exception e)
            {
                Marketbuddy.Log.Error(e, "Error unloading plugin");
            }
        }

        public static HookWrapper<T> Hook<T>(string signature, T detour, bool enable = true, int addressOffset = 0)
            where T : Delegate
        {
            var addr = Marketbuddy.SigScanner.ScanText(signature);
            Marketbuddy.Log.Information($"hooking function at {addr:X}");
            var h = Marketbuddy.Hook.HookFromAddress<T>(addr + addressOffset, detour);
            var wh = new HookWrapper<T>(h);
            if (enable) wh.Enable();
            HookList.Add(wh);
            return wh;
        }

        public static void DisposeHooks()
        {
            foreach (var hook in HookList.Where(hook => !hook.IsDisposed))
            {
                if (hook.IsEnabled)
                    hook.Disable();
                hook.Dispose();
            }

            HookList.Clear();
        }

        public static Task<T> StartSTATask<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
        
        public static unsafe string Utf8StringToString(Utf8String str) {
            if (str.StringPtr == null || str.BufUsed <= 1)
                return string.Empty;
            return Encoding.UTF8.GetString(str.StringPtr, (int)str.BufUsed - 1);
        }

        public static unsafe AtkUnitBase* GetUnitBase(string name, int index = 1)
        {
            return (AtkUnitBase*)Marketbuddy.GameGui.GetAddonByName(name, index).ToPointer();
        }

        internal static unsafe void SendClick(AtkEventListener* arg1, AtkEventType arg2, int arg3, void* target)
        {
            SendClick(arg1, arg2, arg3, target, IntPtr.Zero);
        }

        internal static unsafe void SendClick(AtkEventListener* arg1, AtkEventType arg2, int arg3, void* target, IntPtr arg5)
        {
            var arg4 = Marshal.AllocHGlobal(0x40);
            for (var i = 0; i < 0x40; i++)
                Marshal.WriteByte(arg4, i, 0);

            Marshal.WriteIntPtr(arg4, 0x8, new IntPtr(target));
            Marshal.WriteIntPtr(arg4, 0x10, new IntPtr(arg1));

            if (arg5 == IntPtr.Zero)
            {
                arg5 = Marshal.AllocHGlobal(0x40);
                for (var i = 0; i < 0x40; i++)
                    Marshal.WriteByte(arg5, i, 0);
            }

            arg1->ReceiveEvent(arg2, arg3, (AtkEvent*)arg4, (AtkEventData*)arg5);

            Marshal.FreeHGlobal(arg4);
            Marshal.FreeHGlobal(arg5);
        }
    }
}