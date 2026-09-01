using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface ITargetQueryService
    {
        int RangeAttackableTargetBatchAction(LogicWorld world, LogicEntity source, Vector3 position, float range, Func<LogicEntity, bool> actionFunc, bool stopOnActionFalse = false, List<LogicEntity> actionEntityList = null);
        int AttackableTargetInAabbBatchAction(LogicWorld world, LogicEntity source, AABB aabb, Func<LogicEntity, bool> actionFunc, bool stopOnActionFalse = false, List<LogicEntity> actionEntityList = null);
        LogicEntity SearchByDistanceY(LogicEntity entity, float maxDistance, bool cloakTargeting = false);
        LogicEntity GetEliteOrBossTarget(LogicWorld world, LogicEntity entity, float distance, bool cloakTargeting = false);
        bool IsCloaked(LogicEntity entity);
    }
}
