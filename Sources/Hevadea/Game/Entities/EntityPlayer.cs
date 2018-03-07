﻿using System;
using Hevadea.Framework.Graphic.SpriteAtlas;
using Hevadea.Game.Entities.Components;
using Hevadea.Game.Entities.Components.Ai;
using Hevadea.Game.Entities.Components.Attributes;
using Hevadea.Game.Entities.Components.Interaction;
using Hevadea.Game.Entities.Components.Render;
using Hevadea.Game.Items;
using Hevadea.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hevadea.Game.Entities
{
    public class EntityPlayer : Entity
    {
        public EntityPlayer()
        {
            Width = 8;
            Height = 8;
            Origin = new Point(4, 4);

            HoldingItem = null;

            Add(new Health(20){ ShowHealthBar = false });
            Add(new Attack());
            Add(new Energy());
            Add(new NpcRender(new Sprite(Ressources.TileCreatures, 0, new Point(16, 32))));
            Add(new Inventory(64) {AlowPickUp = true});
            Add(new Interact());
            Add(new Light {On = true, Color = Color.White * 0.30f, Power = 48});
            Add(new Move());
            Add(new Swim());
            Add(new Pushable());
        }

        public Item HoldingItem { get; set; }

        public override bool IsBlocking(Entity entity)
        {
            return !(entity is ItemEntity);
        }

        public override void OnUpdate(GameTime gameTime)
        {
        }

        public override void OnDraw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var start = GetTilePosition();
            var end = new TilePosition(Level.Width / 2, Level.Height / 2);
            
            var pathfinder = new PathFinder(Level);
            var path =  pathfinder.PathFinding(start, end);
            
            if (path != null)
                PathFinder.DrawPath(spriteBatch, path, Color.Blue);
        }
    }
}