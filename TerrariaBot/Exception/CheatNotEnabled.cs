using System;

namespace TerrariaBot.Exception
{
    public class CheatNotEnabled : InvalidOperationException
    {
        public CheatNotEnabled() : base("Cheats must be enabled for this method to be called. You can enable them with the ToogleCheats(bool) method.")
        { }
    }
}
