using System.Runtime.InteropServices;

namespace TerrariaBot
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NetworkMessage
    {
        public ushort length;
        public byte type; // uint8
        public string payload;
    }
}
