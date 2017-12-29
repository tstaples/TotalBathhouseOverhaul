using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace TotalBathhouseOverhaul
{
    internal class MapEditor
    {
        private string VanillaMapPath;
        private string ModifiedMapPath;
        private string LocationName;
        private string TilesheetRoot;

        private Map VanillaMap;
        private Map ModifiedMap;
        private Map TargetMap;
        private GameLocation TargetLocation;

        private IModHelper Helper;

        public MapEditor(IModHelper helper)
        {
            this.Helper = helper;
        }

        public void Load(string locationName, string vanillaMapPath, string modifiedMapPath)
        {
            this.VanillaMapPath = vanillaMapPath;
            this.ModifiedMapPath = modifiedMapPath;
            this.LocationName = locationName;

            this.VanillaMap = Helper.Content.Load<Map>(vanillaMapPath);
            this.ModifiedMap = Helper.Content.Load<Map>(modifiedMapPath);
            this.TargetLocation = Game1.getLocationFromName(locationName);
            this.TargetMap = this.TargetLocation.map;
        }

        // Add any tilesheets that the modified map has to the target map.
        public void AddMissingTilesheets(string tilsheetRoot)
        {
            foreach (var sheet in this.ModifiedMap.TileSheets)
            {
                // Check if it has a sheet with the same name.
                bool targetHasSheet = this.TargetMap.TileSheets.Count(s => s.Id == sheet.Id) > 0;
                if (!targetHasSheet)
                {
                    // TODO: have this take a delegate to get the path for the sheet id if needed.
                    string tilesheetPath = $@"{tilsheetRoot}\{sheet.Id}.png";
                    TileSheet ts = new TileSheet(
                       id: sheet.Id, // a unique ID for the tilesheet
                       map: this.TargetMap,
                       imageSource: this.Helper.Content.GetActualAssetKey(tilesheetPath),
                       sheetSize: sheet.SheetSize, // the size of your tilesheet image (number of columns, number of rows).
                       tileSize: sheet.TileSize // should always be 16x16 for maps
                    );

                    this.TargetMap.AddTileSheet(ts);
                }
            }
            this.TargetMap.LoadTileSheets(Game1.mapDisplayDevice);
        }

        public void ApplyChangesToLayers()
        {
            // Apply to all layers
            foreach (Layer layer in this.TargetMap.Layers)
            {
                ApplyChanges(layer.Id);
            }
        }

        public void ApplyChangesToLayers(string[] layers)
        {
            foreach (string layer in layers)
            {
                ApplyChanges(layer);
            }
        }

        private void ApplyChanges(string layer)
        {
            Layer targetLayer = this.TargetMap.GetLayer(layer);
            Layer modifiedLayer = this.ModifiedMap.GetLayer(layer);
            Layer vanillaLayer = this.VanillaMap.GetLayer(layer);

            for (int x = 0; x < targetLayer.TileWidth; ++x)
            {
                for (int y = 0; y < targetLayer.TileHeight; ++y)
                {
                    Tile vanillaTile = vanillaLayer.Tiles[x, y];
                    Tile modTile = modifiedLayer.Tiles[x, y];
                    Tile tile = targetLayer.Tiles[x, y];

                    if (modTile == null && vanillaTile != null)
                    {
                        // delete the tile
                        targetLayer.Tiles[x, y] = null;
                    }
                    else if (modTile != null && (vanillaTile == null || modTile.TileIndex != vanillaTile.TileIndex || modTile.TileSheet.Id != vanillaTile.TileSheet.Id))
                    {
                        var blendMode = modTile.BlendMode;
                        var tileIndex = modTile.TileIndex;
                        // Find the tilesheet with a matching ID
                        TileSheet newSheet = this.TargetMap.TileSheets
                            .Where(s => s.Id == modTile.TileSheet.Id)
                            .First();

                        Tile newTile = new StaticTile(targetLayer, newSheet, blendMode, tileIndex);

                        // WIP animated tile support.
                        //Tile newTile = null;
                        //AnimatedTile animatedModTile = (AnimatedTile)modTile;
                        //if (animatedModTile != null)
                        //{
                        //    StaticTile[] frames = new StaticTile[animatedModTile.TileFrames.Length];
                        //    for (int i = 0; i < animatedModTile.TileFrames.Length; ++i)
                        //    {
                        //        StaticTile copyFrame = animatedModTile.TileFrames[i];
                        //        frames[i] = new StaticTile(copyFrame.Layer, newSheet, copyFrame.BlendMode, copyFrame.TileIndex);
                        //    }
                        //    newTile = new AnimatedTile(targetLayer, frames, animatedModTile.FrameInterval);
                        //}
                        //else
                        //{
                        //    newTile = new StaticTile(targetLayer, newSheet, blendMode, tileIndex);
                        //}

                        // Keep the tiles original properties
                        if (tile != null)
                            newTile.Properties.CopyFrom(tile.Properties);

                        // Add new properties from the modified tile, replacing any that already exist.
                        foreach (var pair in modTile.Properties)
                        {
                            if (!newTile.Properties.ContainsKey(pair.Key))
                                newTile.Properties.Add(pair.Key, pair.Value);
                            else
                                newTile.Properties[pair.Key] = pair.Value; // overwrite
                        }
                        targetLayer.Tiles[x, y] = newTile;
                    }
                }
            }
        }
    }
}
