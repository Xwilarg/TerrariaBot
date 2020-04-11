using System;

namespace TerrariaBot
{
    public struct PlayerInformation
    {
        public PlayerInformation(string name, byte hairVariant, Color hairColor, Color skinColor,
            Color eyesColor, Color shirtColor, Color underShirtColor, Color pantsColor,
            Color shoesColor, PlayerDifficulty difficulty)
        {
            if (name == null)
                throw new ArgumentNullException("Name cannot be null.");
            if (name.Length > 20)
                throw new ArgumentException("Name length must be inferior or equal to 20 characters.");
            this.name = name;
            this.hairVariant = hairVariant;
            this.hairColor = hairColor;
            this.skinColor = skinColor;
            this.eyesColor = eyesColor;
            this.shirtColor = shirtColor;
            this.underShirtColor = underShirtColor;
            this.pantsColor = pantsColor;
            this.shoesColor = shoesColor;
            this.difficulty = difficulty;
        }

        public static PlayerInformation GetRandomPlayer(string name, PlayerDifficulty difficulty)
        {
            Random r = new Random();
            return new PlayerInformation(name, (byte)r.Next(0, 100), GetRandomColor(r), GetRandomColor(r), GetRandomColor(r), GetRandomColor(r), GetRandomColor(r), GetRandomColor(r), GetRandomColor(r), difficulty);
        }

        private static Color GetRandomColor(Random r)
        {
            return new Color((byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255));
        }

        public string name;
        public byte hairVariant;
        public Color hairColor;
        public Color skinColor;
        public Color eyesColor;
        public Color shirtColor;
        public Color underShirtColor;
        public Color pantsColor;
        public Color shoesColor;
        public PlayerDifficulty difficulty;
    }
}
