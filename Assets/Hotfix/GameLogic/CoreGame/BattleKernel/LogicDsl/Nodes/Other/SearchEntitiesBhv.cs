using System;

namespace LccHotfix
{
    public class SearchEntitiesBhvCfg : ICustomNodeCfg
    {
        public FloatCfg Distance;

        public IntCfg SearchCount;

        public string SaveEntityIDTo = null;
        public string SaveEntityPosTo = null;

        public bool FirstEliteOrBoss = false;
        public bool CloakTargeting = false;

        public EntityVarCfg UserEntityVar { get; set; } = new EntityVarCfg(CvKey.CV_OwnerEntity);

        public Type NodeType() { return typeof(SearchEntitiesBhv); }

        public SearchEntitiesBhvCfg() { }

        public SearchEntitiesBhvCfg(float distance)
        {
            Distance = new FloatCfg(distance);
        }

        public SearchEntitiesBhvCfg(string distanceVar)
        {
            Distance = new FloatCfg(distanceVar);
        }

        public SearchEntitiesBhvCfg WithFirstEliteOrBoss()
        {
            FirstEliteOrBoss = true;
            return this;
        }

        public SearchEntitiesBhvCfg WithCloakTargeting()
        {
            CloakTargeting = true;
            return this;
        }

        public SearchEntitiesBhvCfg WithUserEntity(string entityVar)
        {
            UserEntityVar = new EntityVarCfg(entityVar);
            return this;
        }
    }

    public class SearchEntitiesBhv : BehaviorNode<SearchEntitiesBhvCfg>
    {
        private LogicEntity mUserEntity;
        private LogicWorld mLogicWorld;

        public override void Destroy()
        {
            mUserEntity = null;
            mLogicWorld = null;
            base.Destroy();
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            mUserEntity = _cfg.UserEntityVar.GetEntity(this);
            if (mUserEntity == null)
            {
                CLHelper.LogError(this, $"SearchEntitiesBhv mUserEntity == null, mCfg.EntityVar={_cfg.UserEntityVar.VarKey}");
            }

            mLogicWorld = this.GetLogicWorld();
        }

        protected override float OnUpdate(float dt)
        {
            if (_cfg == null)
            {
                return dt;
            }

            if (_cfg.FirstEliteOrBoss)
            {
                var eliteOrBossTarget = mLogicWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.TargetQueryService?.GetEliteOrBossTarget(mLogicWorld, mUserEntity, _cfg.Distance.GetValue(this), _cfg.CloakTargeting);
                if (eliteOrBossTarget != null)
                {
                    SaveTargetVar(eliteOrBossTarget);
                    return dt;
                }
            }

            var targetId = GetVar<long>(_cfg.SaveEntityIDTo, 0);
            if (targetId == 0)
            {
                SearchEntity();
                return dt;
            }

            var target = mLogicWorld.GetEntityWithComID(targetId);
            if (target == null || !target.IsValid())
            {
                SearchEntity();
                return dt;
            }

            if (!_cfg.CloakTargeting && (mLogicWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.TargetQueryService?.IsCloaked(target) ?? false))
            {
                SearchEntity();
                return dt;
            }

            return dt;
        }

        private void SearchEntity()
        {
            var dis = _cfg.Distance.GetValue(this);
            if (dis <= 0)
            {
                CLHelper.LogError(this, $"SearchEntitiesBhv SearchEntity 配置取值异常 Distance={dis}, cfg:{_cfg.Distance.LogStr}");
                SaveTargetVar(null);
                return;
            }

            var target = mLogicWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.TargetQueryService?.SearchByDistanceY(mUserEntity, dis, _cfg.CloakTargeting);
            SaveTargetVar(target);
        }

        private void SaveTargetVar(LogicEntity target)
        {
            var saveTo = _cfg.SaveEntityIDTo;
            if (string.IsNullOrEmpty(saveTo))
            {
                CLHelper.LogError(this, $"SaveTargetVar mCfg.SaveEidTo = {saveTo}");
                return;
            }

            VarEnvRef.WriteVar(saveTo, target?.ID ?? 0);
            if (!string.IsNullOrEmpty(_cfg.SaveEntityPosTo) && target != null && target.hasComTransform)
            {
                VarEnvRef.WriteVar(_cfg.SaveEntityPosTo, target.position);
            }
        }
    }
}
