using System;
using System.IO;
using TerrariaBot.Client;

namespace TerrariaBot
{
    public class PlayerInformation
    {
        public PlayerInformation(string name, byte hairVariant, Color hairColor, Color skinColor,
            Color eyesColor, Color shirtColor, Color underShirtColor, Color pantsColor,
            Color shoesColor, PlayerDifficulty difficulty)
        {
            if (name == null)
                throw new ArgumentNullException("Name cannot be null.");
            if (name.Length > 20)
                throw new ArgumentException("Name length must be inferior or equal to 20 characters.");
            _name = name;
            _hairVariant = hairVariant;
            _hairColor = hairColor;
            _skinColor = skinColor;
            _eyesColor = eyesColor;
            _shirtColor = shirtColor;
            _underShirtColor = underShirtColor;
            _pantsColor = pantsColor;
            _shoesColor = shoesColor;
            _difficulty = difficulty;
            _health = 100;
            _mana = 20;
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

        internal void Write(BinaryWriter writer)
        {
            writer.Write((byte)1); // Unknown
            writer.Write(_hairVariant);
            writer.Write(_name);
            writer.Write((byte)1); // Unknown
            writer.Write((byte)1); // Unknown
            writer.Write((byte)1); // Unknown
            writer.Write((byte)1); // Unknown
            writer.Write(_hairColor);
            writer.Write(_skinColor);
            writer.Write(_eyesColor);
            writer.Write(_shirtColor);
            writer.Write(_underShirtColor);
            writer.Write(_pantsColor);
            writer.Write(_shoesColor);
            writer.Write((byte)_difficulty);
        }

        public void SetHealth(short value)
        {
            _health = value;
        }

        public void SetMana(short value)
        {
            _mana = value;
        }

        private string _name; internal string GetName() => _name; internal int GetNameLength() => _name.Length;
        private byte _hairVariant;
        private Color _hairColor;
        private Color _skinColor;
        private Color _eyesColor;
        private Color _shirtColor;
        private Color _underShirtColor;
        private Color _pantsColor;
        private Color _shoesColor;
        private PlayerDifficulty _difficulty;
        private short _health; internal short GetHealth() => _health;
        private short _mana; internal short GetMana() => _mana;
    }
}
