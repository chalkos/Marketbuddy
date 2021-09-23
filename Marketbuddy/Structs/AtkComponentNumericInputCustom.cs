using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Marketbuddy.Structs
{
    // Component::GUI::AtkComponentNumericInput
    //   Component::GUI::AtkComponentInputBase
    //     Component::GUI::AtkComponentBase
    //       Component::GUI::AtkEventListener

    [StructLayout(LayoutKind.Explicit, Size = 0x338)]
    public unsafe partial struct AtkComponentNumericInputCustom
    {
        [FieldOffset(0x0)] public AtkComponentInputBase AtkComponentInputBase;
        [FieldOffset(0xC8)] public AtkTextNode* AtkTextNode;
    }
}