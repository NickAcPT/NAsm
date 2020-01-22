using System;

namespace ObjectWeb.Asm.Enums
{
    [Flags]
    public enum AccessFlags
    {
        None,
        Public = 0x0001,
        Private = 0x0002,
        Protected = 0x0004,
        Static = 0x0008,
        Final = 0x0010,
        Super = 0x0020,
        Synchronized = 0x0020,
        Open = 0x0020,
        Transitive = 0x0020,
        Volatile = 0x0040,
        Bridge = 0x0040,
        StaticPhase = 0x0040,
        Static_Phase = StaticPhase,
        Varargs = 0x0080,
        Transient = 0x0080,
        Native = 0x0100,
        Interface = 0x0200,
        Abstract = 0x0400,
        Strict = 0x0800,
        Synthetic = 0x1000,
        Annotation = 0x2000,
        Enum = 0x4000,
        Mandated = 0x8000,
        Module = 0x8000,
        Deprecated = 0x20000,
        Constructor = 0x40000
    }

    public static class AccessFlagsExtensions
    {
        public static bool HasFlagFast(this AccessFlags value, AccessFlags flag)
        {
            return (value & flag) != 0;
        }
        public static bool HasNotFlagFast(this AccessFlags value, AccessFlags flag)
        {
            return (value & flag) == 0;
        }
    }
}