using System;

namespace ObjectWeb.Asm.Enums
{
    [Flags]
    public enum ParsingOptions
    {
        SkipCode = ClassReader.Skip_Code,
        ExpandFrames = ClassReader.Expand_Frames,
        SkipDebug = ClassReader.Skip_Debug,
        SkipFrames = ClassReader.Skip_Frames,
        ExpandAsmInsns = ClassReader.Expand_Asm_Insns
    }

    public static class ParsingOptionsExtensions
    {
        public static bool HasFlagFast(this ParsingOptions value, ParsingOptions flag)
        {
            return (value & flag) != 0;
        }
        public static bool HasNotFlagFast(this ParsingOptions value, ParsingOptions flag)
        {
            return (value & flag) == 0;
        }
    }
}