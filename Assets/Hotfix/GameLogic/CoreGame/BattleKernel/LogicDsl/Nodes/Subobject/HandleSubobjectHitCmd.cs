using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class HandleSubobjectHitCmdCfg : ICustomNodeCfg
    {
        public AssetVarCfg FxPath { get; protected set; }
        public float During { get; set; } = 2f;

        public virtual Type NodeType() { return typeof(HandleSubobjectHitCmd); }

        public FloatCfg AoeRange = new FloatCfg(0);
        public string HitSound { get; set; }

        public DamageAdjustFunc DamageAdjustAction { get; set; }

        public PreHitFunc PreHitAction { get; set; }

        public Vector3? HitBackDirection { get; private set; }
        public float HitBackDistance { get; private set; }
        public Func<CustomNode, float> HitBackEffectAddGetter { get; private set; }

        public HandleSubobjectHitCmdCfg() { }

        public HandleSubobjectHitCmdCfg(string path)
        {
            FxPath = new AssetVarCfg(path);
        }

        public HandleSubobjectHitCmdCfg WithAOE(string aoeRangeVar)
        {
            if (aoeRangeVar != null)
            {
                AoeRange.SetVarID(aoeRangeVar);
            }

            return this;
        }

        public HandleSubobjectHitCmdCfg WithHitSound(string audioEventName)
        {
            HitSound = audioEventName;
            return this;
        }

        public HandleSubobjectHitCmdCfg WithPreHitAction(PreHitFunc func)
        {
            PreHitAction = func;
            return this;
        }

        public HandleSubobjectHitCmdCfg WithDamageAdjustAction(DamageAdjustFunc func)
        {
            DamageAdjustAction = func;
            return this;
        }

        public HandleSubobjectHitCmdCfg WithHitBack(Vector3 direction, float baseDistance, Func<CustomNode, float> effectAddGetter = null)
        {
            HitBackDirection = direction;
            HitBackDistance = baseDistance;
            HitBackEffectAddGetter = effectAddGetter;
            return this;
        }
    }

    public class HandleSubobjectHitCmd : CustomNode, IEntityCommandHandler
    {
        protected HandleSubobjectHitCmdCfg mCfg;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            mCfg = cfg as HandleSubobjectHitCmdCfg;
        }

        public override void Destroy()
        {
            mCfg = null;
            base.Destroy();
        }

        public virtual bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            if (cmd.CmdType != EntityCmdType.Nt_ColliderHit)
            {
                return false;
            }

            var hitInfo = cmd.HitInfo;
            if (mCfg.HitBackDistance > 0 && mCfg.HitBackDirection.HasValue)
            {
                ApplyHitBack(this, entity, ref hitInfo);
            }

            mCfg.PreHitAction?.Invoke(this, ref hitInfo);

            var hitPos = hitInfo.hitPos;
            var hitFxPath = mCfg.FxPath.GetResPath(this);
            var aoeRange = mCfg.AoeRange.GetValue(this);
            GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.BattleEffectService?.PlayEffect(hitFxPath, hitPos, mCfg.During, aoeRange > 0 ? aoeRange : 1f);

            if (!string.IsNullOrEmpty(mCfg.HitSound))
            {
                GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.BattleAudioService?.PlayEntityAudio(entity, mCfg.HitSound);
            }

            var target = entity.OwnerWorld.GetEntityWithComID(hitInfo.hitEntityID);
            if (aoeRange > 0)
            {
                MakeAoeEffect_InAABB(entity, aoeRange, hitPos, ExecuteHitEffect);
            }
            else
            {
                ExecuteHitEffect(this, entity, target, hitInfo);
            }

            if (VarEnvRef.ReadVar<List<SpawnSubobjectInfo>>(CvKey.CV_SpawnSobjListOnHit, out var infoList))
            {
                foreach (var info in infoList)
                {
                    var e = CreateSubobjectEntity(info.subobjectTid, info.subobjectLogicID, info.path, entity.position, info.preEnv);
                    e?.AddComLife(info.lifeTime);
                }
            }

            var hitWithLife = cmd.V0.AsBool;
            var hitLeftCount = cmd.V1.AsInt;
            if (hitWithLife && hitLeftCount <= 0)
            {
                entity.RemoveComLife();
                if (!entity.hasComDeath)
                {
                    entity.AddComDeath(null);
                }
            }

            return true;
        }

        private static void ApplyHitBack(HandleSubobjectHitCmd node, LogicEntity ownerEntity, ref HitInfo hitInfo)
        {
            var target = ownerEntity.OwnerWorld.GetEntityWithComID(hitInfo.hitEntityID);
            var cfg = node.mCfg;
            var effectAdd = cfg.HitBackEffectAddGetter != null ? cfg.HitBackEffectAddGetter(node) : 0f;
            target.ApplyHitBack(cfg.HitBackDirection.Value, cfg.HitBackDistance, effectAdd);
        }

        private void ExecuteHitEffect(CustomNode node, LogicEntity entity, LogicEntity target, HitInfo hitInfo)
        {
            if (target == null)
            {
                CLHelper.LogError(this, "ExecuteHitEffect target == null");
                return;
            }

            var evtDmg = new EvtDamage(node.RootLogic, target, hitInfo);
            mCfg.DamageAdjustAction?.Invoke(this, ref evtDmg);
            GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.DamageEventService?.DispatchDamage(evtDmg);
            if (target.IsDead())
            {
                var killCmd = new EntityCommand { CmdType = EntityCmdType.Nt_Kill };
                entity.SendCmd(killCmd);
            }

            if (VarEnvRef.ReadVar<List<int>>(CvKey.CV_BuffListOnHit, out var buffList))
            {
                var buffMaxLvlDict = new Dictionary<int, int>();
                if (VarEnvRef.HasVar<Dictionary<int, int>>(CvKey.CV_BuffMaxLevel))
                {
                    VarEnvRef.ReadVar<Dictionary<int, int>>(CvKey.CV_BuffMaxLevel, out buffMaxLvlDict);
                }

                foreach (var buffLogicID in buffList)
                {
                    buffMaxLvlDict.TryGetValue(buffLogicID, out var maxLvl);
                    var genInfo = node.CreateBuffGenInfoFromUnit(target, buffLogicID, maxLvl);
                    target.AddBuff(genInfo);
                }
            }
        }
    }
}
