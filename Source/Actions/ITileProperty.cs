using StardewModdingAPI;
using xTile.ObjectModel;

namespace TotalBathhouseOverhaul
{
    internal enum TilePropertyType
    {
        /// Makes something happen when the player interacts (e.g. clicks) with the tile.
        Action,
        /// Makes something happen when the player steps on the tile
        TouchAction
    }

    /// <summary>Represents a tile property.</summary>
    internal interface ITileProperty
    {
        /// <summary>The property type.</summary>
        TilePropertyType PropertyType { get; }

        /// <summary>Runs the logic for this property.</summary>
        void RunOnProperty(PropertyValue property, IModHelper helper, IMonitor monitor);
    }
}
