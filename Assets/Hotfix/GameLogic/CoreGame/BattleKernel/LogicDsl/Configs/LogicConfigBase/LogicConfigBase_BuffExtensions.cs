using System.Runtime.CompilerServices;

namespace LccHotfix
{
    public partial class LogicConfigBase
    {
        /// <summary>
        /// 给节点所属战斗单位添加指定逻辑 ID 的 Buff。
        /// </summary>
        public DelegateBhvCfg AddBuff(int buffLogicID, int maxLvl)
        {
            return new DelegateBhvCfg(node =>
            {
                var ownerEntity = node.GetOwnerEntity();
                if (ownerEntity == null)
                {
                    return;
                }
                var genInfo = node.CreateBuffGenInfoFromUnit(ownerEntity, buffLogicID, maxLvl);
                genInfo.PreEnv.WriteVar(CvKey.CV_MetaWorld, node.GetMetaWorld());
                ownerEntity.AddBuff(genInfo);
            });
        }

        /// <summary>
        /// 驱散 Buff。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RemoveBuffBhvCfg RemoveBuffByBuffTag(params int[] buffTag)
        {
            return RemoveBuffBhvCfg.ByBuffTag(buffTag);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RemoveBuffBhvCfg RemoveBuffByLogicID(int logicID)
        {
            return RemoveBuffBhvCfg.ByLogicID(logicID);
        }

        /// <summary>
        /// 创建 Buff 升级或刷新时执行的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffStateChangedBhvCfg Buff_HandleBuffStateChanged(NodeParamAction action)
        {
            return new HandleBuffStateChangedBhvCfg(action);
        }

        /// <summary>
        /// 创建 Buff 添加到实体时执行的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffAddBhvCfg Buff_HandleBuffAdd(NodeParamAction action)
        {
            return new HandleBuffAddBhvCfg(action);
        }

        /// <summary>
        /// 创建 Buff 从实体移除时执行的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffRemoveBhvCfg Buff_HandleBuffRemove(NodeParamAction action)
        {
            return new HandleBuffRemoveBhvCfg(action);
        }

        /// <summary>
        /// 创建 Buff 在伤害结算前修改伤害上下文或结果的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffBeforeDmgBhvCfg Buff_HandleBeforeDmg(BuffHandleBeforeDmgAction action)
        {
            return new HandleBuffBeforeDmgBhvCfg(action);
        }

        /// <summary>
        /// 创建 Buff 监听并处理指定实体命令的回调节点。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HandleBuffEntityCmdBhvCfg Buff_HandleEntityCmd(int cmd, NodeParamEntityCmdAction action)
        {
            return new HandleBuffEntityCmdBhvCfg(cmd, action);
        }
    }
}
