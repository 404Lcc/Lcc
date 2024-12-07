﻿using System;
using System.Collections.Generic;

namespace Entitas
{
    public class EntityIndex<TEntity, TKey> : AbstractEntityIndex<TEntity, TKey> where TEntity : Entity
    {
        readonly Dictionary<TKey, HashSet<TEntity>> _index;

        public EntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey> getKey) : base(name, group, getKey)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>();
            Activate();
        }

        public EntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey[]> getKeys) : base(name, group, getKeys)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>();
            Activate();
        }

        public EntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey> getKey, IEqualityComparer<TKey> comparer) : base(name, group, getKey)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>(comparer);
            Activate();
        }

        public EntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey[]> getKeys, IEqualityComparer<TKey> comparer) : base(name, group, getKeys)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>(comparer);
            Activate();
        }

        public override void Activate()
        {
            base.Activate();
            IndexEntities(_group);
        }

        public HashSet<TEntity> GetEntities(TKey key)
        {
            if (!_index.TryGetValue(key, out var entities))
            {
                entities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Comparer);
                _index.Add(key, entities);
            }

            return entities;
        }

        public override string ToString() => $"EntityIndex({Name})";

        protected override void Clear()
        {
            foreach (var entities in _index.Values)
            {
                foreach (var entity in entities)
                {
                    if (entity.Aerc is SafeAERC safeAerc)
                    {
                        if (safeAerc.Owners.Contains(this))
                            entity.Release(this);
                    }
                    else
                    {
                        entity.Release(this);
                    }
                }
            }

            _index.Clear();
        }

        protected override void AddEntity(TKey key, TEntity entity)
        {
            GetEntities(key).Add(entity);

            if (entity.Aerc is SafeAERC safeAerc)
            {
                if (!safeAerc.Owners.Contains(this))
                    entity.Retain(this);
            }
            else
            {
                entity.Retain(this);
            }
        }

        protected override void RemoveEntity(TKey key, TEntity entity)
        {
            GetEntities(key).Remove(entity);

            if (entity.Aerc is SafeAERC safeAerc)
            {
                if (safeAerc.Owners.Contains(this))
                    entity.Release(this);
            }
            else
            {
                entity.Release(this);
            }
        }
    }
}
