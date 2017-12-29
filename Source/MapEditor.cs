using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace TotalBathhouseOverhaul
{
    internal class MapEditor
    {
        private Map VanillaMap;
        private Map ModifiedMap;
        private Map TargetMap;
        private GameLocation TargetLocation;

        private IModHelper Helper;
        private IMonitor Monitor;

        private bool Initialized = false;

        public MapEditor(IModHelper helper, IMonitor monitor)
        {
            this.Helper = helper;
            this.Monitor = monitor;
        }

        public bool Init(string locationName, string vanillaMapPath, string modifiedMapPath)
        {
            try
            {
                this.VanillaMap = Helper.Content.Load<Map>(vanillaMapPath);
                this.ModifiedMap = Helper.Content.Load<Map>(modifiedMapPath);
                this.TargetLocation = Game1.getLocationFromName(locationName);
                this.TargetMap = this.TargetLocation.map;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error initializing MapEditor: {ex}");
                return false;
            }
            this.Initialized = true;
            return true;
        }

        public void Patch(string tilsheetRoot, string[] layersToModify)
        {
            Debug.Assert(this.Initialized);

            // Copy map properties
            foreach (var property in this.ModifiedMap.Properties)
            {
                // Only add it if it's not in the vanilla map properties or we changed the value.
                // This way we only set things we actually changed.
                if (!this.VanillaMap.Properties.ContainsKey(property.Key) ||
                    this.VanillaMap.Properties[property.Key] != property.Value)
                {
                    this.TargetMap.Properties[property.Key] = property.Value;
                }
            }

            try
            {
                AddMissingTilesheets(tilsheetRoot);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error adding missing tilesheets to {this.TargetMap?.Id} from root {tilsheetRoot}: {ex}");
                return;
            }

            try
            {
                ApplyChangesToLayers(layersToModify);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error applying changes to layers: {ex}");
                return;
            }
        }

        public void Patch(string tilsheetRoot)
        {
            Debug.Assert(this.Initialized);

            string[] layers = this.TargetMap.Layers
                .Select(l => l.Id)
                .ToArray();
            Patch(tilsheetRoot, layers);
        }

        private void ApplyChangesToLayers(string[] layers)
        {
            foreach (string layer in layers)
            {
                ApplyChanges(layer);
            }
        }

        // Add any tilesheets that the modified map has to the target map.
        private void AddMissingTilesheets(string tilsheetRoot)
        {
            foreach (var sheet in this.ModifiedMap.TileSheets)
            {
                // Check if it has a sheet with the same name.
                bool targetHasSheet = this.TargetMap.TileSheets.Count(s => s.Id == sheet.Id) > 0;
                if (!targetHasSheet)
                {
                    // This only works if the filename is the same as the tilesheet name.
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

        private void ApplyChanges(string layer)
        {
            Layer targetLayer = this.TargetMap.GetLayer(layer);
            Layer modifiedLayer = this.ModifiedMap.GetLayer(layer);
            Layer vanillaLayer = this.VanillaMap.GetLayer(layer);

            // TODO: Maybe support adding new layers if we need it.
            if (targetLayer == null || modifiedLayer == null || vanillaLayer == null)
            {
                this.Monitor.Log($"Couldn't find layer '{layer}' in one of the maps; skipping.");
                return;
            }

            for (int y = 0; y < targetLayer.TileHeight; ++y)
            {
                for (int x = 0; x < targetLayer.TileWidth; ++x)
                {
                    Tile vanillaTile = vanillaLayer.Tiles[x, y];
                    Tile modTile = modifiedLayer.Tiles[x, y];
                    Tile targetTile = targetLayer.Tiles[x, y];

                    // If it was set in the vanilla map but isn't in our map then it was deleted.
                    if (modTile == null)
                    {
                        if (vanillaTile != null)
                        {
                            // delete the tile
                            targetLayer.Tiles[x, y] = null;
                        }
                        continue;
                    }

                    Tile newTile = targetTile;
                    if (vanillaTile == null || AreTileSpritesDifferent(modTile, vanillaTile))
                    {
                        // Find the tilesheet with a matching ID
                        TileSheet tileSheet = GetTilesheetByID(this.TargetMap, modTile.TileSheet.Id);
                        if (tileSheet == null)
                        {
                            this.Monitor.Log($"Failed to find tilesheet {modTile.TileSheet.Id} in target map: {this.TargetMap.Id}");
                            continue;
                        }

                        // If the vanilla tile and mod tile don't have this property it must be from another mod, so keep it.
                        var additionalProperties = targetTile?.Properties
                            .Where(p => (vanillaTile == null || !vanillaTile.Properties.ContainsKey(p.Key)) && !modTile.Properties.ContainsKey(p.Key));

                        newTile = CopyTile(modTile, tileSheet, targetLayer, additionalProperties as IPropertyCollection);
                    }
                    // The tile didn't change but the properties did.
                    else if (vanillaTile == null || AreTilePropertiesDifferent(modTile, vanillaTile))
                    {
                        MergeTileProperties(modTile, newTile);
                    }

                    if (newTile != null)
                    {
                        targetLayer.Tiles[x, y] = newTile;
                    }
                }
            }
        }

        private bool AreTileSpritesDifferent(Tile a, Tile b)
        {
            return a.TileIndex != b.TileIndex || a.TileSheet.Id != b.TileSheet.Id;
        }

        private bool AreTilePropertiesDifferent(Tile a, Tile b)
        {
            return a.Properties != b.Properties;
        }

        // Clones a tile but uses the provided tilesheet and layer.
        private Tile CopyTile(Tile source, TileSheet tileSheet, Layer layer, IPropertyCollection additionalProperties = null)
        {
            var blendMode = source.BlendMode;
            var tileIndex = source.TileIndex;

            Tile newTile = null;
            if (source is AnimatedTile)
            {
                AnimatedTile animatedTile = source as AnimatedTile;
                // Copy the frames
                StaticTile[] frames = new StaticTile[animatedTile.TileFrames.Length];
                for (int i = 0; i < animatedTile.TileFrames.Length; ++i)
                {
                    StaticTile copyFrame = animatedTile.TileFrames[i];
                    frames[i] = new StaticTile(layer, tileSheet, copyFrame.BlendMode, copyFrame.TileIndex);
                }

                newTile = new AnimatedTile(layer, frames, animatedTile.FrameInterval);
            }
            else
            {
                newTile = new StaticTile(layer, tileSheet, blendMode, tileIndex);
            }

            if (additionalProperties?.Count > 0)
            {
                newTile.Properties.CopyFrom(additionalProperties);
            }

            // Do this after adding the additional properties so it overwrites any conflicting ones.
            MergeTileProperties(source, newTile);

            // Copy the tile properties. This contains the data set on tiles on the actual tilesheet (ie. passable).
            newTile.TileIndexProperties.CopyFrom(source.TileIndexProperties);

            return newTile;
        }

        private TileSheet GetTilesheetByID(Map map, string Id)
        {
            return this.TargetMap.TileSheets
                .Where(s => s.Id == Id)
                .First();
        }

        private void MergeTileProperties(Tile from, Tile to, bool replaceConflicting = true)
        {
            // Add new properties from the modified tile, replacing any that already exist.
            foreach (var pair in from.Properties)
            {
                if (!to.Properties.ContainsKey(pair.Key))
                {
                    to.Properties.Add(pair.Key, pair.Value);
                }
                else if (replaceConflicting)
                {
                    to.Properties[pair.Key] = pair.Value; // overwrite
                }
            }
        }
    }
}
