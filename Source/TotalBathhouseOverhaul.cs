using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Tiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using xTile.ObjectModel;
using StardewValley.BellsAndWhistles;

namespace TotalBathhouseOverhaul
{
    public class TotalBathhouseOverhaul : Mod
    {
        //note locker locations female and male, respectively:
        //(7, 16) and (47, 16).
        public static Point femaleLockerLocation = new Point(7, 16);
        public static Point maleLockerLocation = new Point(47, 16);

        //names of things that we need
        private const string bathhouseLocationFilename = "TotalBathHouseOverhaul.tbin";
        private const string steamSpriteSheetFilename = "ztotalbathhouseoverhaul_steam.png";

        //note wall locations if needed
        //(14, 0) [5]
        //(15-39, 0) [6]
        //(40, 0) [7]
        //(14, 1-7) [20]
        //(40, 1-7) [22]
        //(14, 8) [65]
        //(40, 8) [66]
        //(14, 12) [50]
        //(40, 12) [51]
        //(14, 13-19) [20]
        //(40, 13-19) [22]
        //(14, 20) [35]
        //(40, 20) [37]
        //(15-39, 20) [36] walls

        //todo turn this https://pastebin.com/EVFNqzAE
        //into clean loops

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
            
            //wire up various events
            AddEventHandlers();
        }

        private void AddEventHandlers()
        {
            //hook up a keybind to warp to the map
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;

            //handle controller key presses and releases
            ControlEvents.ControllerButtonPressed += this.ControlEvents_ControllerButtonPressed;
            ControlEvents.ControllerButtonReleased += this.ControlEvents_ControllerButtonReleased;

            //handle mouse state changes [press/release]
            ControlEvents.MouseChanged += this.ControlEvents_MouseChanged;

            //load the game locations
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;

            //we have to shred the custom bathhouse location before save or it will fail serialization
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;

            //then load it back after serialization failure is avoided
            SaveEvents.AfterSave += SaveEvents_AfterSave;
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
        
        //ripped out of Entoarox's framework, this is how he handles Actionable tiles in a controller-friendly way
        private void ControlEvents_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (e.ButtonPressed == Buttons.A)
                CheckForAction();
        }

        private void ControlEvents_ControllerButtonReleased(object sender, EventArgsControllerButtonReleased e)
        {
            if (this.actionInfo != null && e.ButtonReleased == Buttons.A)
            {
                FireActionTriggered(this.actionInfo);
                this.actionInfo = null;
            }
        }

        private void ControlEvents_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if (e.NewState.RightButton == ButtonState.Pressed && e.PriorState.RightButton != ButtonState.Pressed)
                CheckForAction();
            if (this.actionInfo != null && e.NewState.RightButton == ButtonState.Released)
            {
                FireActionTriggered(this.actionInfo);
                this.actionInfo = null;
            }
        }

        private void FireActionTriggered(Tuple<StardewValley.Farmer, string, string[], Vector2> actionInfo)
        {
            if (actionInfo != null)
                FireActionTriggered(actionInfo.Item1, actionInfo.Item2, actionInfo.Item3, actionInfo.Item4);
        }

        private void FireActionTriggered(StardewValley.Farmer player, string property, string[] args, Vector2 tilePosition)
        {
            if (!string.IsNullOrEmpty(property))
            {
                switch (property)
                {
                    //de facto change clothes event for the mod, it's all we care about handling at the time of writing.
                    case "ChangeClothes":
                        InitiateChangeClothesFadeout();
                        break;
                }
            }
        }

        //private bool isChangingClothes = false;
        //private bool hadSwimClothes = false;

        private static void SwitchBathingClothesAndClearFade()
        {
            if (Game1.player.bathingClothes)
            {
                Game1.player.changeOutOfSwimSuit();
            } else
            {
                Game1.player.changeIntoSwimsuit();
            }
            Game1.globalFadeToClear(null);
        }

        private void InitiateChangeClothesFadeout()
        {
            Game1.afterFadeFunction changeClothesDelegate = new Game1.afterFadeFunction(SwitchBathingClothesAndClearFade);
            Game1.globalFadeToBlack(changeClothesDelegate);
        }

        private Tuple<StardewValley.Farmer, string, string[], Vector2> actionInfo;

        //almost verbatim a rip from Ento's core, only the event args are replaced with a tuple because I'm not looking to add hookable events. I just want explicit handling.
        private void CheckForAction()
        {
            if (Game1.activeClickableMenu == null && !Game1.player.UsingTool && !Game1.pickingTool && !Game1.menuUp && (!Game1.eventUp || Game1.currentLocation.currentEvent.playerControlSequence) && !Game1.nameSelectUp && Game1.numberOfSelectedItems == -1 && !Game1.fadeToBlack)
            {                
                Vector2 grabTile = new Vector2((Game1.getOldMouseX() + Game1.viewport.X), (Game1.getOldMouseY() + Game1.viewport.Y)) / Game1.tileSize;
                if (!Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
                    grabTile = Game1.player.GetGrabTile();
                Tile tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(new xTile.Dimensions.Location((int)grabTile.X * Game1.tileSize, (int)grabTile.Y * Game1.tileSize), Game1.viewport.Size);
                PropertyValue propertyValue = null;
                if (tile != null)
                    tile.Properties.TryGetValue("Action", out propertyValue);
                if (propertyValue != null)
                {
                    string[] split = ((string)propertyValue).Split(' ');
                    string[] args = new string[split.Length - 1];
                    Array.Copy(split, 1, args, 0, args.Length);
                    this.actionInfo = new Tuple<StardewValley.Farmer, string, string[], Vector2>(Game1.player, split[0], args, grabTile);
                }
            }
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.KeyPressed.Equals(Keys.F7))
                Game1.warpFarmer("CustomBathhouse", 27, 30, false);
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            LoadBathhouseMap();
        }

        private void LoadBathhouseMap()
        {
            //if for whatever reason this exists already, abort. There's a problem.
            if (Game1.getLocationFromName("CustomBathhouse") != null)
                return;

            //load in the TBO sweet sweet tbin
            Map map = helper.Content.Load<Map>(bathhouseLocationFilename);

            //ento's hax require some custom manipulation of the always-front later
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

            //add actionable locker properties for changing clothes
            Tile maleLockerTile = location.map.GetLayer("Buildings").PickTile(new xTile.Dimensions.Location((int)maleLockerLocation.X * Game1.tileSize, (int)maleLockerLocation.Y * Game1.tileSize), Game1.viewport.Size);
            Tile femaleLockerTile = location.map.GetLayer("Buildings").PickTile(new xTile.Dimensions.Location((int)femaleLockerLocation.X * Game1.tileSize, (int)femaleLockerLocation.Y * Game1.tileSize), Game1.viewport.Size);

            foreach (Tile tile in new Tile[] { maleLockerTile, femaleLockerTile })
            {
                //if (Game1.player.isMale) //we're not gender sensitive, are we?
                tile.Properties.Add("Action", "ChangeClothes");
            }

            //LoadBathhouseTilesheet(location);

            //from Ento, no clue why this works. My life is a mess.
            location.map = map;

            //more ento hax
            location.ignoreLights = true;
        }
    }
}
