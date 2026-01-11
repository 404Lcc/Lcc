using Entitas;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class EntityIndexEnum<TEntity, TKey> : AbstractEntityIndex<TEntity, TKey> where TEntity : class, IEntity where TKey : Enum
    {
        readonly Dictionary<TKey, HashSet<TEntity>> _index;
        readonly List<TKey> _allEnum;
        readonly HashSet<TEntity> _all;

        public EntityIndexEnum(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey> getKey) : base(name, group, getKey)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>();
            _allEnum = new List<TKey>();
            _all = new HashSet<TEntity>();
            Activate();
        }

        public EntityIndexEnum(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey[]> getKeys) : base(name, group, getKeys)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>();
            _allEnum = new List<TKey>();
            _all = new HashSet<TEntity>();
            Activate();
        }

        public EntityIndexEnum(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey> getKey, IEqualityComparer<TKey> comparer) : base(name, group, getKey)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>(comparer);
            _allEnum = new List<TKey>();
            _all = new HashSet<TEntity>();
            Activate();
        }

        public EntityIndexEnum(string name, IGroup<TEntity> group, Func<TEntity, IComponent, TKey[]> getKeys, IEqualityComparer<TKey> comparer) : base(name, group, getKeys)
        {
            _index = new Dictionary<TKey, HashSet<TEntity>>(comparer);
            _allEnum = new List<TKey>();
            _all = new HashSet<TEntity>();
            Activate();
        }

        public override void Activate()
        {
            base.Activate();

            foreach (TKey item in Enum.GetValues(typeof(TKey)))
            {
                _allEnum.Add(item);
            }
            indexEntities(_group);
        }

        private void Add(TKey key, TEntity entity)
        {
            foreach (var item in _allEnum)
            {
                if (key.HasFlag(item))
                {
                    if (!_index.TryGetValue(item, out var value))
                    {
                        value = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.comparer);
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
        private void Remove(TKey key, TEntity entity)
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

        public override string ToString()
        {
            return "EntityIndexEnum(" + base.name + ")";
        }

        protected override void clear()
        {
            foreach (var entities in _index.Values)
            {
                foreach (TEntity item in entities)
                {
                    SafeAERC safeAERC = item.aerc as SafeAERC;
                    if (safeAERC != null)
                    {
                        if (safeAERC.owners.Contains(this))
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

        protected override void addEntity(TKey key, TEntity entity)
        {
            Add(key, entity);
            SafeAERC safeAERC = entity.aerc as SafeAERC;
            if (safeAERC != null)
            {
                if (!safeAERC.owners.Contains(this))
                {
                    entity.Retain(this);
                }
            }
            else
            {
                entity.Retain(this);
            }
        }

        protected override void removeEntity(TKey key, TEntity entity)
        {
            Remove(key, entity);
            SafeAERC safeAERC = entity.aerc as SafeAERC;
            if (safeAERC != null)
            {
                if (safeAERC.owners.Contains(this))
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

