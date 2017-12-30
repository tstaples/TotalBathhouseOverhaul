namespace TotalBathhouseOverhaul
{
    /// <summary>
    /// Interface for converting specific tilesheet names to a unique Id, as well as getting the name/path of season specific sheets.
    /// </summary>
    internal interface ITileSheetResolver
    {
        // Gets the generic id for this sheet - ignoring season.
        string[] GetUniqueIdsForSeasonalTileSheets();

        /// <summary>Gets the unique Id for the given id. If the Id is not one handled by this resolver, the id should be returned.</summary>
        string GetUniqueIdForTileSheet(string id);

        /// <summary>Gets the unique Id for the given id. If the Id is not one handled by this resolver, the id should be returned.</summary>
        string GetTileSheetIdForSeason(string uniqueId, string season);
        string GetTileSheetPathForSeason(string uniqueId, string season);
    }
}
