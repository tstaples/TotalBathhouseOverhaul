using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace TotalBathhouseOverhaul
{
    internal class GamepadInputContext : IInputContext
    {
        public static readonly GamepadInputContext DefaultContext = new GamepadInputContext();

        public ICursorPosition CursorPosition { get; set; }

        // This class is stateless so there's no reason to create a new instance.
        private GamepadInputContext()
        {
        }

        public Point GetGrabTIlePoint()
        {
            Vector2 grabTile = this.CursorPosition.GrabTile;
            // The game lets you interact with inspectables even if you're not directly in front of it by
            // checking the next tile over in the direction you're facing if the tile directly in front has no action.
            if (!Game1.currentLocation.isActionableTile((int)grabTile.X, (int)grabTile.Y, Game1.player))
            {
                grabTile = Utility.getTranslatedVector2(grabTile, Game1.player.facingDirection, 1f);
            }
            return Utility.Vector2ToPoint(grabTile);
        }
    }
}
