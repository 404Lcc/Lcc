using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public static class CustomNodeBattleEntityExtensions
    {
        #region 战斗实体获取

        /// <summary>
        /// 根据黑板中的拥有者战斗实体 ID 获取战斗实体。
        /// </summary>
        public static LogicEntity GetFighterEntity(this CustomNode self)
        {
            var world = self.GetLogicWorld();
            if (world == null)
            {
                self.LogError("GetFighterEntity world == null");
                return null;
            }

            //标准先用GetGenInfo获取
            var fighterEntityID = self.GetOwnerFighterEntityID();

            if (fighterEntityID == 0)
            {
                self.LogError("GetFighterEntity fighterEntityID == 0");
                return null;
            }

            var fighterEntity = world.GetEntityWithComID(fighterEntityID);
            if (fighterEntity == null)
            {
                // 能拿到就拿，拿不到业务侧去具体处理
                //self.LogError($"GetFighterEntity fighterEntity == null, fighterEntityID={fighterEntityID}");
                return null;
            }

            return fighterEntity;
        }

        #endregion

        #region 实体查询与创建

        /// <summary>
        /// 获取指定战斗单位配置 ID 对应的所有实体。
        /// </summary>
        public static HashSet<LogicEntity> GetEntitiesWithComBattleUnitTid(this CustomNode self, int unitTid)
        {
            var logicWorld = self.GetLogicWorld();
            return logicWorld.GetEntitiesWithComBattleUnitTid(unitTid);
        }


        /// <summary>
        /// 创建带 Transform 和 Animation 组件的普通逻辑实体。
        /// </summary>
        public static LogicEntity Create_Entity_Transform_Animation_Entity(this CustomNode self, string path, Vector3 pos)
        {
            var world = self.GetLogicWorld();
            if (world == null)
            {
                self.LogError($"CreateSubobjectEntity Spawn logicWorld == null");
                return null;
            }

            var e = world.AddEntity(path);
            e.AddComTransform(pos, Quaternion.identity, Vector3.one);
            e.AddComAnimation(new MainAnimatorCtrl());
            return e;
        }

        /// <summary>
        /// 创建跟随指定拥有者初始位置、并带有移动组件的逻辑实体。
        /// </summary>
        public static LogicEntity Create_OwnerEntity_Transform_Locomotion_Entity(this CustomNode self, LogicEntity ownerEntity, LocomotionBase locomotion, string path)
        {
            var world = self.GetLogicWorld();
            if (world == null)
            {
                self.LogError($"CreateSubobjectEntity Spawn logicWorld == null");
                return null;
            }

            var e = world.AddEntity(path);
            e.AddHolderEntity(ownerEntity.ID);
            e.AddComTransform(ownerEntity.position, Quaternion.identity, Vector3.one);
            e.SetComLocomotion(locomotion);
            return e;
        }

        #endregion

        #region 实体变量查询

        /// <summary>
        /// 通过黑板变量配置获取实体，死亡实体返回 null。
        /// </summary>
        public static LogicEntity GetEntityByVar(this CustomNode self, string varKey)
        {
            var cfg = new EntityVarCfg(varKey);
            var entity = cfg.GetEntity(self, false);
            if (entity.IsDead())
            {
                return null;
            }

            return entity;
        }

        #endregion
    }
}