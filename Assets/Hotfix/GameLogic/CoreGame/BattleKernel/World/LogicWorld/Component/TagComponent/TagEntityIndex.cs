using Entitas;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 按 Tag 位掩码建立多 key 索引：getKey 返回完整 uint 掩码，在 add/remove 内拆位，避免 getKeys 数组分配。
    /// </summary>
    public class TagEntityIndex<TEntity> : AbstractEntityIndex<TEntity, int> where TEntity : class, IEntity
    {
        readonly Dictionary<int, HashSet<TEntity>> _index;

        public TagEntityIndex(string name, IGroup<TEntity> group, Func<TEntity, IComponent, uint> getTags)
            : base(name, group, (entity, component) => unchecked((int)getTags(entity, component)))
        {
            _index = new Dictionary<int, HashSet<TEntity>>();
            Activate();
        }

        public override void Activate()
        {
            base.Activate();
            indexEntities(_group);
        }

        public HashSet<TEntity> GetEntities(int tagKey)
        {
            if (!_index.TryGetValue(tagKey, out var entities))
            {
                entities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.comparer);
                _index.Add(tagKey, entities);
            }

            return entities;
        }

        public override string ToString()
        {
            return "TagEntityIndex(" + name + ")";
        }

        protected override void clear()
        {
            foreach (var entities in _index.Values)
            {
                foreach (var entity in entities)
                {
                    var safeAerc = entity.aerc as SafeAERC;
                    if (safeAerc != null)
                    {
                        if (safeAerc.owners.Contains(this))
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

            _index.Clear();
        }

        protected override void addEntity(int key, TEntity entity)
        {
            ForEachTagKey(unchecked((uint)key), tagKey => AddEntityForKey(tagKey, entity));
        }

        protected override void removeEntity(int key, TEntity entity)
        {
            ForEachTagKey(unchecked((uint)key), tagKey => RemoveEntityForKey(tagKey, entity));
        }

        static void ForEachTagKey(uint tags, Action<int> visit)
        {
            while (tags != 0)
            {
                var flag = tags & (uint)-(int)tags;
                visit((int)flag);
                tags &= ~flag;
            }
        }

        void AddEntityForKey(int tagKey, TEntity entity)
        {
            GetEntities(tagKey).Add(entity);
            RetainEntity(entity);
        }

        void RemoveEntityForKey(int tagKey, TEntity entity)
        {
            GetEntities(tagKey).Remove(entity);
            ReleaseEntity(entity);
        }

        void RetainEntity(TEntity entity)
        {
            var safeAerc = entity.aerc as SafeAERC;
            if (safeAerc != null)
            {
                if (!safeAerc.owners.Contains(this))
                {
                    entity.Retain(this);
                }
            }
            else
            {
                entity.Retain(this);
            }
        }

        void ReleaseEntity(TEntity entity)
        {
            var safeAerc = entity.aerc as SafeAERC;
            if (safeAerc != null)
            {
                if (safeAerc.owners.Contains(this))
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
