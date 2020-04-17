namespace TerrariaBot
{
    public struct Color
    {
        public Color(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public byte r;
        public byte g;
        public byte b;

        public static Color White = new Color(255, 255, 0);
        public static Color Red = new Color(255, 0, 0);
        public static Color Green = new Color(0, 255, 0);
        public static Color Blue = new Color(0, 0, 255);
    }
}
