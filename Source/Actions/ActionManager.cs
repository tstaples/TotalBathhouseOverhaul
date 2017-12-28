using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using xTile.ObjectModel;
using xTile.Tiles;

namespace TotalBathhouseOverhaul
{
    // TODO: Put the generic detection in a base class and derive
    // for detecting the different property types (action, touch action etc.)
    internal class ActionManager
    {
        private IModHelper Helper;
        private IMonitor Monitor;
        private List<ITileProperty> TileProperties;

        public ActionManager(IModHelper helper, IMonitor monitor)
        {
            this.Helper = helper;
            this.Monitor = monitor;
            this.TileProperties = new List<ITileProperty>();
        }

        public void AddTileProperty(ITileProperty tileProperty)
        {
            this.TileProperties.Add(tileProperty);
        }

        public bool CanCheckForAction()
        {
            return (Game1.activeClickableMenu == null && 
                    !Game1.player.UsingTool && 
                    !Game1.pickingTool && 
                    !Game1.menuUp && 
                    (!Game1.eventUp || Game1.currentLocation.currentEvent.playerControlSequence) && 
                    !Game1.nameSelectUp && 
                    Game1.numberOfSelectedItems == -1 && 
                    !Game1.fadeToBlack);
        }

        public void CheckForAction(IInputContext inputContext)
        {
            Point grabTilePoint = inputContext.GetGrabTIlePoint();
            // Prevent running the action if the player is too far.
            if (!Utility.tileWithinRadiusOfPlayer(grabTilePoint.X, grabTilePoint.Y, 1, Game1.player))
                return;

            Tile selectedTile = GetTileAtPoint(grabTilePoint);
            if (selectedTile != null)
            {
                RunTileProperties(selectedTile);
            }
        }

        private Tile GetTileAtPoint(Point point)
        {
            xTile.Dimensions.Location xTileLocation = new xTile.Dimensions.Location(point.X * Game1.tileSize, point.Y * Game1.tileSize);
            Tile tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(xTileLocation, Game1.viewport.Size);
            return tile;
        }

        private void RunTileProperties(Tile tile)
        {
            foreach (var pair in tile.Properties)
            {
                // TODO: could be improved by holding the properties as a map of string to arrays
                // where the key corredponds with the tile property key (ie. action).
                foreach (ITileProperty tileProperty in this.TileProperties)
                {
                    if (tileProperty.PropertyType.ToString() == pair.Key)
                    {
                        tileProperty.RunOnProperty(pair.Value, this.Helper, this.Monitor);
                    }
                }
            }
        }
    }
}
