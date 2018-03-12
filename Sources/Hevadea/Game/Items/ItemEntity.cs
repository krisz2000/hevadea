﻿using Hevadea.Framework.Utils;
using Hevadea.Game.Entities;
using Hevadea.Game.Entities.Components;
using Hevadea.Game.Entities.Components.Attributes;
using Hevadea.Game.Registry;
using Hevadea.Game.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hevadea.Game.Items
{
    public class ItemEntity : Entity
    {
        public Item Item { get; set; }
        public float SpeedX { get; set; }
        public float SpeedY { get; set; }

        public ItemEntity()
        {
            Item = ITEMS.COAL;

            Attach(new Move());
        }

        public override void OnUpdate(GameTime gameTime)
        {
            var move = Get<Move>();
            move.Do(SpeedX, SpeedY, Facing);
            SpeedX = SpeedX / 2;
            SpeedY = SpeedY / 2;
            
            var entities = Level.GetEntitiesOnArea(new Rectangle((int)X - 8, (int)Y - 8, 16, 16));
            foreach (var e in entities)
            {
                var inv = e.Get<Inventory>();
                if (inv?.AlowPickUp ?? false)
                {
                    move.MoveTo(e.X, e.Y);
                    if (Mathf.Distance(e.X, e.Y, X, Y) < 3 && inv.Pickup(Item)) Remove();
                }
            }

        }

        public override void OnDraw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var s = Item.GetSprite();
            s.Draw(spriteBatch, new Vector2(X + 1 - 3, Y + 1 - 3), 0.50f, Color.Black * 0.10f);
            s.Draw(spriteBatch, new Vector2(X - 3, Y - 3), 0.50f, Color.White);
        }

        public override void OnSave(EntityStorage store)
        {
            store.Set("item", Item.Id);
            store.Set("sx", SpeedX);
            store.Set("sy", SpeedY);
        }

        public override void OnLoad(EntityStorage store)
        {
            Item = ITEMS.ById[store.GetInt("item")];
            SpeedX = store.GetFloat("sx", SpeedX);
            SpeedY = store.GetFloat("sy", SpeedY);
        }
    }
}