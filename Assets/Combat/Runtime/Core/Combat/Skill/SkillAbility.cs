using System.Collections.Generic;

namespace LccModel
{
    public class SkillAbility : Entity, IAbilityEntity
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public CombatEntity ParentEntity => GetParent<CombatEntity>();
        public bool Enable { get; set; }


        public SkillConfigObject skillConfig;
        public ExecutionConfigObject executionObject;
        public bool spelling;

        private List<StatusAbility> _statusList = new List<StatusAbility>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            skillConfig = p1 as SkillConfigObject;
            AddComponent<AbilityEffectComponent, List<Effect>>(skillConfig.EffectList);

            executionObject = AssetManager.Instance.LoadAsset<ExecutionConfigObject>(out var handler, $"Execution_{skillConfig.Id}", AssetSuffix.Asset, AssetType.Execution);

            if (skillConfig.SkillSpellType == SkillSpellType.Passive)
            {
                ActivateAbility();
            }
        }





        public void ActivateAbility()
        {
            Enable = true;

            if (skillConfig.EnableChildStatus)
            {
                foreach (var item in skillConfig.StatusList)
                {
                    var status = OwnerEntity.AttachStatus(item.StatusConfigObject);
                    status.OwnerEntity = OwnerEntity;
                    status.isChildStatus = true;
                    status.childStatusData = item;
                    status.ProcessInputKVParams(item.ParamsDict);
                    status.ActivateAbility();
                    _statusList.Add(status);
                }
            }
        }
        public void DeactivateAbility()
        {
            Enable = false;
        }
        public void EndAbility()
        {
            if (skillConfig.EnableChildStatus)
            {
                foreach (var item in _statusList)
                {
                    item.EndAbility();
                }
                _statusList.Clear();
            }
            Dispose();
        }

        public Entity CreateExecution()
        {
            var execution = OwnerEntity.AddChildren<SkillExecution, SkillAbility>(this);
            execution.executionConfigObject = executionObject;
            execution.LoadExecutionEffect();
            this.FireEvent(nameof(CreateExecution), execution);
            return execution;
        }
    }
}