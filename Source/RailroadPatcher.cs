
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.IO;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace TotalBathhouseOverhaul
{
    internal class FailedToLoadTilesheetException : Exception
    {
    }

    internal class RailroadPatcher
    {
        private IMonitor Monitor;
        private IModHelper Helper;

        private string tilesheetID = "ztotalbathhouseoverhaulexterior";

        public RailroadPatcher(IMonitor monitor, IModHelper helper)
        {
            this.Monitor = monitor;
            this.Helper = helper;
        }

        public void OnGameLoaded()
        {
            AddTileSheet();
            PatchRailroadLocation();
        }

        public void OnSeasonChanged()
        {
            // Get an instance of the in-game location you want to patch. For the farm, use Game1.getFarm().
            GameLocation railroad = Game1.getLocationFromName("Railroad");

            // Load new tilesheet texture and set it to current tilesheet's image source
            updateTileSheetForSeason(railroad);

            // Dispose tilesheets
            railroad.map.DisposeTileSheets(Game1.mapDisplayDevice);

            // Reload and cache tilesheets
            railroad.map.LoadTileSheets(Game1.mapDisplayDevice);
        }

        private void Unload()
        {
            GameLocation railroad = Game1.getLocationFromName("Railroad");
            TileSheet ts = railroad.map.GetTileSheet(tilesheetID);
            if (ts != null)
            {
                // Remove any tiles that depend on the custom tilesheet
                railroad.map.RemoveTileSheetDependencies(ts);

                // Remove custom tilesheet from the location
                railroad.map.RemoveTileSheet(ts);
            }
        }

        // Load and add initial tile sheet
        private void AddTileSheet()
        {
            string tileSheetPath = Path.Combine("assets", $"zpathtotalbathhouseoverhaulexterior_{Game1.currentSeason}.png");
            try
            {
                this.Helper.Content.Load<Texture2D>(tileSheetPath);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Could not load tilesheet texture.\n{ex}");
                Unload();
                throw new FailedToLoadTilesheetException();
            }

            // Get an instance of the in-game location you want to patch. For the farm, use Game1.getFarm().
            GameLocation railroad = Game1.getLocationFromName("Railroad");

            // Initialize tilesheet
            TileSheet ts = new TileSheet(
               id: tilesheetID, // a unique ID for the tilesheet
               map: railroad.map,
               imageSource: this.Helper.Content.GetActualAssetKey(tileSheetPath),
               sheetSize: new Size(15, 14), // the size of your tilesheet image (number of columns, number of rows).
               tileSize: new Size(16, 16) // should always be 16x16 for maps
            );

            // Add the tilesheet to the railroad
            railroad.map.AddTileSheet(ts);

            // Reload tilesheets in the location to load and cache the tilesheet, also prevents potential errors
            railroad.map.LoadTileSheets(Game1.mapDisplayDevice);
        }

        // Perform tile edits to the railroad
        private void PatchRailroadLocation()
        {
            // Get an instance of the in-game location you want to patch. For the farm, use Game1.getFarm().
            GameLocation railroad = Game1.getLocationFromName("Railroad");

            // Get a reference to the new exterior
            TileSheet tilesheet = railroad.map.GetTileSheet(tilesheetID);

            // Get a reference to the default tile sheet doing terrain cleanup
            TileSheet defaultTileSheet = railroad.map.GetTileSheet("untitled tile sheet");

            // Trim tiles on left side of building that do not have replacements
            railroad.removeTile(8, 45, "AlwaysFront");
            railroad.removeTile(7, 46, "AlwaysFront");
            railroad.removeTile(6, 47, "AlwaysFront");
            railroad.removeTile(6, 48, "AlwaysFront");

            for (int i = 49; i <= 54; ++i)
            {
                railroad.removeTile(6, i, "Buildings");
            }

            for (int i = 49; i <= 53; ++i)
            {
                railroad.removeTile(6, i, "Front");
            }

            // Remove front five building tiles so back will show
            for (int i = 12; i <= 16; ++i)
            {
                railroad.removeTile(i, 56, "Buildings");
            }

            // Remove reduntant old tiles from the front layer
            for (int i = 7; i <= 21; ++i)
            {
                railroad.removeTile(i, 49, "Front");
                railroad.removeTile(i, 50, "Front");
                railroad.removeTile(i, 51, "Front");
                railroad.removeTile(i, 52, "Front");
                railroad.removeTile(i, 53, "Front");
            }

            // Remove window and water feature tiles
            railroad.removeTile(8, 54, "Front");
            railroad.removeTile(12, 54, "Front");
            railroad.removeTile(15, 54, "Front");
            railroad.removeTile(16, 54, "Front");
            railroad.removeTile(8, 55, "Front");
            railroad.removeTile(12, 55, "Front");
            railroad.removeTile(15, 55, "Front");
            railroad.removeTile(16, 55, "Front");

            // Remove northernmost building layer of old bathhouse
            for (int i = 7; i <= 21; ++i)
            {
                railroad.removeTile(i, 49, "Buildings");
            }

            // Set new bathhouse exterior tiles
            for (int i = 9, n = 2; i <= 19 && n <= 12; i++, n++)
            {
                setTile(railroad, "AlwaysFront", i, 45, n, tilesheet);
            }

            for (int i = 8, n = 16; i <= 20 && n <= 28; i++, n++)
            {
                setTile(railroad, "AlwaysFront", i, 46, n, tilesheet);
            }

            for (int i = 7, n = 30; i <= 21 && n <= 44; i++, n++)
            {
                setTile(railroad, "AlwaysFront", i, 47, n, tilesheet);
            }

            for (int i = 7, n = 45; i <= 21 && n <= 59; i++, n++)
            {
                setTile(railroad, "AlwaysFront", i, 48, n, tilesheet);
            }

            for (int i = 7, n = 60; i <= 21 && n <= 74; i++, n++)
            {
                setTile(railroad, "AlwaysFront", i, 49, n, tilesheet);
            }

            for (int i = 7, n = 75; i <= 21 && n <= 89; i++, n++)
            {
                setTile(railroad, "AlwaysFront", i, 50, n, tilesheet);
            }

            for (int i = 7, n = 75; i <= 21 && n <= 89; i++, n++)
            {
                setTile(railroad, "Buildings", i, 50, n, tilesheet);
            }

            for (int i = 7, n = 90; i <= 21 && n <= 104; i++, n++)
            {
                setTile(railroad, "Buildings", i, 51, n, tilesheet);
            }

            for (int i = 7, n = 105; i <= 21 && n <= 119; i++, n++)
            {
                setTile(railroad, "Buildings", i, 52, n, tilesheet);
            }

            for (int i = 7, n = 120; i <= 21 && n <= 134; i++, n++)
            {
                setTile(railroad, "Buildings", i, 53, n, tilesheet);
            }

            for (int i = 7, n = 135; i <= 21 && n <= 149; i++, n++)
            {
                setTile(railroad, "Buildings", i, 54, n, tilesheet);
            }

            for (int i = 7, n = 150; i <= 21 && n <= 164; i++, n++)
            {
                setTile(railroad, "Buildings", i, 55, n, tilesheet);
            }

            for (int i = 7, n = 165; i <= 21 && n <= 179; i++, n++)
            {
                setTile(railroad, "Buildings", i, 56, n, tilesheet);
            }

            // Cover up unneeded pathway
            for (int i = 10; i <= 13; ++i)
            {
                setTile(railroad, "Back", i, 58, 227, defaultTileSheet);
            }

            // Place new pathway
            setTile(railroad, "Back", 14, 58, 624, defaultTileSheet);
            setTile(railroad, "Back", 14, 57, 549, defaultTileSheet);

            // Clean up front building edge after pathway moving
            setTile(railroad, "Back", 13, 57, 565, defaultTileSheet);
            setTile(railroad, "Back", 15, 57, 670, defaultTileSheet);
            setTile(railroad, "Back", 9, 57, 562, defaultTileSheet);
            setTile(railroad, "Back", 10, 57, 562, defaultTileSheet);
            setTile(railroad, "Back", 11, 57, 562, defaultTileSheet);

            // Shift leftside edge dirt shadows right one
            setTile(railroad, "Back", 6, 50, 615, defaultTileSheet);
            for (int i = 51; i <= 55; ++i)
            {
                setTile(railroad, "Back", 6, i, 588, defaultTileSheet);
            }

            // Clean up dirt shadow corner
            setTile(railroad, "Back", 6, 56, 588, defaultTileSheet);
            setTile(railroad, "Back", 6, 57, 645, defaultTileSheet);

            // Replace old edge dirt shadows with plain dirt
            for (int i = 50; i <= 56; ++i)
            {
                setTile(railroad, "Back", 5, i, 227, defaultTileSheet);
            }

            // Add remaining shadow underneath left edge of building
            setTile(railroad, "Back", 7, 50, 537, defaultTileSheet);
            setTile(railroad, "Back", 7, 51, 537, defaultTileSheet);
            setTile(railroad, "Back", 7, 52, 537, defaultTileSheet);
            setTile(railroad, "Back", 7, 53, 537, defaultTileSheet);

            // Remove the barrels
            railroad.removeTile(4, 54, "Buildings");
            railroad.removeTile(5, 54, "Buildings");
            railroad.removeTile(5, 55, "Buildings");
            railroad.removeTile(6, 55, "Buildings");
            railroad.removeTile(6, 56, "Buildings");
            railroad.removeTile(18, 57, "Buildings");
            railroad.removeTile(19, 57, "Buildings");

            railroad.removeTile(4, 53, "Front");
            railroad.removeTile(5, 53, "Front");
            railroad.removeTile(5, 54, "Front");
            railroad.removeTile(6, 54, "Front");
            railroad.removeTile(6, 55, "Front");
            railroad.removeTile(18, 56, "Front");
            railroad.removeTile(19, 56, "Front");

            // Move some rocks
            setTile(railroad, "Back", 12, 59, 227, defaultTileSheet);
            setTile(railroad, "Back", 12, 58, 610, defaultTileSheet);

            railroad.removeTile(5, 52, "Buildings");
            setTile(railroad, "Buildings", 5, 54, 282, defaultTileSheet);

            // Make door functional
            railroad.setTileProperty(14, 55, "Buildings", "Action", "Warp 27 30 CustomBathhouse");

            // Set entryway tiles to passable
            tilesheet.Properties.Add($"@TileIndex@{171}@Passable", new PropertyValue(true));
            tilesheet.Properties.Add($"@TileIndex@{172}@Passable", new PropertyValue(true));
            tilesheet.Properties.Add($"@TileIndex@{173}@Passable", new PropertyValue(true));

            // Make entryway tiles sound like stone
            tilesheet.Properties.Add($"@TileIndex@{171}@Type", new PropertyValue("Stone"));
            tilesheet.Properties.Add($"@TileIndex@{172}@Type", new PropertyValue("Stone"));
            tilesheet.Properties.Add($"@TileIndex@{173}@Type", new PropertyValue("Stone"));

            // Put lamps on exterior
            setTile(railroad, "Front", 11, 55, 186, tilesheet);
            setTile(railroad, "Front", 17, 55, 186, tilesheet);
            railroad.map.Properties["DayTiles"] = "Front 11 55 186 Front 17 55 186";
            railroad.map.Properties["NightTiles"] = "Front 11 55 201 Front 17 55 201";
            railroad.map.Properties["Light"] = "11 55 4 17 55 4";
        }

        // Used for every individual tile edit
        private void setTile(GameLocation gl, string layerName, int tileX, int tileY, int tileID, TileSheet ts, bool removeWater = false)
        {
            Layer layer = gl.map.GetLayer(layerName);
            layer.Tiles[tileX, tileY] = new StaticTile(layer, ts, BlendMode.Alpha, tileID);

            if (removeWater)
                gl.waterTiles[tileX, tileY] = false;
        }

        // Load new tilesheet texture and set it to current tilesheet's image source
        private void updateTileSheetForSeason(GameLocation railroad)
        {
            string tileSheetPath = $"zpathtotalbathhouseoverhaulexterior_{Game1.currentSeason}.png";

            try
            {
                this.Helper.Content.Load<Texture2D>(tileSheetPath);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Could not load tilesheet texture.\n{ex}");
                Unload();
                throw new FailedToLoadTilesheetException();
            }

            railroad.map.GetTileSheet(tilesheetID).ImageSource = $"{this.Helper.Content.GetActualAssetKey(tileSheetPath)}";
        }
    }
}
