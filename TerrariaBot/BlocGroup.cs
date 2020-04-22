using System.Linq;

namespace TerrariaBot
{
    internal static class BlocGroup
    {
        internal static bool IsBlockSolide(Bloc bloc)
        {
            if ((int)bloc > 190) return true;
            return solidBlocks.Contains(bloc) || oreBlocks.Contains(bloc) || trapBlocks.Contains(bloc);
        }

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
    }
}
