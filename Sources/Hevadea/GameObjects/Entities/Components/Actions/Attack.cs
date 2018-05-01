﻿using Hevadea.Framework.Graphic.SpriteAtlas;
using Hevadea.Framework.Utils;
using Hevadea.GameObjects.Entities.Components.Attributes;
using Hevadea.GameObjects.Entities.Components.States;
using Hevadea.GameObjects.Items;
using Hevadea.GameObjects.Items.Tags;
using Hevadea.GameObjects.Tiles.Components;
using Hevadea.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Hevadea.GameObjects.Entities.Components.Actions
{
    public class Attack : EntityComponent, IEntityComponentUpdatable, IEntityComponentDrawable
    {
        public bool IsAttacking { get; private set; }
     
        public float BaseDamages { get; set; } = 1f;
        public bool CanAttackTile { get; set; } = true;
        public bool CanAttackEntities { get; set; } = true;
        public double AttackCooldown { get; set; } = 1;
        
        public Point HitBoxSize { get; set; } = new Point(26, 26);
        
        private Direction _lastDirection = Direction.North;
        private double _speedFactor = 1;
        private double _timer;

        private Sprite _swingUP;
        private Sprite _swingDown;
        private Sprite _swingLeft;
        private Sprite _swingRight;

        public Attack()
        {
            _swingUP = new Sprite(Ressources.TileIcons, 5);
            _swingDown = new Sprite(Ressources.TileIcons, 3);
            _swingLeft = new Sprite(Ressources.TileIcons, 6);
            _swingRight = new Sprite(Ressources.TileIcons, 4);
        }
        
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!IsAttacking) return;
            var invTimer = 1f - _timer;
            var HitBox = GetHitbox();

            switch (_lastDirection)
            {
                case Direction.North:
                    _swingUP.Draw(spriteBatch, HitBox, Color.White);
                    break;
                case Direction.East:
                    _swingRight.Draw(spriteBatch, HitBox, Color.White);
                    break;
                case Direction.South:
                    _swingDown.Draw(spriteBatch, HitBox, Color.White);
                    break;
                case Direction.West:
                    _swingLeft.Draw(spriteBatch, HitBox, Color.White);
                    break;
            }
        }

		public Rectangle GetHitbox()
		{
			return new Rectangle(Owner.Position.ToPoint() - new Rectangle(new Point(0), HitBoxSize).GetAnchorPoint(DirectionToAnchore[Owner.Facing]), HitBoxSize);
		}

        public void Update(GameTime gameTime)
        {
            _timer = Math.Max(0.0, _timer - gameTime.ElapsedGameTime.TotalSeconds * 5);

            if (!IsAttacking) _speedFactor = AttackCooldown;

            if (IsAttacking && _timer < 0.1)
            {
                _speedFactor = _speedFactor * 0.9;
                IsAttacking = false;
            }
        }

        public float GetBaseDamages()
        {
            var damages = BaseDamages;

            var energy = Owner.GetComponent<Energy>();

            if (energy != null) damages = damages * (energy.Value / energy.MaxValue);

            return damages;
        }

        private static Dictionary<Direction, Anchor> DirectionToAnchore = new Dictionary<Direction, Anchor>()
        {
            {Direction.North, Anchor.Bottom},
            {Direction.South, Anchor.Top},
            {Direction.West, Anchor.Right},
            {Direction.East, Anchor.Left},
        };
        
        public void Do(Item weapon)
        {
            if (IsAttacking) return;
            if (Owner.GetComponent<Pickup>()?.HasPickedUpEntity() ?? false) return;

            var baseDamages = GetBaseDamages();
            if (!Owner.GetComponent<Energy>()?.Reduce(1f) ?? false) return;
            var facingTile = Owner.GetFacingTile();

            if (CanAttackEntities)
            {
                var facingEntities = Owner.Level.GetEntitiesOnArea(GetHitbox());
                facingEntities.Sort((a, b) =>
                    {
                        return Mathf.Distance(a.X, a.Y, Owner.X, Owner.Y)
                            .CompareTo(Mathf.Distance(b.X, b.Y, Owner.X, Owner.Y));
                    });
                
                foreach (var e in facingEntities)
                    if (e != Owner)
                    {
                        if (e.HasComponent<Breakable>())
                        {
                            e.GetComponent<Breakable>()?.Break(weapon);
                            IsAttacking = true;
                            break;
                        }

                        var eHealth = e.GetComponent<Health>();
                        if (!eHealth?.Invicible ?? false)
                        {
                            var damages = baseDamages * (weapon?.Tag<DamageTag>()?.GetDamages(e) ?? 1f);
                            eHealth.Hurt(Owner, damages , Owner.Facing);
                            
                            IsAttacking = true;
                            break;
                        }
                    }
            }

            if (CanAttackTile && !IsAttacking)
            {
                var tile = Owner.Level.GetTile(facingTile);
                if (tile.HasTag<DamageTile>())
                {
                    tile.Tag<DamageTile>().Hurt(baseDamages * (weapon?.Tag<DamageTag>()?.GetDamages(tile) ?? 1f), facingTile, Owner.Level);   
                }

                if (tile.HasTag<BreakableTile>())
                {
                    tile.Tag<BreakableTile>().Break(facingTile, Owner.Level);
                }

                IsAttacking = true;
            }

            if (IsAttacking)
            {
                if (Owner == Owner.Game.MainPlayer)
                {
                    Owner.Game.Camera.Thrauma += 0.1f;
                }
            }

            _lastDirection = Owner.Facing;
            _timer = _speedFactor;
        }
    }
}