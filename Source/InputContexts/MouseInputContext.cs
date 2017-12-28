using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace TotalBathhouseOverhaul
{
    internal class MouseInputContext : IInputContext
    {
        public static readonly MouseInputContext DefaultContext = new MouseInputContext();

        public ICursorPosition CursorPosition { get; set; }

        // This class is stateless so there's no reason to create a new instance.
        private MouseInputContext()
        {
        }

        public Point GetGrabTIlePoint()
        {
            int xTile = (int)CursorPosition.Tile.X;
            int yTile = (int)CursorPosition.Tile.Y;
            if (Game1.currentLocation.isActionableTile(xTile, yTile, Game1.player))
            {
                return new Point(xTile, yTile);
            }
            // It's possible to have the 'inspect' hover icon when hovering above the tile that has the action.
            // The game handles this by checking the next tile down if the first one has no action. We're doing the same thing here.
            if (Game1.currentLocation.isActionableTile(xTile, yTile + 1, Game1.player))
            {
                return new Point(xTile, yTile + 1);
            }
            return Point.Zero;
        }
    }
}
