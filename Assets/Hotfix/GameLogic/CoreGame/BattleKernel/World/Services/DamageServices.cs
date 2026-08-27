using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IDamageEventService
    {
        void AddDamageHandler(Action<EvtDamage> handler);
        void RemoveDamageHandler(Action<EvtDamage> handler);
        void AddHealHandler(Action<EvtHeal> handler);
        void RemoveHealHandler(Action<EvtHeal> handler);
        void DispatchDamage(EvtDamage evt);
        void DispatchHeal(EvtHeal evt);
    }

    public interface IDamagePolicyService
    {
        void ModifyDamageResult(in DamageContext context, LogicEntity defender, ref DamageResult result);
        void ModifyHeal(ref HealContext context);
        void DispatchTriggerDeath(LogicEntity entity);
    }

    public interface IUnitOwnerInfoProvider
    {
        IBattlePlayerInfo GetOwnerInfo(LogicEntity entity);
    }

    public interface ICombatPropertyVolumeProvider
    {
        void AddCategoryVolumes(LogicEntity entity, ref PropertySnapshot snapshot);
        void AddSubobjectVolume(IBattlePlayerInfo playerInfo, uint subobjectTid, ref PropertySnapshot snapshot);
    }

    public interface ITargetQueryService
    {
        int RangeAttackableTargetBatchAction(LogicWorld world, LogicEntity source, Vector3 position, float range, Func<LogicEntity, bool> actionFunc, bool stopOnActionFalse = false, List<LogicEntity> actionEntityList = null);
        int AttackableTargetInAabbBatchAction(LogicWorld world, LogicEntity source, AABB aabb, Func<LogicEntity, bool> actionFunc, bool stopOnActionFalse = false, List<LogicEntity> actionEntityList = null);
        LogicEntity SearchByDistanceY(LogicEntity entity, float maxDistance, bool cloakTargeting = false);
        LogicEntity GetEliteOrBossTarget(LogicWorld world, LogicEntity entity, float distance, bool cloakTargeting = false);
        bool IsCloaked(LogicEntity entity);
    }

}
