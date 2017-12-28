using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using xTile;
using xTile.Tiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using xTile.ObjectModel;
using StardewValley.BellsAndWhistles;
using xTile.Dimensions;
using xTile.Layers;


namespace TotalBathhouseOverhaul
{
    public class TotalBathhouseOverhaul : Mod
    {
        //names of things that we need
        private const string assetsRoot = "Assets";
        private string bathhouseLocationFilename => Path.Combine(assetsRoot, "TotalBathHouseOverhaul.tbin");
        private string steamSpriteSheetFilename => Path.Combine(assetsRoot, "ztotalbathhouseoverhaul_steam.png");

        private RailroadPatcher RailroadPatcher;

        private ActionManager ActionManager;
        private IInputContext CurrentInputContext;

        public static TotalBathhouseOverhaul instance;

        public IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            //capture the singleton instance and a ref to helper for use around the mod.
            instance = this;
            instance.helper = helper;

            //load the mod's ability to inject custom scheduling
            helper.Content.AssetEditors.Add(new ScheduleLibrary());

            //try to initialize the character schedules library.
            ScheduleLibrary.Initialize(instance.helper);

            this.RailroadPatcher = new RailroadPatcher(this.Monitor, helper);

            this.CurrentInputContext = MouseInputContext.DefaultContext;
            this.ActionManager = new ActionManager(this.Helper, this.Monitor);
            this.ActionManager.AddTileProperty(new ChangeClothesAction());
            this.ActionManager.AddTileProperty(new MessageAction());

            //wire up various events
            AddEventHandlers();
        }

        private void AddEventHandlers()
        {
            InputEvents.ButtonPressed += InputEvents_ButtonPressed;

            //load the game locations
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;

            //we have to shred the custom bathhouse location before save or it will fail serialization
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterReturnToTitle += SaveEvents_BeforeSave;

            //then load it back after serialization failure is avoided
            SaveEvents.AfterSave += SaveEvents_AfterSave;
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
                Game1.warpFarmer("CustomBathhouse", 27, 30, false);
            }
        }

        private void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            LoadBathhouseMap();
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            Game1.locations.Remove(Game1.getLocationFromName("CustomBathhouse"));
        }

        private static Point GetMouseHitLocation()
        {
            //I wrote this part, basically.
            double mouseX = (double)(Game1.getMouseX() + Game1.viewport.X - Game1.player.getStandingX());
            double mouseY = (double)(Game1.getMouseY() + Game1.viewport.Y - Game1.player.getStandingY());

            //figure out where the cursor position should be, relative to the player.
            return new Point((int)Math.Round((mouseX + Game1.player.getStandingX() - (Game1.tileSize / 2)) / Game1.tileSize), (int)Math.Round((mouseY + Game1.player.getStandingY() - (Game1.tileSize / 2)) / Game1.tileSize));
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            LoadBathhouseMap();

            try
            {
               this.RailroadPatcher.OnGameLoaded();
            }
            catch (FailedToLoadTilesheetException)
            {
                UnloadMod();
            }
        }

        // Executed after a new day starts
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            // If it's the start of a season, load the new tilesheet texture and set it to the new image source for the custom tilesheet
            if (Game1.dayOfMonth == 1)
            {
                try
                {
                    this.RailroadPatcher.OnSeasonChanged();
                }
                catch (FailedToLoadTilesheetException)
                {
                    UnloadMod();
                }
            }
        }

        private void LoadBathhouseMap()
        {
            //if for whatever reason this exists already, abort. There's a problem.
            if (Game1.getLocationFromName("CustomBathhouse") != null)
                return;

            //load in the TBO sweet sweet tbin
            Map map = helper.Content.Load<Map>(bathhouseLocationFilename);

            //ento's hax require some custom manipulation of the always-front later
            if (map.Layers.Contains(map.GetLayer("AlwaysFront")))
                map.RemoveLayer(map.GetLayer("AlwaysFront"));

            Texture2D steamTexture = this.Helper.Content.Load<Texture2D>(steamSpriteSheetFilename);

            // add the new location
            GameLocation location = new CustomBathhouse(map, "CustomBathhouse", steamTexture) { IsOutdoors = false, IsFarm = false };

            Game1.locations.Add(location);

            //apparently this does things
            if (location.map.Properties.ContainsKey("DayTiles"))
                location.map.Properties.Remove("DayTiles");

            //apparently this does things too.
            if (location.map.Properties.ContainsKey("NightTiles"))
                location.map.Properties.Remove("NightTiles");

            //LoadBathhouseTilesheet(location);

            //from Ento, no clue why this works. My life is a mess.
            location.map = map;

            //more ento hax
            location.ignoreLights = true;
        }
 
        // Unloads the mod (used when something goes wrong)
        private void UnloadMod()
        {
            // Unsubscribe from events
            SaveEvents.AfterLoad -= this.SaveEvents_AfterLoad;
            TimeEvents.AfterDayStarted -= this.TimeEvents_AfterDayStarted;
        }
    }
}
