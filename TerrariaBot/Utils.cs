using System.IO;

namespace TerrariaBot
{
    public static class Utils
    {
        public static void WriteColor(this BinaryWriter w, Color color)
        {
            w.Write(color.r);
            w.Write(color.g);
            w.Write(color.b);
        }
    }
}
