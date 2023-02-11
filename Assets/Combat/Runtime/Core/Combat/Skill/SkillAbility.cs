using System.Collections.Generic;

namespace LccModel
{
    public class SkillAbility : Entity, IAbilityEntity
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public CombatEntity ParentEntity => GetParent<CombatEntity>();
        public bool Enable { get; set; }


        public SkillConfigObject skillConfigObject;
        public ExecutionConfigObject executionConfigObject;
        public bool spelling;

        private List<StatusAbility> _statusList = new List<StatusAbility>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            skillConfigObject = p1 as SkillConfigObject;
            AddComponent<AbilityEffectComponent, List<Effect>>(skillConfigObject.EffectList);

            executionConfigObject = AssetManager.Instance.LoadAsset<ExecutionConfigObject>(out var handler, $"Execution_{skillConfigObject.Id}", AssetSuffix.Asset, AssetType.Execution);

        }





        public void ActivateAbility()
        {
            Enable = true;

            if (skillConfigObject.EnableChildStatus)
            {
                foreach (var item in skillConfigObject.StatusList)
                {
                    var status = OwnerEntity.AttachStatus(item.StatusConfigObject);
                    status.OwnerEntity = OwnerEntity;
                    status.isChildStatus = true;
                    status.childStatusData = item;
                    status.SetParams(item.ParamsDict);
                    status.ActivateAbility();
                    _statusList.Add(status);
                }
            }
        }
        public void EndAbility()
        {
            Enable = false;

            if (skillConfigObject.EnableChildStatus)
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
            execution.executionConfigObject = executionConfigObject;
            execution.LoadExecutionEffect();
            FireEvent(nameof(CreateExecution), execution);
            return execution;
        }
    }
}