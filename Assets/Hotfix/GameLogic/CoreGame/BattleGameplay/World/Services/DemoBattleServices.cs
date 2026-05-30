using System;
using System.Collections.Generic;
using PBConfig;
using UnityEngine;

namespace LccHotfix
{
    public class DemoBattleModeLogicService : IBattleModeLogicService
    {
        public BattleModeLogic CreateModeLogic(ICustomLogicGenInfo genInfo)
        {
            genInfo.PreEnv.ReadVar(CvKey.CV_WorldInfo, out ECGameWorldCreationInfo creationInfo);
            var service = creationInfo.CustomLogicService;
            return service.CreateLogic<BattleModeLogic>(genInfo);
        }

        public void DestroyModeLogic(BattleModeLogic logic)
        {
            Main.CustomLogicService?.DestroyLogic(logic);
        }
    }

    public class DemoBattleFeedbackSink : IBattleFeedbackSink
    {
        public void ShowDamageMiss(Vector3 position, bool usePrimaryStateFeedbackStyle)
        {
            BattleLog.Debug($"Damage miss at {position}");
        }

        public void ShowDamageBlock(Vector3 position, bool usePrimaryStateFeedbackStyle)
        {
            BattleLog.Debug($"Damage block at {position}");
        }

        public void ShowDamageNumber(int damage, Vector3 position, bool useTaggedDefenderStyle, bool isCritical)
        {
            BattleLog.Debug($"Damage {damage} at {position}, critical={isCritical}");
        }

        public void ShowHealNumber(int healing, Vector3 position, bool useTaggedTargetStyle)
        {
            BattleLog.Debug($"Heal {healing} at {position}");
        }
    }

    public class DemoViewLoadService : IViewLoadService
    {
        public void LoadObjectAsync(string objName, Action<IReceiveLoaded> onComplete)
        {
            BattleLog.Warning($"DemoViewLoadService skipped loading view: {objName}");
            onComplete?.Invoke(null);
        }
    }

    public class DemoDamageEventService : IDamageEventService
    {
        private readonly List<Action<EvtDamage>> _damageHandlers = new List<Action<EvtDamage>>();
        private readonly List<Action<EvtHeal>> _healHandlers = new List<Action<EvtHeal>>();

        public void AddDamageHandler(Action<EvtDamage> handler)
        {
            if (handler != null && !_damageHandlers.Contains(handler))
            {
                _damageHandlers.Add(handler);
            }
        }

        public void RemoveDamageHandler(Action<EvtDamage> handler)
        {
            _damageHandlers.Remove(handler);
        }

        public void AddHealHandler(Action<EvtHeal> handler)
        {
            if (handler != null && !_healHandlers.Contains(handler))
            {
                _healHandlers.Add(handler);
            }
        }

        public void RemoveHealHandler(Action<EvtHeal> handler)
        {
            _healHandlers.Remove(handler);
        }

        public void DispatchDamage(EvtDamage evt)
        {
            for (int i = 0; i < _damageHandlers.Count; i++)
            {
                _damageHandlers[i]?.Invoke(evt);
            }
        }

        public void DispatchHeal(EvtHeal evt)
        {
            for (int i = 0; i < _healHandlers.Count; i++)
            {
                _healHandlers[i]?.Invoke(evt);
            }
        }
    }

    public class DemoDamagePolicyService : IDamagePolicyService
    {
        public bool UsePrimaryStateFeedbackStyle(LogicEntity entity)
        {
            return false;
        }

        public bool UseTaggedFeedbackStyle(LogicEntity entity)
        {
            return false;
        }

        public void ModifyHeal(ref HealContext context)
        {
        }

        public void DispatchTriggerDeath(LogicEntity entity)
        {
            if (entity != null && !entity.hasComDeath)
            {
                entity.AddComDeath(null);
            }
        }
    }

    public class DemoDamagePropertyModifier : IDamagePropertyModifier
    {
        public void ModifyContextProperties(UnitSource attacker, LogicEntity defender, ref DamageContext context)
        {
            context.RandomFinalDamageRate = 1f;
        }

        public double ApplyDamageTypeDamage(in DamageContext context, double baseDamage)
        {
            return baseDamage;
        }
    }

    public class DemoUnitOwnerInfoProvider : IUnitOwnerInfoProvider
    {
        public IBattlePlayerInfo GetOwnerInfo(LogicEntity entity)
        {
            return entity != null && entity.hasComOwnerPlayer ? entity.comOwnerPlayer.PlayerInfoRef : null;
        }
    }

    public class DemoCombatPropertyVolumeProvider : ICombatPropertyVolumeProvider
    {
        public void AddCategoryVolumes(LogicEntity entity, ref PropertySnapshot snapshot)
        {
        }

        public void AddSubobjectVolume(IBattlePlayerInfo playerInfo, uint subobjectTid, ref PropertySnapshot snapshot)
        {
        }
    }

    public class DemoTargetQueryService : ITargetQueryService
    {
        public int RangeAttackableTargetBatchAction(LogicWorld world, LogicEntity source, Vector3 position, float range, Func<LogicEntity, bool> actionFunc, bool stopOnActionFalse = false, List<LogicEntity> actionEntityList = null)
        {
            if (world == null || source == null)
            {
                return 0;
            }

            int count = 0;
            var group = world.GetLogicGroup_Faction_HP_Transform();
            foreach (var target in group.GetEntities())
            {
                if (target == source || target.Faction == source.Faction || target.comHp.Hp <= 0)
                {
                    continue;
                }

                if (Vector3.Distance(position, target.position) > range)
                {
                    continue;
                }

                actionEntityList?.Add(target);
                count++;
                if (actionFunc != null && !actionFunc(target) && stopOnActionFalse)
                {
                    break;
                }
            }

            return count;
        }

        public LogicEntity SearchByDistanceY(LogicEntity entity, float maxDistance, bool cloakTargeting = false)
        {
            if (entity == null)
            {
                return null;
            }

            LogicEntity best = null;
            float bestDistance = maxDistance;
            var group = entity.OwnerWorld.GetLogicGroup_Faction_HP_Transform();
            foreach (var target in group.GetEntities())
            {
                if (target == entity || target.Faction == entity.Faction || target.comHp.Hp <= 0)
                {
                    continue;
                }

                var distance = Mathf.Abs(target.position.y - entity.position.y);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            return best;
        }

        public LogicEntity GetEliteOrBossTarget(LogicWorld world, LogicEntity entity, float distance, bool cloakTargeting = false)
        {
            return SearchByDistanceY(entity, distance, cloakTargeting);
        }

        public bool IsCloaked(LogicEntity entity)
        {
            return false;
        }
    }

    public class DemoSubobjectModelOverrideProvider : ISubobjectModelOverrideProvider
    {
        public string ResolveMainModelPath(IBattlePlayerInfo playerInfo, uint subobjectTid, string defaultPath)
        {
            return defaultPath;
        }
    }

    public class DemoSkillLogicOverrideProvider : ISkillLogicOverrideProvider
    {
        public int ResolveSkillLogicId(IBattlePlayerInfo playerInfo, TFighter fighterCfg, int defaultLogicId)
        {
            return defaultLogicId;
        }
    }

    public class DemoBattleEffectService : IBattleEffectService
    {
        public void PlayEffect(string path, Vector3 position, float duration, float scale = 1f)
        {
            BattleLog.Debug($"PlayEffect path={path}, position={position}, duration={duration}, scale={scale}");
        }
    }

    public class DemoBattleAudioService : IBattleAudioService
    {
        public void PlayEntityAudio(LogicEntity entity, string eventName)
        {
            BattleLog.Debug($"PlayEntityAudio entity={entity?.ID}, event={eventName}");
        }
    }

    public class DemoBattleLogService : IBattleLogService
    {
        public bool IsDebugEnabled => true;

        public void Debug(string info, string path = null, string memberName = null, int lineNumber = 0)
        {
            UnityEngine.Debug.Log(info);
        }

        public void Warning(string info, string path = null, string memberName = null, int lineNumber = 0)
        {
            UnityEngine.Debug.LogWarning(info);
        }

        public void Error(string info, string path = null, string memberName = null, int lineNumber = 0)
        {
            UnityEngine.Debug.LogError(info);
        }

        public void Exception(Exception exception, string path = null, string memberName = null, int lineNumber = 0)
        {
            UnityEngine.Debug.LogException(exception);
        }
    }

    public class DemoDeathProcessService : IDeathProcessService
    {
        public void RemoveExternalComponentsBeforeDestroy(LogicEntity entity)
        {
        }
    }

    public class DemoSubobjectTransferEffectService : ISubobjectTransferEffectService
    {
        public void OnTransferred(CustomNode node, LogicEntity target)
        {
        }
    }
}
