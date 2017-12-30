﻿using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.IO;
using xTile;
using Microsoft.Xna.Framework.Graphics;
using System;
using xTile.Tiles;
using System.Linq;
using xTile.Dimensions;
using Microsoft.Xna.Framework;

namespace TotalBathhouseOverhaul
{
    public class TotalBathhouseOverhaul : Mod
    {
        public const string BathhouseLocationName = "TotalBathhouseOverhaul";

        // Asset paths.
        public const string AssetsRoot = "Assets";
        private string BathhouseLocationFilename => Path.Combine(AssetsRoot, "TotalBathHouseOverhaul.tbin");
        private string SteamSpriteSheetFilename => Path.Combine(AssetsRoot, "ztotalbathhouseoverhaul_steam.png");

        // Handles NPC schedules.
        private ScheduleLibrary ScheduleLibrary;

        private MapEditor MapEditor;

        // Detects when custom actions are fired and runs them.
        private ActionManager ActionManager;
        private IInputContext CurrentInputContext;

        public override void Entry(IModHelper helper)
        {
            //load the mod's ability to inject custom scheduling
            this.ScheduleLibrary = ScheduleLibrary.Create(helper);
            helper.Content.AssetEditors.Add(this.ScheduleLibrary);

            this.MapEditor = new MapEditor(helper, this.Monitor);

            this.CurrentInputContext = MouseInputContext.DefaultContext;
            this.ActionManager = new ActionManager(this.Helper, this.Monitor);
            this.ActionManager.AddTileProperty(new ChangeClothesAction());
            this.ActionManager.AddTileProperty(new MessageAction());

            //wire up various events
            AddEventHandlers();

#if DEBUG
            // Changes the season and updates the map.
            helper.ConsoleCommands.Add("change_season", "", (name, args) =>
            {
                helper.ConsoleCommands.Trigger("world_setseason", args);
                this.MapEditor.SeasonChanged(Game1.currentSeason);
            });
#endif // DEBUG
        }

        private void AddEventHandlers()
        {
            // Listen to input events (keyboard/mouse and gamepad).
            InputEvents.ButtonPressed += InputEvents_ButtonPressed;

            // Watch for season changes.
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;

            //load the game locations
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;

            //we have to shred the custom bathhouse location before save or it will fail serialization
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterReturnToTitle += SaveEvents_BeforeSave;

            //then load it back after serialization failure is avoided
            SaveEvents.AfterSave += SaveEvents_AfterSave;
        }

        private void UnloadMod()
        {
            this.Monitor.Log("Unloading.", LogLevel.Info);
            InputEvents.ButtonPressed -= InputEvents_ButtonPressed;
            SaveEvents.AfterLoad -= SaveEvents_AfterLoad;
            SaveEvents.BeforeSave -= SaveEvents_BeforeSave;
            SaveEvents.AfterReturnToTitle -= SaveEvents_BeforeSave;
            SaveEvents.AfterSave -= SaveEvents_AfterSave;
            TimeEvents.AfterDayStarted -= TimeEvents_AfterDayStarted;
            Game1.locations.Remove(Game1.getLocationFromName(BathhouseLocationName));
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady)
                return;

            // TODO: remove ControllerA check once IsActionButton works for gamepads. https://github.com/Pathoschild/SMAPI/issues/416
            if (e.IsActionButton || e.Button == SButton.ControllerA)
            {
                const int controllerOffset = 2000;
                bool isGamepad = (int)e.Button > controllerOffset;
                this.CurrentInputContext = isGamepad ? (IInputContext)GamepadInputContext.DefaultContext : MouseInputContext.DefaultContext;
                this.CurrentInputContext.CursorPosition = e.Cursor;

                if (this.ActionManager.CanCheckForAction())
                {
                    this.ActionManager.CheckForAction(this.CurrentInputContext);
                }
            }
            else if (e.Button.Equals(SButton.F7))
            {
                Game1.warpFarmer(BathhouseLocationName, 27, 30, false);
            }
            else if (e.Button.Equals(SButton.F8))
            {
                // Warp to bed
                Point bedSpot = (Game1.getLocationFromName("FarmHouse") as StardewValley.Locations.FarmHouse).getBedSpot();
                Game1.warpFarmer("FarmHouse", bedSpot.X, bedSpot.Y, false);
            }
        }

        private void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            // Add the location back once saving is finished.
            LoadBathhouseMap();
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            // Remove our location so it doesn't get saved to disk.
            Game1.locations.Remove(Game1.getLocationFromName(BathhouseLocationName));
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            LoadBathhouseMap();

            string vanillaPath = Path.Combine(AssetsRoot, "Railroad_Original.tbin");
            string modifiedPath = Path.Combine(AssetsRoot, "Railroad.tbin");

            if (this.MapEditor.Init("RailRoad", vanillaPath, modifiedPath, new BathHouseTileSheetResolver()))
            {
                this.MapEditor.Patch(AssetsRoot);
            }
        }

        // Executed after a new day starts
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            // If it's the start of a season, load the new tilesheet texture and set it to the new image source for the custom tilesheet
            if (Game1.dayOfMonth == 1)
            {
                this.MapEditor.SeasonChanged(Game1.currentSeason);
                // TODO: Do season change stuff in mapeditor
            }
        }

        private void LoadBathhouseMap()
        {
            //if for whatever reason this exists already, abort. There's a problem.
            if (Game1.getLocationFromName(BathhouseLocationName) != null)
                return;

            //load in the TBO sweet sweet tbin
            Map map = this.Helper.Content.Load<Map>(BathhouseLocationFilename);

            //ento's hax require some custom manipulation of the always-front later
            if (map.Layers.Contains(map.GetLayer("AlwaysFront")))
                map.RemoveLayer(map.GetLayer("AlwaysFront"));

            Texture2D steamTexture = this.Helper.Content.Load<Texture2D>(SteamSpriteSheetFilename);

            // add the new location
            GameLocation location = new CustomBathhouse(map, BathhouseLocationName, steamTexture) { IsOutdoors = false, IsFarm = false };

            Game1.locations.Add(location);

            // TODO: cleanup location adding.
            Map sennaMap = this.Helper.Content.Load<Map>(Path.Combine(AssetsRoot, "SennaRoom.tbin"));
            GameLocation sennasRoom = new GameLocation(sennaMap, "SennaRoom");
            Game1.locations.Add(sennasRoom);

            //apparently this does things
            if (location.map.Properties.ContainsKey("DayTiles"))
                location.map.Properties.Remove("DayTiles");

            //apparently this does things too.
            if (location.map.Properties.ContainsKey("NightTiles"))
                location.map.Properties.Remove("NightTiles");

            //from Ento, no clue why this works. My life is a mess.
            location.map = map;

            //more ento hax
            location.ignoreLights = true;
        }
    }
}
