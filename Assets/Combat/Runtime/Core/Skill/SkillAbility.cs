using System.Collections.Generic;

namespace LccModel
{
    public partial class SkillAbility : Entity, IAbilityEntity
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public CombatEntity ParentEntity { get => GetParent<CombatEntity>(); }
        public bool Enable { get; set; }
        public SkillConfigObject SkillConfig { get; set; }
        public bool Spelling { get; set; }
        public GameTimer CooldownTimer { get; } = new GameTimer(1f);
        private List<StatusAbility> ChildrenStatuses { get; set; } = new List<StatusAbility>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            SkillConfig = p1 as SkillConfigObject;
            //Name = SkillConfig.Name;
            AddComponent<AbilityEffectComponent>(SkillConfig.Effects);
#if !SERVER
            //Awake_Client();
#endif
            if (SkillConfig.SkillSpellType == SkillSpellType.Passive)
            {
                TryActivateAbility();
            }
        }
        
        public void TryActivateAbility()
        {
            this.ActivateAbility();
        }

        public void DeactivateAbility()
        {
            Enable = false;
        }

        public void ActivateAbility()
        {
            //base.ActivateAbility();
            FireEvent(nameof(ActivateAbility));
            //子状态效果
            if (SkillConfig.EnableChildrenStatuses)
            {
                foreach (var item in SkillConfig.ChildrenStatuses)
                {
                    var status = OwnerEntity.AttachStatus(item.StatusConfigObject);
                    status.OwnerEntity = OwnerEntity;
                    status.IsChildStatus = true;
                    status.ChildStatusData = item;
                    status.ProcessInputKVParams(item.Params);
                    status.TryActivateAbility();
                    ChildrenStatuses.Add(status);
                }
            }
        }

        public void EndAbility()
        {
            //子状态效果
            if (SkillConfig.EnableChildrenStatuses)
            {
                foreach (var item in ChildrenStatuses)
                {
                    item.EndAbility();
                }
                ChildrenStatuses.Clear();
            }
            Dispose();
        }

        public Entity CreateExecution()
        {
            return null;
            //var execution = OwnerEntity.AddChildren<SkillExecution>(this);
            //execution.ExecutionObject = ExecutionObject;
            //execution.LoadExecutionEffects();
            //this.FireEvent(nameof(CreateExecution), execution);
            //if (ExecutionObject != null)
            //{
            //    execution.AddComponent<UpdateComponent>();
            //}
            //return execution;
        }
    }
}