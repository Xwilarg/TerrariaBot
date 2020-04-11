namespace TerrariaBot
{
    public struct NetworkMessage
    {
        public ushort length;
        public byte type; // uint8
        public string payload;
    }
}
