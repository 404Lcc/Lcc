using System.Runtime.CompilerServices;
using UnityEngine;

namespace LccHotfix
{
    public partial class LogicConfigBase
    {
        /// <summary>
        /// 对目标实体变量指定的目标派发伤害事件，实际结算由伤害系统处理。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg MakeDamageTo(string targetIDVar, bool logError = false)
        {
            return new DelegateBhvCfg(node =>
            {
                var entityCfg = new EntityVarCfg(targetIDVar);
                var target = entityCfg.GetEntity(node, logError);
                if (target != null)
                {
                    node.GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.DamageEventService?.DispatchDamage(new EvtDamage(node.RootLogic, target));
                }
            });
        }
    }
}
