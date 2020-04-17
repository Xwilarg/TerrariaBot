using System.IO;

namespace TerrariaBot
{
    public static class Utils
    {
        public static void Write(this BinaryWriter w, Color color)
        {
            w.Write(color.r);
            w.Write(color.g);
            w.Write(color.b);
        }

        public static Color ReadColor(this BinaryReader r)
        {
            return new Color(r.ReadByte(), r.ReadByte(), r.ReadByte());
        }
    }
}
