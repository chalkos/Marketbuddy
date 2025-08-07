using System;
using Dalamud.Bindings.ImGui;

namespace Marketbuddy.Common;

public static class ImGuiEx
{
    internal static string GetClipboardText()
    {
        try
        {
            return ImGui.GetClipboardText() ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}