﻿using System.Collections.Generic;
using Maker.Hevadea.Game;
using Maker.Hevadea.Game.Entities;
using Maker.Hevadea.Game.Registry;
using Maker.Hevadea.Game.Tiles;

namespace Maker.Hevadea.WorldGenerator.WorldFeatures
{
    public class StairCaseFeature : WorldFeature
    {
        public override string GetName() => "staires";
        private float _progress = 0f;
        
        private LevelGenerator _from;
        private LevelGenerator _to;
        
        public List<Tile> CanbegeneratedOn { get; set; } = new List<Tile>();
        public int TryCount { get; set; } = 300; 
        
        public StairCaseFeature(LevelGenerator from, LevelGenerator to)
        {
            _from = from;
            _to = to;
        }
        
        public override float GetProgress()
        {
            return _progress;
        }

        public override void Apply(Generator gen, World world)
        {
            var from = world.GetLevel(_from.LevelName);
            var to = world.GetLevel(_to.LevelName);
            
            for (var i = 0; i < TryCount; i++)
            {
                var x = gen.Random.Next(0, gen.Size);
                var y = gen.Random.Next(0, gen.Size);

                if (from.IsValid(x, y, 5, 5, true, new List<Tile> {TILES.ROCK}) 
                   && to.IsValid(x, y, 5, 5, true, new List<Tile> {TILES.ROCK, TILES.DIRT}))
                {
                    from.FillRectangle(x + 1, y + 1, 3, 3, TILES.DIRT);
                    to.FillRectangle(x + 1, y + 1, 3, 3, TILES.DIRT);
                    
                    from.SpawnEntity(new StairsEntity(false, to.Id), x + 2, y + 2);
                    to.SpawnEntity(new StairsEntity(true, from.Id), x + 2, y + 2);
                }
            }
        }
    }
}