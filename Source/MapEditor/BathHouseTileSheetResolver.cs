namespace TotalBathhouseOverhaul
{
    internal class BathHouseTileSheetResolver : ITileSheetResolver
    {
        // Also the prefix of the tilesheets.
        private const string UniqueTilesheetID = "zpathtotalbathhouseoverhaulexterior";

        public string[] GetUniqueIdsForSeasonalTileSheets()
        {
            return new string[] { UniqueTilesheetID };
        }

        public string GetUniqueIdForTileSheet(string id)
        {
            if (id.StartsWith(UniqueTilesheetID))
            {
                return UniqueTilesheetID;
            }
            return id;
        }

        public string GetTileSheetIdForSeason(string uniqueId, string season)
        {
            return $"{UniqueTilesheetID}_{season}";
        }

        public string GetTileSheetPathForSeason(string uniqueId, string season)
        {
            return System.IO.Path.Combine(TotalBathhouseOverhaul.AssetsRoot, $"{GetTileSheetIdForSeason(uniqueId, season)}.png");
        }
    }
}
