using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class SysCollision : IExecuteSystem
    {
        private IGroup<LogicEntity> enemyGroup;
        private IGroup<LogicEntity> friendGroup;
        private readonly AQuadSpace root;
        private readonly CollectResult result;

        public SysCollision(ECWorlds world)
        {
            enemyGroup = world.LogicWorld.GetLogicGroup_Bounds_NoneSubobject();
            friendGroup = world.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComCollider, LogicComponentsLookup.ComTransform));
            root = SpatialPartition.Create(new AABB(Vector2.zero, 15), 3);
            result = new CollectResult();
        }

        public void Execute()
        {
            var dt = Time.deltaTime;
            var enemies = enemyGroup.GetEntities();
            var friends = GetEntities(dt);
            FilterHit(friends, enemies, dt);
            //FilterHitOrgion(friends, enemies, dt);
            Handle(friends, dt);
        }

        //测试通过后会重构
        private void FilterHit(IEnumerable<LogicEntity> friends, LogicEntity[] enemies, float dt)
        {
            root.Clear();
            result.result.Clear();
            root.AddObjects(ToSpace(friends, 0));
            root.AddObjects(ToSpace(enemies, 1));
            root.Collect(result);
            foreach (var item in result.result)
            {
                LogicEntity friend, enemy;
                FindInfo(item, out friend, out enemy);
                var maker = friend.comCollider.handler.RawHitMaker;
                if (maker is HitMaker hm)
                {
                    if (hm.IsFull())
                    {
                        continue;
                    }

                    if (hm.IsHit(friend, dt, enemy, out var hit))
                    {
                        hm.Add(hit);
                    }
                }
            }
        }

        private void FindInfo(CollectPair item, out LogicEntity friend, out LogicEntity enemy)
        {
            friend = item.obj0 as LogicEntity;
            enemy = item.obj1 as LogicEntity;
            if (enemy.hasComCollider)
            {
                var tem = friend;
                friend = enemy;
                enemy = tem;
            }
        }

        private void Handle(IEnumerable<LogicEntity> friends, float dt)
        {
            foreach (var entity in friends)
            {
                var handler = entity.comCollider.handler;
                var maker = handler.RawHitMaker;
                bool hasHits = CheckRawHits(dt, entity, maker);
                if (hasHits)
                {
                    handler.HandleRawHits(entity, handler.RawHitMaker.RawHits, dt);
                }

                handler.Cleanup();
            }
        }

        private void FilterHitOrgion(IEnumerable<LogicEntity> friends, LogicEntity[] entities, float dt)
        {
            foreach (var friend in friends)
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var enemy = entities[i];
                    if (enemy == friend)
                        continue;

                    var maker = friend.comCollider.handler.RawHitMaker;
                    if (maker is HitMaker hm)
                    {
                        if (hm.IsFull())
                        {
                            break;
                        }

                        if (hm.IsHit(friend, dt, enemy, out var hit))
                        {
                            hm.Add(hit);
                        }
                    }
                }
            }
        }

        private IEnumerable<SpaceObject> ToSpace(IEnumerable<LogicEntity> v, int layer)
        {
            foreach (var item in v)
            {
                var aabb = item.comBounds.GetBounds();
                yield return new SpaceObject(aabb, item, layer);
            }
        }

        private IEnumerable<LogicEntity> GetEntities(float dt)
        {
            foreach (var entity in friendGroup.GetEntities())
            {
                if (CanCollision(entity, dt))
                    yield return entity;
            }
        }

        private bool CheckRawHits(float dt, LogicEntity ownerEntity, IRawHitMaker maker)
        {
            if (maker is RaycastHitAABBMaker ray1)
            {
                Vector3 dir1 = ownerEntity.comTransform.rotation * Vector3.right * ray1.Distance * dt / 0.016f;
                ray1.SortRawHits(dir1);
            }

            if (maker is HitMaker hm1)
            {
                return hm1.ContainsHit();
            }

            return false;
        }

        private bool CanCollision(LogicEntity entity, float dt)
        {
            if (entity == null)
                return false;
            if (entity.hasComAttributes)
            {
                return entity.comAttributes.GetValue<bool>(AttributeBool.CanCollision);
            }

            var handler = entity.comCollider.handler;
            if (handler.GetType() == typeof(SubobjectColliderHandlerBase))
            {
                var ch = handler as SubobjectColliderHandlerBase;
                //主动碰撞间隔
                if (ch._hitIntervalTimer > 0)
                {
                    ch._hitIntervalTimer -= dt;
                }

                if (!ch.IsActiveAsSource(entity))
                    return false;
            }

            return true;
        }
    }
}