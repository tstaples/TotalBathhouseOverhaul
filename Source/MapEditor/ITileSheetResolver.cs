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

    internal interface ITileSheetGroup
    {
        // Unique identifier for this tilesheet. This is used as the tilesheet name.
        string UniqueId { get; }

        // The size of your tilesheet image (number of columns, number of rows).
        xTile.Dimensions.Size SheetSize { get; }
        // should always be 16x16 for maps
        xTile.Dimensions.Size TileSize { get; }

        string GetTileSheetForSeason(string season);
        string GetTileSheetPathForSeason(string season);
    }

    internal interface ITileSheetProvider
    {
        ITileSheetGroup[] TileSheetGroups { get; }

        ITileSheetGroup GetTileSheetGroupById(string uniqueId);
    }
}
