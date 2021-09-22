using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Marketbuddy.Common
{
    // based on https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Helper/Common.cs
    // and https://github.com/daemitus/ClickLib/blob/master/ClickLib/ClickBase.cs
    internal unsafe class Commons : IDisposable
    {
        public static List<IHookWrapper> HookList = new();

        private readonly InputSimulator InputSimulator;

        public Commons(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            InputSimulator = new InputSimulator();
        }

        public static DalamudPluginInterface PluginInterface { get; private set; }

        public static SigScanner Scanner => PluginInterface.TargetModuleScanner;

        public void Dispose()
        {
            foreach (var hook in HookList.Where(hook => !hook.IsDisposed))
            {
                if (hook.IsEnabled)
                    hook.Disable();
                hook.Dispose();
            }

            HookList.Clear();
        }

        public void pressEsc()
        {
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
        }

        public void WriteSeString(byte** startPtr, IntPtr alloc, SeString seString)
        {
            if (startPtr == null) return;
            var start = *startPtr;
            if (start == null) return;
            if (start == (byte*) alloc) return;
            WriteSeString((byte*) alloc, seString);
            *startPtr = (byte*) alloc;
        }

        public SeString ReadSeString(byte** startPtr)
        {
            if (startPtr == null) return null;
            var start = *startPtr;
            if (start == null) return null;
            return ReadSeString(start);
        }

        public SeString ReadSeString(byte* ptr)
        {
            var offset = 0;
            while (true)
            {
                var b = *(ptr + offset);
                if (b == 0) break;
                offset += 1;
            }

            var bytes = new byte[offset];
            Marshal.Copy(new IntPtr(ptr), bytes, 0, offset);
            return PluginInterface.SeStringManager.Parse(bytes);
        }

        public void WriteSeString(byte* dst, SeString s)
        {
            var bytes = s.Encode();
            for (var i = 0; i < bytes.Length; i++) *(dst + i) = bytes[i];
            *(dst + bytes.Length) = 0;
        }

        public SeString ReadSeString(Utf8String xivString)
        {
            var len = (int) (xivString.BufUsed > int.MaxValue ? int.MaxValue : xivString.BufUsed);
            var bytes = new byte[len];
            Marshal.Copy(new IntPtr(xivString.StringPtr), bytes, 0, len);
            return PluginInterface.SeStringManager.Parse(bytes);
        }

        public void WriteSeString(Utf8String xivString, SeString s)
        {
            var bytes = s.Encode();
            int i;
            xivString.BufUsed = 0;
            for (i = 0; i < bytes.Length && i < xivString.BufSize - 1; i++)
            {
                *(xivString.StringPtr + i) = bytes[i];
                xivString.BufUsed++;
            }

            *(xivString.StringPtr + i) = 0;
        }

        public static HookWrapper<T> Hook<T>(string signature, T detour, bool enable = true, int addressOffset = 0)
            where T : Delegate
        {
            var addr = Scanner.ScanText(signature);
            PluginLog.Information("hooking function at {add}", addr);
            var h = new Hook<T>(addr + addressOffset, detour);
            var wh = new HookWrapper<T>(h);
            if (enable) wh.Enable();
            HookList.Add(wh);
            return wh;
        }

        public static void OpenBrowser(string url)
        {
            Process.Start(new ProcessStartInfo {FileName = url, UseShellExecute = true});
        }

        protected ReceiveEventDelegate GetReceiveEventDelegate(AtkEventListener* eventListener)
        {
            var receiveEventAddress = new IntPtr(eventListener->vfunc[2]);
            return Marshal.GetDelegateForFunctionPointer<ReceiveEventDelegate>(receiveEventAddress);
        }

        internal void SendClick(IntPtr arg1, EventType arg2, uint arg3, void* target)
        {
            SendClick(arg1, arg2, arg3, target, IntPtr.Zero);
        }

        internal void SendClick(IntPtr arg1, EventType arg2, uint arg3, void* target, IntPtr arg5)
        {
            var receiveEvent = GetReceiveEventDelegate((AtkEventListener*) arg1);

            var arg4 = Marshal.AllocHGlobal(0x40);
            for (var i = 0; i < 0x40; i++)
                Marshal.WriteByte(arg4, i, 0);

            Marshal.WriteIntPtr(arg4, 0x8, new IntPtr(target));
            Marshal.WriteIntPtr(arg4, 0x10, arg1);

            if (arg5 == IntPtr.Zero)
            {
                arg5 = Marshal.AllocHGlobal(0x40);
                for (var i = 0; i < 0x40; i++)
                    Marshal.WriteByte(arg5, i, 0);
            }

            receiveEvent(arg1, arg2, arg3, arg4, arg5);

            Marshal.FreeHGlobal(arg4);
            Marshal.FreeHGlobal(arg5);
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

        protected delegate void ReceiveEventDelegate(IntPtr addon, EventType evt, uint a3, IntPtr a4, IntPtr a5);
    }
}