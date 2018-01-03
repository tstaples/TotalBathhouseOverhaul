using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;

namespace TotalBathhouseOverhaul
{
    //internal class BathHouseTileSheetResolver : ITileSheetResolver
    //{
    //    // Also the prefix of the tilesheets.
    //    private const string UniqueTilesheetID = "zpathtotalbathhouseoverhaulexterior";

    //    public string[] GetUniqueIdsForSeasonalTileSheets()
    //    {
    //        return new string[] { UniqueTilesheetID };
    //    }

    //    public string GetUniqueIdForTileSheet(string id)
    //    {
    //        if (id.StartsWith(UniqueTilesheetID))
    //        {
    //            return UniqueTilesheetID;
    //        }
    //        return id;
    //    }

    //    public string GetTileSheetIdForSeason(string uniqueId, string season)
    //    {
    //        return $"{UniqueTilesheetID}_{season}";
    //    }

    //    public string GetTileSheetPathForSeason(string uniqueId, string season)
    //    {
    //        return System.IO.Path.Combine(TotalBathhouseOverhaul.AssetsRoot, $"{GetTileSheetIdForSeason(uniqueId, season)}.png");
    //    }
    //}

    internal class TileSheetProvider : ITileSheetProvider
    {
        public ITileSheetGroup[] TileSheetGroups { get; set; }

        public TileSheetProvider() { }
        public TileSheetProvider(ITileSheetGroup tileSheetGroup)
        {
            this.TileSheetGroups = new ITileSheetGroup[] { tileSheetGroup };
        }
        public TileSheetProvider(ITileSheetGroup[] tileSheetGroups)
        {
            this.TileSheetGroups = tileSheetGroups;
        }

        public ITileSheetGroup GetTileSheetGroupById(string uniqueId)
        {
            return TileSheetGroups.FirstOrDefault(p => p.UniqueId == uniqueId);
        }
    }

    internal abstract class TileSheetGroup : ITileSheetGroup
    {
        public static Size DefaultTileSize = new Size(16, 16);

        public string UniqueId { get; set; }
        public Size SheetSize { get; set; }
        public Size TileSize { get; set; } = DefaultTileSize;

        public virtual string GetTileSheetForSeason(string season)
        {
            return Path.GetFileNameWithoutExtension(GetTileSheetPathForSeason(season));
        }

        public abstract string GetTileSheetPathForSeason(string season);
    }

    internal class SingleTileSheet : TileSheetGroup
    {
        public string TileSheetPath;

        public override string GetTileSheetPathForSeason(string season)
        {
            return this.TileSheetPath;
        }
    }

    enum Season
    {
        Spring,
        Summer,
        Fall,
        Winter
    }

    internal class SeasonalTileSheetGroup : TileSheetGroup
    {
        // If a season doesn't contain a sheet then use sheet for this season instead.
        public Season DefaultSeason => Season.Spring;
        public string DefaultTileSheetPath => this.TileSheetPaths.ContainsKey(this.DefaultSeason) ? this.TileSheetPaths[this.DefaultSeason] : null;

        public Dictionary<Season, string> TileSheetPaths = new Dictionary<Season, string>();

        public override string GetTileSheetPathForSeason(string season)
        {
            Season key = (Season)Enum.Parse(typeof(Season), season, true);
            if (this.TileSheetPaths.ContainsKey(key))
            {
                return this.TileSheetPaths[key];
            }
            return this.DefaultTileSheetPath;
        }
    }
}
