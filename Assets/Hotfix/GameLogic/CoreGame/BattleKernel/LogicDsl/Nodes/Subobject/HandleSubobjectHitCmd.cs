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

        public BuffPreEnvFunc BuffPreEnvAction { get; set; }

        public Vector3? HitBackDirection { get; private set; }
        public float HitBackDistance { get; private set; }
        public Func<CustomNode, float> HitBackEffectAddGetter { get; private set; }
        public bool BindHitFxToTarget { get; private set; }
        public float EdgeDamageRatio { get; private set; } = 1f; // 伤害随距离线性递减，边缘的伤害比例
        public float EdgeBuffTimeRatio { get; private set; } = 1f; // buff时间随距离线性递减，边缘的buff时长比例

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

        public HandleSubobjectHitCmdCfg WithBuffPreEnvAction(BuffPreEnvFunc func)
        {
            BuffPreEnvAction = func;
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

        public HandleSubobjectHitCmdCfg WithBindHitFxToTarget()
        {
            BindHitFxToTarget = true;
            return this;
        }

        /// <summary>
        /// 伤害和buff时间随目标与受击点的距离线性递减：中心为满伤害，最边缘为指定值
        /// 顺便也可以对附加的buff时长做调整
        /// </summary>
        /// <returns></returns>
        public HandleSubobjectHitCmdCfg WithAOEDistanceDecay(float edgeDamageRatio, float edgeBuffTimeRatio = 1f)
        {
            EdgeDamageRatio = edgeDamageRatio;
            EdgeBuffTimeRatio = edgeBuffTimeRatio;
            return this;
        }
    }

    public class HandleSubobjectHitCmd : CustomNode, IEntityCommandHandler
    {
        public const string BoundHitFxListKey = "BoundHitFxList";

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
            var aoeRange = mCfg.AoeRange.GetValue(this);
            var effectService = this.GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.BattleEffectService;
            if (hitInfo.hitType == HitType.Obstacle)
            {
                // Obstacle: play HitGroundFx when configured.
                var hitGroundFxTid = entity.comSubobject?.Cfg?.HitGroundFx ?? 0u;
                if (hitGroundFxTid > 0)
                {
                    var hitGroundFxPath = new AssetVarCfg(hitGroundFxTid).GetResPath(this, false);
                    if (!string.IsNullOrEmpty(hitGroundFxPath))
                        effectService?.PlayEffect(hitGroundFxPath, hitPos, mCfg.During, aoeRange > 0 ? aoeRange : 1f);
                }
            }
            else
            {
                var hitFxPath = mCfg.FxPath.GetResPath(this);
                if (!string.IsNullOrEmpty(hitFxPath))
                {
                    var hitTarget = entity.OwnerWorld?.GetEntityWithComID(hitInfo.hitEntityID);
                    if (!TryPlayHitFxBoundToTarget(hitFxPath, hitTarget))
                        effectService?.PlayEffect(hitFxPath, hitPos, mCfg.During, aoeRange > 0 ? aoeRange : 1f);
                }
            }

            if (!string.IsNullOrEmpty(mCfg.HitSound))
            {
                this.GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.BattleAudioService?.PlayEntityAudio(entity, mCfg.HitSound);
            }

            if (hitInfo.hitType == HitType.Obstacle)
            {
                HandleHitLife(entity, cmd, hitInfo);
                return true;
            }

            var target = entity.OwnerWorld.GetEntityWithComID(hitInfo.hitEntityID);
            if (aoeRange > 0)
            {
                this.MakeAoeEffect_InAABB(entity, aoeRange, hitPos, ExecuteHitEffect);
            }
            else
            {
                ExecuteHitEffect(this, entity, target, hitInfo);
            }

            if (VarEnvRef.ReadVar<List<SpawnSubobjectInfo>>(CvKey.CV_SpawnSobjListOnHit, out var infoList))
            {
                foreach (var info in infoList)
                {
                    var e = this.CreateSubobjectEntity(info.subobjectTid, info.subobjectLogicID, info.path, entity.position, info.preEnv);
                    e?.AddComLife(info.lifeTime);
                }
            }

            HandleHitLife(entity, cmd, hitInfo);

            return true;
        }

        private bool TryPlayHitFxBoundToTarget(string hitFxPath, LogicEntity target)
        {
            if (!mCfg.BindHitFxToTarget)
                return false;

            var bindTf = target?.GetMainViewBindTransform("Center", false);
            if (bindTf == null)
                return false;

            var fx = Main.FxService.Create(hitFxPath, bindTf, mCfg.During);
            TrackBoundHitFx(target, fx);
            return true;
        }

        public static void TrackBoundHitFx(LogicEntity target, FxOne fx)
        {
            if (target == null || fx == null || !target.hasComFSM || target.comFSM.Logic == null)
                return;

            var logic = target.comFSM.Logic;
            var list = logic.GetVar<List<FxOne>>(BoundHitFxListKey, null);
            if (list == null)
            {
                list = new List<FxOne>();
                logic.SetVar(BoundHitFxListKey, list);
            }

            list.Add(fx);
        }

        public static void ReleaseBoundHitFx(LogicEntity target)
        {
            if (target == null || !target.hasComFSM || target.comFSM.Logic == null)
                return;

            var logic = target.comFSM.Logic;
            var list = logic.GetVar<List<FxOne>>(BoundHitFxListKey, null);
            if (list == null)
                return;

            for (var i = 0; i < list.Count; i++)
            {
                var fx = list[i];
                if (fx != null && !fx.bIsReleased)
                    fx.Release();
            }

            list.Clear();
            logic.ClearVar<List<FxOne>>(BoundHitFxListKey);
        }

        private static void HandleHitLife(LogicEntity entity, EntityCommand cmd, HitInfo hitInfo)
        {
            var hitWithLife = cmd.V0.AsBool;
            var hitLeftCount = cmd.V1.AsInt;
            if (hitInfo.keepAliveOnHit || !hitWithLife || hitLeftCount > 0)
            {
                return;
            }

            entity.RemoveComLife();
            if (!entity.hasComDeath)
            {
                entity.AddComDeath(null);
            }
        }

        private static void ApplyHitBack(HandleSubobjectHitCmd node, LogicEntity ownerEntity, ref HitInfo hitInfo)
        {
            // var target = ownerEntity.OwnerWorld.GetEntityWithComID(hitInfo.hitEntityID);
            // var cfg = node.mCfg;
            // var effectAdd = cfg.HitBackEffectAddGetter != null ? cfg.HitBackEffectAddGetter(node) : 0f;
            // target.ApplyHitBack(cfg.HitBackDirection.Value, cfg.HitBackDistance, effectAdd);
        }

        private void ExecuteHitEffect(CustomNode node, LogicEntity entity, LogicEntity target, HitInfo hitInfo)
        {
            if (target == null)
            {
                CLogger.LogError(this, "ExecuteHitEffect target == null");
                return;
            }

            var distance = (entity.position - target.position).magnitude;
            var distanceDecayRatio = entity.hasComBounds ? Math.Clamp(1 - distance / entity.comBounds.GetRadius(), 0f, 1f) : 1f;

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
                    mCfg.BuffPreEnvAction?.Invoke(this, genInfo, hitInfo);
                    genInfo.PreEnv.WriteVar(CvKey.CV_SubobjHitInfo, hitInfo);

                    // buff时间随距中心距离衰减
                    if (mCfg.EdgeBuffTimeRatio != 1f)
                    {
                        genInfo.DurationAddRate -= (1 - distanceDecayRatio) * (1 - mCfg.EdgeBuffTimeRatio);
                    }

                    target.AddBuff(genInfo);
                }
            }

            var evtDmg = new EvtDamage(node.RootLogic, target, hitInfo);
            mCfg.DamageAdjustAction?.Invoke(this, ref evtDmg);

            // 伤害随距中心距离衰减
            if (mCfg.EdgeDamageRatio != 1f)
            {
                evtDmg.Context.SkillDamageFactor *= distanceDecayRatio * (1 - mCfg.EdgeDamageRatio) + mCfg.EdgeDamageRatio;
            }

            this.GetLogicWorld()?.GetCreationInfo<BattleKernelCreationInfo>()?.DamageEventService?.DispatchDamage(evtDmg);
            if (target.IsDead())
            {
                var killCmd = new EntityCommand { CmdType = EntityCmdType.Nt_Kill };
                entity.SendCmd(killCmd);
            }
        }
    }
}
