﻿using Maker.Hevadea.Game.Entities.Component;
using Maker.Hevadea.Game.Entities.Component.Render;
using Maker.Hevadea.Game.Items;
using Maker.Rise;
using Maker.Rise.Graphic.Particles;
using Maker.Rise.Ressource;
using Microsoft.Xna.Framework;

namespace Maker.Hevadea.Game.Entities.Creatures
{
    public class PlayerEntity : Entity
    {
        public PlayerEntity()
        {
            Width = 8;
            Height = 8;
            Origin = new Point(4, 4);

            HoldingItem = null;

            Components.Add(new Health(20){ ShowHealthBar = false });
            Components.Add(new Attack());
            Components.Add(new Energy());
            Components.Add(new NpcRender(new Sprite(Ressources.TileCreatures, 0, new Point(16, 32))));
            Components.Add(new Inventory(64) {AlowPickUp = true});
            Components.Add(new Interact());
            Components.Add(new Light {On = true, Color = Color.White * 0.30f, Power = 48});
            Components.Add(new Move());
            Components.Add(new Swim());
        }

        public Item HoldingItem { get; set; }

        public override bool IsBlocking(Entity entity)
        {
            return true;
        }

        public override void OnUpdate(GameTime gameTime)
        {
            Level.ParticleSystem.EmiteAt(new ColoredParticle{ Color = Color.Red}, X, Y, Engine.Random.Next() - 0.5f, Engine.Random.Next() - 0.5f);
        }
    }
}