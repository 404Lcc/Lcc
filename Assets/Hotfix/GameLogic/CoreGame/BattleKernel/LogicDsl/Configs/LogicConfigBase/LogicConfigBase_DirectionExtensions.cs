using System.Runtime.CompilerServices;
using UnityEngine;

namespace LccHotfix
{
    public partial class LogicConfigBase
    {
        /// <summary>
        /// 面向目标。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DelegateBhvCfg FaceTo(string targetIDVar, bool logError = false)
        {
            return new DelegateBhvCfg(node =>
            {
                var selfEntity = node.GetOwnerEntity();
                var target = new EntityVarCfg(targetIDVar).GetEntity(node, logError);
                if (selfEntity == null || !selfEntity.hasComTransform || target == null || !target.hasComTransform)
                    return;

                var to = target.position - selfEntity.position;
                to.y = 0;
                if (to.sqrMagnitude <= 0.0001f)
                    return;

                selfEntity.comTransform.SetRotation(Quaternion.FromToRotation(Vector3.forward, to));
            });
        }
    }
}
