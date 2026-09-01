using UnityEngine;

namespace LccHotfix
{
    public static class CustomNodeHitExtensions
    {
    #region AOE命中处理


        /// <summary>
        /// 以中心点和范围构造 AABB，并对范围内目标执行 AOE 命中效果。
        /// </summary>
        public static void MakeAoeEffect_InAABB(this CustomNode self, LogicEntity entity, float aoeRange, Vector3 aoeCenterPos, NodeHitEffectFunc executeEffectFunc)
        {
            if (aoeRange <= 0)
            {
                self.LogError($"MakeAoeEffect_InAABB AoeRange({aoeRange}) <= 0");
                return;
            }

            var creationInfo = entity?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>();
            if (creationInfo == null)
            {
                self.LogError("MakeAoeEffect_InAABB creationInfo == null");
                return;
            }

            var aoeAABB = new AABB(aoeCenterPos, aoeRange, creationInfo.BattlePlane);
            self.MakeAoeEffect_InAABB(entity, aoeAABB, executeEffectFunc);
        }

        /// <summary>
        /// 对指定 AABB 范围内可碰撞的目标实体执行 AOE 命中效果。
        /// </summary>
        public static void MakeAoeEffect_InAABB(this CustomNode self, LogicEntity entity, AABB aabb, NodeHitEffectFunc executeEffectFunc)
        {
            var world = entity?.OwnerWorld;
            var targetQueryService = world?.GetCreationInfo<BattleKernelCreationInfo>()?.TargetQueryService;
            if (targetQueryService == null)
            {
                return;
            }

            targetQueryService.AttackableTargetInAabbBatchAction(world, entity, aabb, item =>
            {
                var aoeHitInfo = new HitInfo { hitPos = item.position, hitEntityID = item.ID };
                executeEffectFunc(self, entity, item, aoeHitInfo);
                return true;
            });
        }

    #endregion
    }
}
