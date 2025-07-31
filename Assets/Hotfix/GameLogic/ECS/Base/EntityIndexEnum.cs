using Entitas;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class EntityIndexEnum<TEntity, TKey> : AbstractEntityIndex<TEntity, TKey> where TEntity : Entity where TKey : Enum
    {
        private readonly HashSet<TEntity> _all;
        private readonly List<TKey> _allEnum;
        private readonly Dictionary<TKey, HashSet<TEntity>> _index;

        public EntityIndexEnum(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey> getKey) : base(name, group, getKey)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>();
            _all = new HashSet<TEntity>();
            _allEnum = new List<TKey>();
            Activate();
        }

        public EntityIndexEnum(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey[]> getKeys) : base(name, group, getKeys)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>();
            _all = new HashSet<TEntity>();
            _allEnum = new List<TKey>();
            Activate();
        }

        public EntityIndexEnum(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey> getKey, IEqualityComparer<TKey> comparer) : base(name, group, getKey)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>(comparer);
            _all = new HashSet<TEntity>();
            _allEnum = new List<TKey>();
            Activate();
        }

        public EntityIndexEnum(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey[]> getKeys, IEqualityComparer<TKey> comparer) : base(name, group, getKeys)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>(comparer);
            _all = new HashSet<TEntity>();
            _allEnum = new List<TKey>();
            Activate();
        }

        public override void Activate()
        {
            base.Activate();

            foreach (TKey item in Enum.GetValues(typeof(TKey)))
            {
                _allEnum.Add(item);
            }
            IndexEntities(_group);
        }

        public HashSet<TEntity> GetEntities(TKey key)
        {
            _all.Clear();
            foreach (var item in _allEnum)
            {
                if (key.HasFlag(item))
                {
                    if (_index.TryGetValue(item, out var value))
                    {
                        foreach (var entity in value)
                        {
                            _all.Add(entity);
                        }
                    }

                }
            }
            return _all;
        }

        private void AddEntitie(TKey key, TEntity entity)
        {
            foreach (var item in _allEnum)
            {
                if (key.HasFlag(item))
                {
                    if (!_index.TryGetValue(item, out var value))
                    {
                        value = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Comparer);
                        value.Add(entity);
                        _index.Add(item, value);
                    }
                    else
                    {
                        value.Add(entity);
                    }
                }
            }
        }
        private void RemoveEntitie(TKey key, TEntity entity)
        {
            foreach (var item in _allEnum)
            {
                if (key.HasFlag(item))
                {
                    if (_index.TryGetValue(item, out var value))
                    {
                        value.Remove(entity);
                    }
                }
            }
        }

        public override string ToString() => $"EntityIndexEnum({Name})";

        protected override void Clear()
        {
            foreach (HashSet<TEntity> value in _index.Values)
            {
                foreach (TEntity item in value)
                {
                    SafeAERC safeAERC = item.Aerc as SafeAERC;
                    if (safeAERC != null)
                    {
                        if (safeAERC.Owners.Contains(this))
                        {
                            item.Release(this);
                        }
                    }
                    else
                    {
                        item.Release(this);
                    }
                }
            }

            _index.Clear();
        }

        protected override void AddEntity(TKey key, TEntity entity)
        {
            AddEntitie(key, entity);
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
            RemoveEntitie(key, entity);
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