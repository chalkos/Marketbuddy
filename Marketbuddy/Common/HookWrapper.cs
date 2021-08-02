using System;
using Dalamud.Hooking;
using Dalamud.Plugin;

namespace Marketbuddy.Common
{
    // based on https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Helper/HookWrapper.cs
    public interface IHookWrapper : IDisposable
    {
        public void Enable();
        public void Disable();

        public bool IsEnabled { get; }
        public bool IsDisposed { get; }

    }

    public class HookWrapper<T> : IHookWrapper where T : Delegate
    {

        private Hook<T> wrappedHook;

        private bool disposed;

        public HookWrapper(Hook<T> hook)
        {
            wrappedHook = hook;
        }

        public void Enable()
        {
            if (disposed) return;
            wrappedHook?.Enable();
        }

        public void Disable()
        {
            if (disposed) return;
            wrappedHook?.Disable();
        }

        public void Dispose()
        {
            PluginLog.Information("Disposing of {cdelegate}", typeof(T).Name);
            Disable();
            disposed = true;
            wrappedHook?.Dispose();
        }

        public T Original => wrappedHook.Original;
        public bool IsEnabled => wrappedHook.IsEnabled;
        public bool IsDisposed => wrappedHook.IsDisposed;
    }
}