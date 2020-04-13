namespace TerrariaBot
{
    /// <summary>
    /// Change default player values. Null values will be kept as default
    /// </summary>
    public struct PlayerStartModifier
    {
        public PlayerStartModifier(short? healthModifier = null, short? manaModifier = null)
        {
            this.healthModifier = healthModifier;
            this.manaModifier = manaModifier;
        }

        public short? healthModifier;
        public short? manaModifier;
    }
}
