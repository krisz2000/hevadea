﻿namespace Hevadea.GameObjects.Entities.Blueprints
{
    public class GenericEntityBlueprint<T> : EntityBlueprint where T : Entity, new()
    {
        public GenericEntityBlueprint(string name) : base(name)
        {
        }

        public override Entity Construct()
        {
            var entity = new T();
            entity.Blueprint = this;
            return entity;
        }
    }
}