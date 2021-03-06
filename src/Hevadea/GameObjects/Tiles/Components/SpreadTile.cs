﻿using Hevadea.Framework;
using Hevadea.Utils;
using Hevadea.Worlds;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Hevadea.GameObjects.Tiles.Components
{
    public class Spread : TileComponent, IUpdatableTileComponent
    {
        public List<Tile> SpreadTo { get; set; } = new List<Tile>();
        public int SpreadChance { get; set; } = 10;

        public void Update(Tile tile, Coordinates position, Dictionary<string, object> data, Level level, GameTime gameTime)
        {
            if (Rise.Rnd.Next(SpreadChance) == 0)
            {
                var d = (Direction)Rise.Rnd.Next(0, 4);
                var p = d.ToPoint();

                if (SpreadTo.Contains(level.GetTile(position.X + p.X, position.Y + p.Y))) level.SetTile(position.X + p.X, position.Y + p.Y, AttachedTile);
            }
        }
    }
}