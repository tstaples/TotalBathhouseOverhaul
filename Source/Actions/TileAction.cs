using System;
using StardewModdingAPI;
using xTile.ObjectModel;

namespace TotalBathhouseOverhaul
{
    /// <summary>Base class for any Action properties.</summary>
    internal abstract class TileAction : TileProperty
    {
        public override TilePropertyType PropertyType => TilePropertyType.Action;
    }
}
