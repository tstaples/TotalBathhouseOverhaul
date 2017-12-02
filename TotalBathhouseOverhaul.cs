using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Tiles;

namespace TotalBathhouseOverhaul
{
    public class TotalBathhouseOverhaul : Mod
    {
        //note locker locations female and male, respectively:
        //(7, 16) and (47, 16).

        public static TotalBathhouseOverhaul instance;

        public IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            instance.helper = helper;

            helper.Content.AssetEditors.Add(new ScheduleLibrary());

            //try to initialize the character schedules library.
            ScheduleLibrary.Initialize(instance.helper);

            //load the game locations
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;

            //hook up a keybind to warp to the map
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.KeyPressed.Equals(Keys.F7))
                Game1.warpFarmer("TotalBathhouseOverhaul", 27, 30, false);
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            //load in the TBO sweet sweet tbin
            Map map = helper.Content.Load<Map>("TotalBathHouseOverhaul.tbin", ContentSource.ModFolder);

            // add the new location
            //the map name is long but unlikely to suffer from collision. Is rly gud name.
            GameLocation location = new GameLocation(map, "TotalBathhouseOverhaul") { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(location);

            LoadBathhouseTilesheet();
        }

        private void LoadBathhouseTilesheet()
        {
            // This gets the asset key for a tilesheet.png file from your mod's folder. You can also load a game tilesheet like
            // this: helper.Content.GetActualAssetKey("spring_town", ContentSource.GameContent).
            string tilesheetPath = helper.Content.GetActualAssetKey("ztotalbathhouseoverhaul_tiles.png", ContentSource.ModFolder);

            // Get an instance of the in-game location you want to patch. For the farm, use Game1.getFarm() instead.
            GameLocation location = Game1.getLocationFromName("TotalBathhouseOverhaul");

            // Add the tilesheet.
            TileSheet tilesheet = new TileSheet(
               id: "tbo-spritesheet", // a unique ID for the tilesheet
               map: location.map,
               imageSource: tilesheetPath,
               sheetSize: new xTile.Dimensions.Size(240, 432), // the pixel size of your tilesheet image.
               tileSize: new xTile.Dimensions.Size(16, 16) // should always be 16x16 for maps
            );
            location.map.AddTileSheet(tilesheet);
            location.map.LoadTileSheets(Game1.mapDisplayDevice);
        }
    }
}
