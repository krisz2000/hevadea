﻿using Hevadea.GameObjects.Entities.Blueprints;
using Hevadea.GameObjects.Tiles;
using Hevadea.Worlds;
using System.Collections.Generic;

namespace Hevadea.WorldGenerator.LevelFeatures
{
    public class Decorator : LevelFeature
    {
        public List<Tile> CanBePlantOn { get; set; } = new List<Tile>();
        public GeneratorFunctions PlacingFunction { get; set; } = Functions.Flat(0.9f);
        public float Threashold { get; set; } = 1f;
        public int Chance { get; set; } = 1;
        public int RandomOffset { get; set; } = 4;
        public int Spacing { get; set; } = 1;

        private float _progress = 0;
        private EntityBlueprint _blueprint;

        public Decorator(EntityBlueprint blueprint)
        {
            _blueprint = blueprint;
        }

        public override float GetProgress()
        {
            return _progress;
        }

        public override void Apply(Generator gen, LevelGenerator levelGen, Level level)
        {
            for (var x = 0; x < gen.Size; x += Spacing)
            {
                for (var y = 0; y < gen.Size; y += Spacing)
                {
                    if (gen.Random.Next(0, Chance) == 0 && CanBePlantOn.Contains(level.GetTile(x, y)) && PlacingFunction(x, y, gen, levelGen, level) < Threashold && level.GetEntitiesAt(x, y).Count == 0)
                        level.AddEntityAt(_blueprint.Construct(), x, y, gen.Random.Next(-RandomOffset, RandomOffset), gen.Random.Next(-RandomOffset, RandomOffset));
                }

                _progress = (x / (float)gen.Size);
            }
        }

        public override string GetName()
        {
            return $"Populating [{_blueprint.Name}]";
        }
    }
}