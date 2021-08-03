using System;
using ImGuiScene;

namespace UIDev
{
    internal interface IPluginUIMock : IDisposable
    {
        void Initialize(SimpleImGuiScene scene);
    }
}