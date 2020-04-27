using System.Linq;

namespace TerrariaBot
{
    internal static class BlocGroup
    {
        internal static bool IsSolid(int id)
        {
            if (id == 0) // 0 is at the same time dirt and nothing
                return false;
            return _solidBlocks.Contains(id);
        }

        internal static readonly int blocPixelSize = 16; // A bloc is 16 pixels in the game

        private static readonly Bloc[] solidBlocks = new[]
        {
            Bloc.Nothing, Bloc.StoneBlock, Bloc.Grass, Bloc.CorruptGrass, Bloc.EbonstoneBlock, Bloc.Wood, Bloc.GrayBrick,
            Bloc.RedBrick, Bloc.ClayBlock, Bloc.BlueBrick, Bloc.GreenBrick, Bloc.PinkBrick, Bloc.GoldBrick, Bloc.SilverBrick,
            Bloc.CopperBrick, Bloc.SandBlock, Bloc.Glass, Bloc.AshBlock, Bloc.MudBlock, Bloc.JungleGrass, Bloc.SapphireBlock,
            Bloc.RubyBlock, Bloc.EmeraldBlock, Bloc.TopazBlock, Bloc.AmethystBlock, Bloc.DiamondBlock, Bloc.MushroomGrass,
            Bloc.ObsidianBrick, Bloc.HellstoneBrick, Bloc.HallowedGrass, Bloc.EbonsandBlock, Bloc.PearlsandBlock, Bloc.PearlstoneBlock,
            Bloc.PearlstoneBrick, Bloc.IridescentBrick, Bloc.MudstoneBrick, Bloc.CobaltBrick, Bloc.MythrilBrick, Bloc.SiltBlock,
            Bloc.WoodenBeam, Bloc.IceRodBlock, Bloc.ActiveStoneBlack, Bloc.DartTrap, Bloc.DemoniteBrick, Bloc.CandleCaneBlock,
            Bloc.GreenCandyCanBlock, Bloc.SnowBlock, Bloc.SnowBrick, Bloc.AdamantiteBeam, Bloc.SandstoneBrick, Bloc.EbonstoneBrick,
            Bloc.RedStucco, Bloc.YellowStucco, Bloc.GreenStucco, Bloc.GrayStucco, Bloc.Ebonwood, Bloc.RichMahogany, Bloc.Pearlwood,
            Bloc.RainbowBrick, Bloc.IceBlock, Bloc.PurpleIceBlock, Bloc.PinkIceBlock, Bloc.PineTreeBlock, Bloc.TinBrick,
            Bloc.TungstenBrick, Bloc.PlatiniumBrick, Bloc.TealMoss, Bloc.ChartreuseMoss, Bloc.RedMoss, Bloc.BlueMoss,
            Bloc.PurpleMoss, Bloc.CactusPlaced, Bloc.Cloud, Bloc.GlowingMushroomPlaced
        };

        private static readonly Bloc[] oreBlocks = new[]
        {
            Bloc.IronOre, Bloc.CopperOre, Bloc.GoldOre, Bloc.SilverOre, Bloc.DemoniteOre, Bloc.Meteorite, Bloc.Obsidian,
            Bloc.Hellstone, Bloc.CobaltOre, Bloc.MythrilOre, Bloc.AdamantiteOre, Bloc.TinOre, Bloc.LeadOre, Bloc.TungstenOre,
            Bloc.PlatiniumOre, Bloc.PreciousStone
        };

        private static readonly Bloc[] lootBlocks = new[]
        {
            Bloc.CrystalHeart, Bloc.Chest, Bloc.Pot, Bloc.PiggyBank, Bloc.Orb, Bloc.Safe
        };

        private static readonly Bloc[] trapBlocks = new[]
        {
            Bloc.CorruptionThorny, Bloc.Spike, Bloc.JungleThorn, Bloc.PressurePlate, Bloc.Boulder
        };

        private static readonly Bloc[] craftBlocks = new[]
        {
            Bloc.Bottle, Bloc.Anvil, Bloc.Furnace, Bloc.WorkBench, Bloc.Altar, Bloc.Hellforge, Bloc.Loom, Bloc.Keg, Bloc.CookingPot,
            Bloc.Sawmill, Bloc.Workshop, Bloc.Forge, Bloc.MythrilAnvil
        };

        private static readonly int[] _solidBlocks = new[]
        {
            379, 371, 357, 408, 409, 415, 416, 417, 418,
            232, 311, 312, 313, 315, 321, 322, 239, 380,
            367, 357, 368, 369, 325, 460, 326, 458, 459, 327, 345, 328, 329,
            421, 422, 426, 430, 431, 432, 433, 434, 446, 447, 448, 427,
            435, 436, 437, 438, 439,
            284, 346, 347, 348, 350, 370, 383, 385, 396, 397, 399, 401, 398, 400, 402, 403, 404, 407,
            170, 221, 272, 229, 230, 222, 223, 224, 225, 226, 235, 191, 211, 208, 192, 193, 194, 195,
            200, 203, 204, 189, 190, 198, 206, 248, 249, 250, 251, 252, 253, 273, 274,
            255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268,
            202, 188, 179, 381, 180, 181, 182, 183, 196, 197, 175, 176, 177, 162, 163, 164,
            234,
            137, 160, 161, 145, 146, 147, 148, 138, 140, 151, 152, 153, 154, 155, 156, 157, 158, 159,
            376, // Only top
            127, 130, 107, 108, 111, 109, 110, 112, 116, 117, 123, 118, 119, 120, 121, 122, 150, 199,
            0, // Need to check this value
            1, 2, 3, 4, 6, 7, 8, 9, 166, 167, 168, 169, 10, 11, 19, 22, 23, 25, 30,
            37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 53, 54, 56, 57, 58, 59, 60, 63, 64, 65, 66, 67, 68, 75, 76, 70,
            18, 14, 469, 16, 134, 114,
            87, 88, 101,
            384, 405, 387, 388
        };
    }
}
