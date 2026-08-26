using System.Runtime.CompilerServices;
using HotUpdate.Framework.PbCfg;
using PBConfig;
using UnityEngine;

namespace LccHotfix
{
    public static class CustomNodeContextExtensions
    {
    #region 世界与上下文获取


        /// <summary>
        /// 获取当前逻辑节点黑板中的 LogicWorld。
        /// </summary>
        public static LogicWorld GetLogicWorld(this CustomNode self)
        {
            //标准先用GetGenInfo获取
            var genInfo = self.GetGenInfo<IHasLogicWorld>(false);
            if (genInfo != null)
            {
                return genInfo.LogicWorld;
            }
            //老代码的保底适配，从黑板固定Key取
            var key = CvKey.CV_LogicWorld;
            var world = self.GetVar<LogicWorld>(key);
            if (world == null)
            {
                self.LogError("GetLogicWorld world == null");
            }
            return world;
        }

        /// <summary>
        /// 获取当前逻辑节点黑板中的 MetaWorld。
        /// </summary>
        public static MetaWorld GetMetaWorld(this CustomNode self)
        {
            //标准先用GetGenInfo获取
            var genInfo = self.GetGenInfo<IHasMetaWorld>(false);
            if (genInfo != null)
            {
                return genInfo.MetaWorld;
            }
            //老代码的保底适配，从黑板固定Key取
            var metaWorld = self.GetVar<MetaWorld>(CvKey.CV_MetaWorld);
            if (metaWorld == null)
            {
                self.LogError("GetMetaWorld metaWorld == null");
            }
            return metaWorld;
        }

        /// <summary>
        /// 判断当前逻辑世界是否已经结束战斗。
        /// </summary>
        public static bool GameOver(this CustomNode self)
        {
            var logicWorld = self.GetLogicWorld();
            return logicWorld.GameOver;
        }

        /// <summary>
        /// 获取当前逻辑节点黑板中记录的拥有者实体。
        /// </summary>
        public static LogicEntity GetOwnerEntity(this CustomNode self)
        {
            //标准先用GetGenInfo获取
            var genInfo = self.GetGenInfo<IHasOwnerEntity>(false);
            if (genInfo != null)
            {
                return genInfo.OwnerEntity;
            }
            //老代码的保底适配，从黑板固定Key取
            var owner = self.GetVar<LogicEntity>(CvKey.CV_OwnerEntity);
            if (owner == null)
            {
                self.LogError("GetOwnerEntity owner == null");
            }
            return owner;
        }

        /// <summary>
        /// 获取当前逻辑节点的拥有者战斗实体 ID。
        /// </summary>
        public static long GetOwnerFighterEntityID(this CustomNode self)
        {
            //标准先用GetGenInfo获取
            var genInfo = self.GetGenInfo<IHasOwnerFighterEntityID>(false);
            if (genInfo != null)
            {
                return genInfo.OwnerFighterEntityID;
            }
            //老代码的保底适配，从黑板固定Key取
            return self.GetVar<long>(CvKey.CV_OwnerFighterEntityID);
        }

        /// <summary>
        /// 获取当前逻辑节点黑板中的拥有者玩家信息。
        /// </summary>
        public static IBattlePlayerInfo GetOwnerBattlePlayerInfo(this CustomNode self)
        {
            //标准先用GetGenInfo获取
            var genInfo = self.GetGenInfo<IHasOwnerPlayerInfo>(false);
            if (genInfo != null)
            {
                return genInfo.OwnerPlayerInfo;
            }
            //老代码的保底适配，从黑板固定Key取
            var playerInfo = self.GetVar<IBattlePlayerInfo>(CvKey.CV_OwnerPlayerInfo);
            if (playerInfo == null)
            {
                self.LogError("node.GetOwnerBattlePlayerInfo == null");
            }
            return playerInfo;
        }


        /// <summary>
        /// 获取当前逻辑节点黑板中的拥有者玩家信息。
        /// </summary>
        public static InGamePlayerInfo GetOwnerPlayerInfo(this CustomNode self)
        {
            var playerInfo = self.GetVar<InGamePlayerInfo>(CvKey.CV_OwnerPlayerInfo);
            if (playerInfo == null)
            {
                CLHelper.LogError(self, "node.GetOwnerPlayerInfo == null");
            }

            return playerInfo;
        }


        /// <summary>
        /// 读取当前逻辑上的补给卡 TID；GenInfo 优先，否则读黑板。
        /// </summary>
        public static uint TryGetBattleSupplyId(this CustomNode self)
        {
            if (self?.GenInfo is SupplyLogicGenInfo supplyGenInfo)
                return supplyGenInfo.BattleSupplyId;
            if (self?.GenInfo is SubobjectGenInfo subobjectGenInfo)
                return subobjectGenInfo.BattleSupplyId;
            return self?.GetVar<uint>(CvKey.CV_BattleSupplyId, 0u) ?? 0u;
        }

    #endregion
    }
}
