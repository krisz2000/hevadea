﻿using Hevadea.Framework;
using Hevadea.Framework.Utils;
using Hevadea.Game.Entities.Components.Ai.Actions;
using Hevadea.Game.Entities.Components.Attributes;
using Hevadea.Game.Registry;
using Hevadea.Game.Tiles;
using Hevadea.Game.Worlds;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Hevadea.Game.Entities.Components.Ai.Behaviors
{
    public class BehaviorAnimal : IBehavior
    {
        public List<Tile> NaturalEnvironment { get; set; } = new List<Tile>();
        public float MoveSpeed { get; set; } = 1f;
        public double IdleChance { get; set; } = 0.5f;
        public double IdleTime { get; set; } = 1.0;

        public void Update(Agent agent, GameTime gameTime)
        {
            if (!agent.IsBusy())
            {
                if (Rise.Random.NextDouble() < IdleChance)
                {
                    agent.ActionQueue.Enqueue(new ActionWait(Rise.Random.NextDouble() * IdleTime));
                }
                else
                {
                    var dx = Rise.Random.NextValue(-1, 0, 1);
                    var dy = Rise.Random.NextValue(-1, 0, 1);

                    if (dx == 0)
                    {
                        Rise.Random.NextValue(-1, 1);
                    }

                    var entityTilePosition = agent.AttachedEntity.GetTilePosition();
                    var destination = new TilePosition(entityTilePosition.X + dx, entityTilePosition.Y + dy);

                    if ((dx != 0 || dy !=0) && NaturalEnvironment.Contains(agent.AttachedEntity.Level.GetTile(destination)))
                    {
                        agent.ActionQueue.Enqueue(new ActionMoveToLocation(destination, MoveSpeed));
                    }
                }
            }
        }
        
        public void IaAborted(Agent agent, AgentAbortReason why)
        {
          
        }

        public void IaFinish(Agent agent)
        {
            
        }
    }
}