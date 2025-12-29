using Entitas;
using System.Collections.Generic;
using System;

namespace LccHotfix
{
    public class GroupEntityIndex<TEntity, TKey> : AbstractEntityIndex<TEntity, TKey> where TEntity : Entity
    {
        private readonly Dictionary<TKey, TEntity> _index;

        public GroupEntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey> getKey) : base(name, group, getKey)
        {
            _index = new Dictionary<TKey, TEntity>();
            Activate();
        }

        public GroupEntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey[]> getKeys) : base(name, group, getKeys)
        {
            _index = new Dictionary<TKey, TEntity>();
            Activate();
        }

        public GroupEntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey> getKey, IEqualityComparer<TKey> comparer) : base(name, group, getKey)
        {
            _index = new Dictionary<TKey, TEntity>(comparer);
            Activate();
        }

        public GroupEntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey[]> getKeys, IEqualityComparer<TKey> comparer) : base(name, group, getKeys)
        {
            _index = new Dictionary<TKey, TEntity>(comparer);
            Activate();
        }

        public override void Activate()
        {
            base.Activate();
            IndexEntities(_group);
        }


        public TEntity GetEntity(TKey key)
        {
            _index.TryGetValue(key, out var value);
            return value;
        }

        public override string ToString() => $"GroupEntityIndex({Name})";

        protected override void Clear()
        {
            foreach (TEntity value in _index.Values)
            {
                SafeAERC safeAERC = value.Aerc as SafeAERC;
                if (safeAERC != null)
                {
                    if (safeAERC.Owners.Contains(this))
                    {
                        value.Release(this);
                    }
                }
                else
                {
                    value.Release(this);
                }
            }

            _index.Clear();
        }

        protected override void AddEntity(TKey key, TEntity entity)
        {
            if (_index.ContainsKey(key))
            {
                throw new EntityIndexException($"Entity for key '{key}' already exists!", "Only one entity for a primary key is allowed.");
            }

            _index.Add(key, entity);
            SafeAERC safeAERC = entity.Aerc as SafeAERC;
            if (safeAERC != null)
            {
                if (!safeAERC.Owners.Contains(this))
                {
                    entity.Retain(this);
                }
            }
            else
            {
                entity.Retain(this);
            }
        }

        protected override void RemoveEntity(TKey key, TEntity entity)
        {
            _index.Remove(key);
            SafeAERC safeAERC = entity.Aerc as SafeAERC;
            if (safeAERC != null)
            {
                if (safeAERC.Owners.Contains(this))
                {
                    entity.Release(this);
                }
            }
            else
            {
                entity.Release(this);
            }
        }
    }
}