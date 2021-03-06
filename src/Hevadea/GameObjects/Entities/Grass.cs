﻿using Hevadea.Framework;
using Hevadea.Framework.Graphic.SpriteAtlas;
using Hevadea.GameObjects.Entities.Components.Attributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hevadea.GameObjects.Entities
{
    public class Grass : Entity
    {
        public int Variant { get; set; } = 0;
        private static Sprite[] _sprites;

        public Grass()
        {
            SortingOffset = 1;

            Variant = Rise.Rnd.Next(0, 4);

            if (_sprites == null)
            {
                _sprites = new Sprite[4];
                for (int i = 0; i < 4; i++)
                {
                    _sprites[i] = new Sprite(Ressources.TileEntities, new Point(7, i));
                }
            }

            AddComponent(new Breakable());
            AddComponent(new Burnable(1f));
        }

        public override void OnDraw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprites[Variant].Draw(spriteBatch, new Vector2(X - 8, Y - 12), 1f, Color.White);
        }
    }
}