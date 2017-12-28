using System;
using StardewModdingAPI;
using StardewValley;

namespace TotalBathhouseOverhaul
{
    /// <summary>Toggles the player's bathing suit on/off.</summary>
    internal class ChangeClothesAction : TileAction
    {
        protected override string ActionName => "ChangeClothes";

        public override void Execute(IModHelper helper, IMonitor monitor)
        {
            Game1.globalFadeToBlack(() =>
            {
                if (Game1.player.bathingClothes)
                {
                    Game1.player.changeOutOfSwimSuit();
                }
                else
                {
                    Game1.player.changeIntoSwimsuit();
                }
                Game1.globalFadeToClear(null);
            });
        }
    }
}
