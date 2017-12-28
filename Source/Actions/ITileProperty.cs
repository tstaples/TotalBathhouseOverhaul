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

        /// <summary>Parses the data set on the tile and verifies if this property should be run.</summary>
        /// <param name="property">The property to parse.</param>
        /// <returns>True if this property should be executed.</returns>
        bool Parse(PropertyValue property);

        /// <summary>Runs the logic for this property.</summary>
        void Execute(IModHelper helper, IMonitor monitor);
    }
}
