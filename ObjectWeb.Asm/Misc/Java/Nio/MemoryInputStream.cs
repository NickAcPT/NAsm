using System.IO;
using ObjectWeb.Misc.Java.IO;

namespace ObjectWeb.Misc.Java.Nio
{
    public static partial class MemoryStreamExtensions
    {
        public static InputStream ToInputStream(this MemoryStream stream)
        {
            return new MemoryInputStream(stream);
        }
    }

    public class MemoryInputStream : InputStream
    {
        public MemoryInputStream(MemoryStream stream)
        {
            Stream = stream;
        }


        public MemoryStream Stream { get; }

        public static implicit operator MemoryInputStream(MemoryStream stream)
        {
            return new MemoryInputStream(stream);
        }

        public override int Read()
        {
            return Stream.ReadByte();
        }
    }
}