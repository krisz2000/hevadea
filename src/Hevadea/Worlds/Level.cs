﻿using Hevadea.Framework;
using Hevadea.Framework.Extension;
using Hevadea.Framework.Graphic;
using Hevadea.Framework.Graphic.Particles;
using Hevadea.Framework.Utils;
using Hevadea.GameObjects;
using Hevadea.GameObjects.Entities;
using Hevadea.GameObjects.Entities.Blueprints;
using Hevadea.GameObjects.Entities.Components;
using Hevadea.GameObjects.Entities.Components.States;
using Hevadea.GameObjects.Tiles;
using Hevadea.GameObjects.Tiles.Renderers;
using Hevadea.Registry;
using Hevadea.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hevadea.Worlds
{
    public partial class Level
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public LevelProperties Properties { get; }
        public bool IsInitialized { get; private set; } = false;
        public Chunk[,] Chunks { get; set; }

        public ParticleSystem ParticleSystem { get; }
        public Minimap Minimap { get; set; }

        private GameState _gameState;
        private World _world;

        private static readonly BlendState LightBlend = new BlendState
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.DestinationColor,
            ColorDestinationBlend = Blend.Zero
        };

        public Level(LevelProperties properties, int width, int height)
        {
            Properties = properties;
            Width = width;
            Height = height;
            ParticleSystem = new ParticleSystem();
            Minimap = new Minimap(this);

            Chunks = new Chunk[width / 16, height / 16];

            for (int x = 0; x < width / 16; x++)
            {
                for (int y = 0; y < height / 16; y++)
                {
                    Chunks[x, y] = new Chunk(x, y);
                }
            }
        }

        /* --- Game Loop -----------------------------------------------------*/

        public void Initialize(World world, GameState gameState)
        {
            _world = world;
            _gameState = gameState;
            foreach (var c in Chunks)
            {
                c.Level = this;
                foreach (var e in c.Entities) e.Initialize(this, world, _gameState);
            }

            IsInitialized = true;
        }

        public RenderState Prepare(bool drawHint)
        {
            Coordinates focusedTile = _gameState.Camera.FocusedTile;

            Point dist = new Point((_gameState.Camera.GetWidth() / 2 / Game.Unit) + 1,
                                    _gameState.Camera.GetHeight() / 2 / Game.Unit);

            Point renderBegin = new Point(Math.Max(0, focusedTile.X - dist.X),
                                          Math.Max(0, focusedTile.Y - dist.Y - 1));

            Point renderEnd = new Point(Math.Min(Width, focusedTile.X + dist.X + 1),
                                        Math.Min(Height, focusedTile.Y + dist.Y + 6));

            EntityColection onScreenEntities = new EntityColection();
            EntityColection aliveEntities = new EntityColection();

            for (int x = renderBegin.X; x < renderEnd.X; x++)
            {
                for (int y = renderBegin.Y; y < renderEnd.Y; y++)
                {
                    onScreenEntities.AddRange(GetEntitiesAt(x, y));
                }
            }

            if (drawHint) onScreenEntities.SortForRender();

            // TODO: For now the alives entities is equal to on screen entities,
            // BUT: in the futur this will depend on the field of view of all the player connected on the server.
            aliveEntities.AddRange(onScreenEntities);

            return new RenderState(renderBegin, renderEnd, onScreenEntities, aliveEntities);
        }

        public void Update(GameTime gameTime)
        {
            RenderState renderState = Prepare(false);

            // Update all alive entities.
            renderState.AliveEntities.UpdateAll(gameTime);

            // Do the random update of tiles.
            for (int i = 0; i < Width * Height / 50; i++)
            {
                Coordinates tile = new Coordinates(Rise.Rnd.Next(Width), Rise.Rnd.Next(Height));
                GetTile(tile).Update(tile, GetTileDataAt(tile), this, gameTime);
            }

            ParticleSystem.Update(gameTime);
        }

        public void Draw(LevelSpriteBatchPool spriteBatchPool, GameTime gameTime)
        {
            RenderState renderState = Prepare(true);
            spriteBatchPool.Begin(_gameState.Camera);

            // Draw Tiles.
            for (int x = renderState.RenderBegin.X; x < renderState.RenderEnd.X; x++)
            {
                for (int y = renderState.RenderBegin.Y; y < renderState.RenderEnd.Y; y++)
                {
                    Coordinates tile = new Coordinates(x, y);
                    GetTile(tile).Draw(spriteBatchPool.TileSpriteBatch, tile, GetTileDataAt(tile), this, gameTime);
                }
            }

            ParticleSystem.Draw(spriteBatchPool.TileSpriteBatch, gameTime);

            // Draw Entities, Shadows and lights.
            foreach (var e in renderState.OnScreenEntities)
            {
                // Draw the entity.
                e.Draw(spriteBatchPool.EntitiesSpriteBatch, gameTime);

                // Draw Entity overlay.
                if (Rise.ShowGui)
                {
                    e.DrawOverlay(spriteBatchPool.OverlaySpriteBatch, gameTime);
                    if (Rise.ShowDebugOverlay) spriteBatchPool.OverlaySpriteBatch.PutPixel(e.Position, Color.Magenta);
                }

                // Draw Entity light source.
                LightSource light = e.GetComponent<LightSource>();
                if (light != null && light.IsOn)
                {
                    DrawLight(spriteBatchPool.LightsSpriteBatch, e.X, e.Y, light.Power, light.Color);
                }

                // TODO: Draw Entity shadow.
            }

            FinalizeDraw(spriteBatchPool);
        }

        private void FinalizeDraw(LevelSpriteBatchPool spriteBatchPool)
        {
            // Get the ambiant lightning.
            Color ambiantLight = Properties.AmbiantLight;

            if (Properties.AffectedByDayNightCycle)
            {
                ambiantLight = _world.DayNightCycle.GetAmbiantLight();
            }

            // Get temporary render targets.
            RenderTarget2D worldRenderTarget = Rise.Graphic.RenderTarget[0];
            RenderTarget2D lightRenderTarget = Rise.Graphic.RenderTarget[1];

            // Draw Entities and tiles to their own rendertarget.
            Rise.Graphic.SetRenderTarget(worldRenderTarget);
            spriteBatchPool.TileSpriteBatch.End();
            spriteBatchPool.ShadowsSpriteBatch.End();
            spriteBatchPool.EntitiesSpriteBatch.End();
            spriteBatchPool.OverlaySpriteBatch.End();

            // Draw shadow to their own rendertarget.
            Rise.Graphic.SetRenderTarget(lightRenderTarget);
            Rise.Graphic.Clear(ambiantLight);
            spriteBatchPool.LightsSpriteBatch.End();

            // Now let's draw everything to the screen.
            Rise.Graphic.SetDefaultRenderTarget();

            // Blit the world on screen.
            spriteBatchPool.GenericSpriteBatch.Begin();
            spriteBatchPool.GenericSpriteBatch.Draw(worldRenderTarget, Rise.Graphic.GetBound(), Color.White);
            spriteBatchPool.GenericSpriteBatch.End();

            // Apply lightning.
            spriteBatchPool.GenericSpriteBatch.Begin(SpriteSortMode.Immediate, LightBlend);
            spriteBatchPool.GenericSpriteBatch.Draw(lightRenderTarget, Rise.Graphic.GetBound(), Color.White);
            spriteBatchPool.GenericSpriteBatch.End();
        }

        public static void DrawLight(SpriteBatch spriteBatch, float x, float y, float power, Color color)
        {
            spriteBatch.Draw(Ressources.ImgLight, x - power, y - power, power * 2, power * 2, color);
        }

        /* --- Save & Load -------------------------------------------------- */

        public static Level Load(LevelStorage store)
        {
            return new Level(LEVELS.GetProperties(store.Type), store.Width, store.Height)
            {
                Id = store.Id,
                Name = store.Name,
            };
        }

        public LevelStorage Save()
        {
            return new LevelStorage()
            {
                Id = Id,
                Name = Name,
                Type = Properties.Name,

                Width = Width,
                Height = Height,
            };
        }

        /* --- Chunks ------------------------------------------------------- */

        public Chunk GetChunkAt(Coordinates t) => GetChunkAt(t.X, t.Y);

        public Chunk GetChunkAt(int tx, int ty)
        {
            if (tx < 0 || ty < 0 || tx >= Width || ty >= Height) return null;
            return Chunks[tx / Chunk.CHUNK_SIZE, ty / Chunk.CHUNK_SIZE];
        }

        /* --- Tiles -------------------------------------------------------- */

        public Tile GetTile(Coordinates t) => GetTile(t.X, t.Y);

        public Tile GetTile(int tx, int ty)
        {
            Chunk chunk = GetChunkAt(tx, ty);

            if (chunk != null)
            {
                return chunk.Tiles[tx % Chunk.CHUNK_SIZE, ty % Chunk.CHUNK_SIZE];
            }

            return TILES.VOID;
        }

        public bool SetTile(Coordinates t, Tile tile) => SetTile(t.X, t.Y, tile);

        public bool SetTile(int tx, int ty, Tile tile)
        {
            Chunk chunk = GetChunkAt(tx, ty);

            if (chunk != null)
            {
                chunk.Tiles[tx % Chunk.CHUNK_SIZE, ty % Chunk.CHUNK_SIZE] = tile;

                if (IsInitialized)
                {
                    for (var x = -1; x <= 1; x++)
                        for (var y = -1; y <= 1; y++)
                        {
                            var xx = tx + x;
                            var yy = ty + y;

                            if (xx >= 0 && yy >= 0 && xx < Width && yy < Height)
                                SetTileConnection(xx, yy, null);
                        }
                }

                return true;
            }

            return false;
        }

        public bool IsAll<T>(Rectangle area) where T : TileComponent => IsAll(area, (t) => t.HasTag<T>());

        public bool IsAll(Rectangle area, Tile tile) => IsAll(area, (t) => t == tile);

        public bool IsAll(Rectangle area, Predicate<Tile> predicat)
        {
            var beginX = area.X / Game.Unit;
            var beginY = area.Y / Game.Unit;

            var endX = (area.X + area.Width) / Game.Unit;
            var endY = (area.Y + area.Height) / Game.Unit;

            var result = true;

            for (var x = beginX; x <= endX; x++)
                for (var y = beginY; y <= endY; y++)
                {
                    if (x < 0 || y < 0 || x >= Width || y >= Height) continue;
                    result &= predicat(GetTile(x, y));
                }

            return result;
        }

        /* --- Tile data ---------------------------------------------------- */

        public Dictionary<string, object> GetTileDataAt(Coordinates t) => GetTileDataAt(t.X, t.Y);

        public Dictionary<string, object> GetTileDataAt(int tx, int ty)
        {
            Chunk chunk = GetChunkAt(tx, ty);

            if (chunk != null)
            {
                return chunk.Data[tx % Chunk.CHUNK_SIZE, ty % Chunk.CHUNK_SIZE];
            }

            return null;
        }

        public T GetTileData<T>(Coordinates t, string dataName, T defaultValue) => GetTileData(t.X, t.Y, dataName, defaultValue);

        public T GetTileData<T>(int tx, int ty, string dataName, T defaultValue)
        {
            return (T)GetTileDataAt(tx, ty).GetValueOrDefault(dataName, defaultValue);
        }

        public void SetTileDataAt(Coordinates t, Dictionary<string, object> data) => SetTileDataAt(t.X, t.Y, data);

        public void SetTileDataAt(int tx, int ty, Dictionary<string, object> data)
        {
            Chunk chunk = GetChunkAt(tx, ty);

            if (chunk != null)
            {
                chunk.Data[tx % Chunk.CHUNK_SIZE, ty % Chunk.CHUNK_SIZE] = data;
            }
        }

        internal void SetTileData<T>(Coordinates t, string dataName, T value) => SetTileData(t.X, t.Y, dataName, value);

        public void SetTileData<T>(int tx, int ty, string dataName, T value)
        {
            GetTileDataAt(tx, ty)[dataName] = value;
        }

        public void ClearTileDataAt(Coordinates tilePosition) => ClearTileDataAt(tilePosition.X, tilePosition.Y);

        public void ClearTileDataAt(int tx, int ty)
        {
            GetTileDataAt(tx, ty)?.Clear();
        }

        /* --- Tile Connections --------------------------------------------- */

        public TileConnection GetTileConnection(Coordinates t) => GetTileConnection(t.X, t.Y);

        public TileConnection GetTileConnection(int tx, int ty)
        {
            Chunk chunk = GetChunkAt(tx, ty);

            if (chunk != null)
            {
                return chunk.CachedTileConnection[tx % Chunk.CHUNK_SIZE, ty % Chunk.CHUNK_SIZE];
            }

            return null;
        }

        public void SetTileConnection(Coordinates t, TileConnection tileConnection) => SetTileConnection(t.X, t.Y, tileConnection);

        public void SetTileConnection(int tx, int ty, TileConnection tileConnection)
        {
            Chunk chunk = GetChunkAt(tx, ty);

            if (chunk != null)
            {
                chunk.CachedTileConnection[tx % Chunk.CHUNK_SIZE, ty % Chunk.CHUNK_SIZE] = tileConnection;
            }
        }

        /* --- Entities ----------------------------------------------------- */

        public void AddEntity(Entity e)
        {
            Chunk chunk = GetChunkAt(e.GetTilePosition());

            chunk.AddEntity(e);

            e.Level = this;

            if (IsInitialized)
            {
                e.Initialize(this, _world, _gameState);
            }
        }

        public Entity AddEntityAt(Entity e, Coordinates t)
        {
            return AddEntityAt(e, t.X + 0.5f, t.Y + 0.5f);
        }

        public Entity AddEntityAt(Entity e, float tx, float ty)
        {
            AddEntity(e);
            e.SetPosition(tx * Game.Unit, ty * Game.Unit);
            return e;
        }

        public Entity AddEntityAt(Entity e, int tx, int ty, float offX = 0f, float offY = 0f)
        {
            AddEntity(e);
            e.SetPosition((tx * Game.Unit) + (Game.Unit / 2) + offX,
                          (ty * Game.Unit) + (Game.Unit / 2) + offY);
            return e;
        }

        public Entity AddEntityAt(EntityBlueprint b, int tx, int ty, float offX = 0f, float offY = 0f)
        {
            var entity = b.Construct();
            AddEntityAt(entity, tx, ty, offX, offY);
            return entity;
        }

        public void RemoveEntity(Entity e)
        {
            Chunk chunk = GetChunkAt(e.GetTilePosition());

            chunk.RemoveEntity(e);
        }

        public List<Entity> GetEntitiesAt(Coordinates t) => GetEntitiesAt(t.X, t.Y);

        public List<Entity> GetEntitiesAt(int tx, int ty)
        {
            Chunk chunk = GetChunkAt(tx, ty);

            if (chunk != null)
            {
                return chunk.EntitiesOnTiles[tx % Chunk.CHUNK_SIZE, ty % Chunk.CHUNK_SIZE].ToList();
            }

            return new List<Entity>();
        }

        public List<Entity> GetEntitiesOnArea(Vector2 center, float radius) => GetEntitiesOnArea(center.X, center.Y, radius);

        public List<Entity> GetEntitiesOnArea(float cx, float cy, float radius)
        {
            var entities = GetEntitiesOnArea(new RectangleF(cx - radius, cy - radius, radius * 2, radius * 2));
            return entities.Where(e => Mathf.Distance(e.X, e.Y, cx, cy) <= radius).ToList();
        }

        public List<Entity> GetEntitiesOnArea(Rectangle area) => GetEntitiesOnArea(new RectangleF(area.X, area.Y, area.Width, area.Height));

        public List<Entity> GetEntitiesOnArea(RectangleF area)
        {
            var result = new List<Entity>();

            var beginX = (area.X / Game.Unit) - 1;
            var beginY = (area.Y / Game.Unit) - 1;

            var endX = ((area.X + area.Width) / Game.Unit) + 1;
            var endY = ((area.Y + area.Height) / Game.Unit) + 1;

            for (int x = (int)beginX; x < endX; x++)
                for (int y = (int)beginY; y < endY; y++)
                {
                    if (x < 0 || y < 0 || x >= Width || y >= Height) continue;
                    var entities = GetEntitiesAt(x, y);

                    result.AddRange(entities.Where(i => i.GetComponent<Colider>()?.GetHitBox().IntersectsWith(area) ?? area.Contains(i.Position)));
                }

            return result;
        }
    }
}