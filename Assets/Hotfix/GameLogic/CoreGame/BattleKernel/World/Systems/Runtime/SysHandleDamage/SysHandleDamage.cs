using System;
using System.Runtime.CompilerServices;
using Entitas;
using Random = System.Random;

namespace LccHotfix
{
    public enum EDamageType
    {
        EdtNull,
        EdtAmmunition,
        EdtPhysics,
        EdtEnergy,
        EdtElectricity,
        EdtForce,
        EdtDark,
    }

    public struct EvtDamage : IValueEvent
    {
        public const int PointCloseTargetDmgOffset = 4;

        public DamageContext Context;

        public LogicEntity DefenderEntity;

        /// 伤害触发的源头逻辑 + 目标对象
        public EvtDamage(CustomLogic sourceLogic, LogicEntity e_defender, HitInfo? hitInfo = null)
        {
            DefenderEntity = e_defender;
            if (e_defender == null)
            {
                CLHelper.LogError(sourceLogic, "new EvtDamage e_defender == null");
                Context = default;
                return;
            }

            if (!sourceLogic.GetSumUnitSource(out var attackerSum))
            {
                if (attackerSum.FighterEnityId == 0)
                {
                    attackerSum = sourceLogic.CreateTempSumUnitSource();
                }
            }

            sourceLogic.GetSubobjectSource(out var attackerSbj);

            float skillDamageFactor = sourceLogic.GetVar<float>(CvKey.CV_SkillDmageRate, 1f);

            if (BattleLogger.IsDebugEnabled)
            {
                BattleLogger.LogDebug($"EvtDamage SkillDamageFactor={skillDamageFactor}, sourceLogic={sourceLogic.GenInfo.LogicConfigID}");
            }

            EDamageType damageType = EDamageType.EdtNull;
            if (sourceLogic.HasVar<EDamageType>(CvKey.CV_DamageType))
                damageType = sourceLogic.GetVar<EDamageType>(CvKey.CV_DamageType);

            Context = MakeDamageContext(attackerSum, attackerSbj, e_defender, hitInfo, skillDamageFactor, damageType);
        }

        public static EvtDamage MakeRealDmgEvt(UnitSource atker, LogicEntity dfder, float realDmg, EDamageType damageType)
        {
            if (dfder == null)
            {
                return new EvtDamage();
            }

            EvtDamage evt = new EvtDamage();
            evt.DefenderEntity = dfder;
            evt.Context = MakeDamageContext(atker, new SubobjectSource(), dfder, null, 0, damageType);
            evt.Context.FinalFixedDamage = realDmg;
            return evt;
        }

        private static DamageContext MakeDamageContext(UnitSource attackerSum, SubobjectSource sbjSource, LogicEntity e_defender, HitInfo? hitInfo, float skillDamageFactor, EDamageType damageType)
        {
            var Context = new DamageContext();
            Context.World = e_defender.OwnerWorld;
            Context.Attacker = attackerSum;
            Context.Defender = new UnitSource(e_defender);
            Context.Skill = new SkillSource();
            Context.Subobject = sbjSource;
            Context.HitInfo = hitInfo;
            Context.DamageType = damageType;

            Context.SkillDamageFactor = skillDamageFactor;
            Context.StageDamageFactor = 0f;
            Context.Timestamp = BattleTime.GetNowTicks(Context.World);

            Context.World.GetCreationInfo<BattleKernelCreationInfo>().DamagePropertyModifier?.ModifyContextProperties(attackerSum, e_defender, ref Context);

            if (BattleLogger.IsDebugEnabled)
            {
                BattleLogger.LogDebug($"EvtDamage make damge context SkillDamageFactor = {Context.SkillDamageFactor}");
                BattleLogger.LogDebug($"EvtDamage make damge context SkillFixedDamage = {Context.SkillFixedDamage}");
                BattleLogger.LogDebug($"EvtDamage make damge context ExtraCritDamage = {Context.ExtraCritDamage}");
                BattleLogger.LogDebug($"EvtDamage make damge context RandomFinalDamageRate = {Context.RandomFinalDamageRate}");
            }

            return Context;
        }
    }

    // 目前没有特别细的回血流程，以及没有时间设计，先放在这里试试
    public struct EvtHeal : IValueEvent
    {
        public HealContext Context;

        public static EvtHeal MakeHealEvt(LogicEntity healer, LogicEntity target, float healing)
        {
            if (healer == null || target == null)
            {
                BattleLogger.LogError("new EvtHeal healer == null || target == null");
                return new EvtHeal();
            }

            var healUS = new UnitSource(healer);
            var targetUS = new UnitSource(target);
            EvtHeal evt = new EvtHeal();
            evt.Context = MakeHealContext(healer.OwnerWorld, healUS, targetUS, healing);
            return evt;
        }

        public static HealContext MakeHealContext(LogicWorld world, UnitSource healUS, UnitSource targetUS, float healing)
        {
            HealContext ctx = new HealContext();
            ctx.World = world;
            ctx.Healer = healUS;
            ctx.Target = targetUS;
            ctx.Healing = healing;
            return ctx;
        }
    }

    public class SysHandleDamage : IInitializeSystem, ITearDownSystem
    {
        private readonly IDamageCalculator _calculator;
        private readonly IEntityDamageHandler _handler;
        private readonly IEntityHealHandler _healHandler;
        private DamageRecorder _recorder;
        private MetaWorld _metaWorld;
        private ECWorlds _worlds;

        public SysHandleDamage(ECWorlds worlds)
        {
            _worlds = worlds;
            _metaWorld = worlds.MetaWorld;
            _calculator = new AdvancedDamageCalculator();
            _handler = new DamageHandler();
            _healHandler = new HealHandler();
        }

        public void Initialize()
        {
            _worlds.LogicWorld.GetCreationInfo<BattleKernelCreationInfo>().DamageEventService?.AddDamageHandler(HandleEvtDamage);
            _worlds.LogicWorld.GetCreationInfo<BattleKernelCreationInfo>().DamageEventService?.AddHealHandler(HandleEvtHeal);
            _recorder = _metaWorld.comUniGameMode.DmgRecorder;
        }

        public void TearDown()
        {
            _worlds.LogicWorld.GetCreationInfo<BattleKernelCreationInfo>().DamageEventService?.RemoveDamageHandler(HandleEvtDamage);
            _worlds.LogicWorld.GetCreationInfo<BattleKernelCreationInfo>().DamageEventService?.RemoveHealHandler(HandleEvtHeal);
        }


        private void HandleEvtDamage(EvtDamage evt)
        {
            ApplyDamage(ref evt.Context);
        }

        private void HandleEvtHeal(EvtHeal evt)
        {
            ApplyHeal(ref evt.Context);
        }

        private Random _randomMaker = new Random();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ApplyDamage(ref DamageContext context)
        {
            context.Random = _randomMaker;

            // 计算伤害
            var result = _calculator.Calculate(ref context);

            // 执行伤害
            _handler?.HandleDamage(context, ref result);

            // 记录伤害
            _recorder?.RecordDamage(context, result);
        }

        public void ApplyHeal(ref HealContext context)
        {
            context.World?.GetCreationInfo<BattleKernelCreationInfo>()?.DamagePolicyService?.ModifyHeal(ref context);

            // 处理治疗
            _healHandler?.HandleHeal(context);
            // TODO ： 记录治疗
        }
    }
}
