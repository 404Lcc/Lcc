using Entitas;
using System.Collections.Generic;

namespace LccHotfix
{
    public static class WorldExtension
    {
        public static LogicEntity GetEntityWithComID(this ECSWorld world, long id)
        {
            return world.GetEntityIndex<ComID, PrimaryEntityIndex<LogicEntity, long>>().GetEntity(id);
        }

        public static HashSet<LogicEntity> GetEntitiesWithComTag(this ECSWorld world, TagType tag)
        {
            return world.GetEntityIndex<ComTag, EntityIndexEnum<LogicEntity, TagType>>().GetEntities(tag);
        }

        public static HashSet<LogicEntity> GetEntitiesWithComFaction(this ECSWorld world, FactionType faction)
        {
            return world.GetEntityIndex<ComFaction, EntityIndex<LogicEntity, FactionType>>().GetEntities(faction);
        }

        public static HashSet<LogicEntity> GetEntitiesWithComOwnerEntity(this ECSWorld world, int ownerEntityID)
        {
            return world.GetEntityIndex<ComOwnerEntity, EntityIndex<LogicEntity, int>>().GetEntities(ownerEntityID);
        }

        public static LogicEntity GetEntitiesWithComUnityObjectRelated(this ECSWorld world, int gameObjectInstanceID)
        {
            return world.GetEntityIndex<ComUnityObjectRelated, GroupEntityIndex<LogicEntity, int>>().GetEntity(gameObjectInstanceID);
        }

        public static void AddBattle(this ECSWorld world)
        {
            LogicSystems System = world.System;

            //控制
            System.Add(new SysControl(world));

            //技能
            System.Add(new SysSkillCD(world));
            System.Add(new SysSkillProcess(world));

            //buff
            System.Add(new SysBuffs(world));

            //子物体
            System.Add(new SysSubobject(world));

            //状态机
            System.Add(new SysFSM(world));

            //移动
            System.Add(new SysLocomotion(world));

            //碰撞
            System.Add(new SysCollision(world));
            System.Add(new SysAABBCollision(world));
            
            //寻路
            System.Add(new SysAStar(world));
            
            //避障
            System.Add(new SysOrca(world));
            System.Add(new SysOrcaAgent(world));

            //游戏模式
            System.Add(new SysGameMode(world));
        }

        public static void AddRender(this ECSWorld world)
        {
            LogicSystems System = world.System;
            //显示
            System.Add(new SysSyncViewTransform(world));
            System.Add(new SysViewUpdate(world));

            System.Add(new SysCameraBlender(world));
        }


        public static void AddDeath(this ECSWorld world)
        {
            LogicSystems System = world.System;
            //生命周期
            System.Add(new SysLife(world));
            System.Add(new SysDeathProcess(world));
        }
    }
}