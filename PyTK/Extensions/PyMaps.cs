﻿using System.Collections.Generic;
using StardewValley;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using StardewValley.Locations;
using xTile;
using xTile.Tiles;
using xTile.Layers;
using System.IO;

namespace PyTK.Extensions
{
    public static class PyMaps
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T sObjectOnMap<T>(this Vector2 t) where T : SObject
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, SObject> objects = location.objects;
                if (objects.ContainsKey(t) && (objects[t] is T))
                    return ((T) objects[t]);
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T terrainOnMap<T>(this Vector2 t) where T : TerrainFeature
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, TerrainFeature> terrain = location.terrainFeatures;
                if (terrain.ContainsKey(t) && (terrain[t] is T))
                    return ((T) terrain[t]);
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static T furnitureOnMap<T>(this Vector2 t) where T : Furniture
        {
            if (Game1.currentLocation is DecoratableLocation location)
            {
                List<Furniture> furniture = location.furniture;
                return ((T) furniture.Find(f => f.getBoundingBox(t).Intersects(new Rectangle((int) t.X * Game1.tileSize, (int) t.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize))));
            }
            return null;
        }

        /// <summary>Looks for an object of the requested type on this map position.</summary>
        /// <returns>Return the object or null.</returns>
        public static SObject sObjectOnMap(this Vector2 t)
        {
            if (Game1.currentLocation is GameLocation location)
            {
                Dictionary<Vector2, SObject> objects = location.objects;
                if (objects.ContainsKey(t))
                    return objects[t];
            }
            return null;
        }

        public static bool hasTileSheet(this Map map, TileSheet tilesheet)
        {
            foreach (TileSheet ts in map.TileSheets)
                if (tilesheet.ImageSource.EndsWith(new FileInfo(ts.ImageSource).Name) || tilesheet.Id == ts.Id)
                    return true;

            return false;
        }

        public static GameLocation clearArea(this GameLocation l, Rectangle area)
        {

            for (int x = area.X; x < area.Width; x++)
                for (int y = area.Y; y < area.Height; y++)
                {
                    l.objects.Remove(new Vector2(x, y));
                    l.largeTerrainFeatures.Remove(l.largeTerrainFeatures.Find(p => p.tilePosition == new Vector2(x,y)));
                    l.terrainFeatures.Remove(new Vector2(x, y));
                }

            return l;
        }

        public static Map mergeInto(this Map t, Map map, Vector2 position, Rectangle? sourceArea, bool includeEmpty = false, bool properties = true)
        {
            Rectangle sourceRectangle = sourceArea.HasValue ? sourceArea.Value : new Rectangle(0, 0, t.DisplayWidth / Game1.tileSize, t.DisplayHeight / Game1.tileSize);

            foreach (TileSheet tilesheet in t.TileSheets)
                if (!map.hasTileSheet(tilesheet))
                    map.AddTileSheet(new TileSheet(tilesheet.Id, map, tilesheet.ImageSource, tilesheet.SheetSize, tilesheet.TileSize));

            for (Vector2 _x = new Vector2(sourceRectangle.X, position.X); _x.X < sourceRectangle.Width; _x += new Vector2(1, 1))
            {
                for (Vector2 _y = new Vector2(sourceRectangle.Y, position.Y); _y.X < sourceRectangle.Height; _y += new Vector2(1, 1))
                {
                    foreach (Layer layer in t.Layers)
                    {
                        Tile sourceTile = layer.Tiles[(int)_x.X, (int)_y.X];
                        Layer mapLayer = map.GetLayer(layer.Id);

                        if (mapLayer == null)
                        {
                            map.InsertLayer(new Layer(layer.Id, map, map.Layers[0].LayerSize, map.Layers[0].TileSize), map.Layers.Count);
                            mapLayer = map.GetLayer(layer.Id);
                        }
                        
                        if (sourceTile == null)
                        {
                            if (includeEmpty)
                            {
                                try
                                {
                                    mapLayer.Tiles[(int)_x.Y, (int)_y.Y] = null;
                                }
                                catch { }
                            }
                            continue;
                        }

                        TileSheet tilesheet = map.GetTileSheet(sourceTile.TileSheet.Id);
                        Tile newTile = new StaticTile(mapLayer, tilesheet, BlendMode.Additive, sourceTile.TileIndex);

                        if (sourceTile is AnimatedTile aniTile)
                        {
                            List<StaticTile> staticTiles = new List<StaticTile>();

                            foreach (StaticTile frame in aniTile.TileFrames)
                                staticTiles.Add(new StaticTile(mapLayer, tilesheet, BlendMode.Additive, frame.TileIndex));

                            newTile = new AnimatedTile(mapLayer, staticTiles.ToArray(), aniTile.FrameInterval);
                        }

                        if(properties)
                            foreach (var prop in sourceTile.Properties)
                                newTile.Properties.Add(prop);

                        mapLayer.Tiles[(int)_x.Y, (int)_y.Y] = newTile;
                    }

                }
            }
            return map;
        }

        



    }
}
