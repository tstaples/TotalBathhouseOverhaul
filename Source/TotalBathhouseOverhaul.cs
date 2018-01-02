using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.IO;
using xTile;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TotalBathhouseOverhaul
{
    public class TotalBathhouseOverhaul : Mod
    {
        public const string BathhouseLocationName = "TotalBathhouseOverhaul";
        public const string SennaRoomLocationName = "SennaRoom";

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
        }

        private void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            // Add the location back once saving is finished.
            LoadCustomLocations();
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            // Remove our location so it doesn't get saved to disk.
            Game1.locations.Remove(Game1.getLocationFromName(BathhouseLocationName));
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            LoadCustomLocations();

            string vanillaPath = Path.Combine(AssetsRoot, "Railroad_Original.tbin");
            string modifiedPath = Path.Combine(AssetsRoot, "Railroad.tbin");

            if (this.MapEditor.Init("RailRoad", vanillaPath, modifiedPath))
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
                // TODO: Do season change stuff in mapeditor
            }
        }

        private void LoadCustomLocations()
        {
            LoadBathhouseMap();
            LoadSennaRoom();
        }

        private void LoadBathhouseMap()
        {
            //if for whatever reason this exists already, abort. There's a problem.
            if (Game1.getLocationFromName(BathhouseLocationName) != null)
                return;

            //load in the TBO sweet sweet tbin
            Map map = this.Helper.Content.Load<Map>(BathhouseLocationFilename);

            // The TBin contains fog on the AlwaysFront Layer, but we're adding our own in so just remove this layer.
            // This can be removed once the fog is taken out of that layer.
            if (map.Layers.Contains(map.GetLayer("AlwaysFront")))
                map.RemoveLayer(map.GetLayer("AlwaysFront"));

            Texture2D steamTexture = this.Helper.Content.Load<Texture2D>(SteamSpriteSheetFilename);

            // add the new location
            GameLocation location = new CustomBathhouse(map, BathhouseLocationName, steamTexture) { IsOutdoors = false, IsFarm = false };
            location.map = map;

            // Removing these and turning on ignoreLights ensures teh entire map is always fully lit.
            // Currently this has no affect since there's no lighting in the map anyway.
            if (location.map.Properties.ContainsKey("DayTiles"))
                location.map.Properties.Remove("DayTiles");
            if (location.map.Properties.ContainsKey("NightTiles"))
                location.map.Properties.Remove("NightTiles");
            location.ignoreLights = true;

            Game1.locations.Add(location);
        }

        private void LoadSennaRoom()
        {
            if (Game1.getLocationFromName(SennaRoomLocationName) != null)
                return;

            Map sennaMap = this.Helper.Content.Load<Map>(Path.Combine(AssetsRoot, "SennaRoom.tbin"));
            GameLocation sennaRoom = new GameLocation(sennaMap, SennaRoomLocationName) { IsOutdoors = false, IsFarm = false };
            PatchDoors(sennaRoom, Game1.mouseCursors, new Rectangle(512, 144, 16, 48));
            Game1.locations.Add(sennaRoom);
        }

        // The game has hard-coded tile indices for the doors and uses those to determine which animation to use.
        // To avoid having to have our doors at the same index we just overwrite the animation it makes with our own.
        private void PatchDoors(GameLocation location, Texture2D animationSheet, Rectangle sourceRect)
        {
            TemporaryAnimatedSprite GetDoorAnimation(Point tilePoint, bool flipped)
            {
                // This is based on GameLocation::loadLights().
                Vector2 tilePosition = Utility.PointToVector2(tilePoint);
                Vector2 position = new Vector2(tilePosition.X, (tilePosition.Y - 2)) * (float)Game1.tileSize;
                float layerDepth = (float)((tilePosition.Y + 1) * Game1.tileSize - Game1.pixelZoom * 3) / 10000f;

                var animatedSprite = new TemporaryAnimatedSprite(
                    texture: animationSheet,
                    sourceRect: sourceRect,
                    animationInterval: 100f,
                    animationLength: 4,
                    numberOfLoops: 1,
                    position: position,
                    flicker: false,
                    flipped: flipped,
                    layerDepth: layerDepth,
                    alphaFade: 0f,
                    color: Color.White,
                    scale: (float)Game1.pixelZoom,
                    scaleChange: 0f,
                    rotation: 0f,
                    rotationChange: 0f)
                {
                    holdLastFrame = true,
                    paused = true,
                    endSound = null
                };

                return animatedSprite;
            }

            // Replace the door sprites with our own.
            var newDoorSprites = new Dictionary<Point, TemporaryAnimatedSprite>();
            foreach (var pair in location.doorSprites)
            {
                // TODO: find a way to determine if they're flipped.
                newDoorSprites.Add(pair.Key, GetDoorAnimation(pair.Key, false));
            }
            location.doorSprites = newDoorSprites;
        }
    }
}
