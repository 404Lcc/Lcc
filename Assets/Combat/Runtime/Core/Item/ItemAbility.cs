using System.Collections.Generic;

namespace LccModel
{
    public partial class ItemAbility : Entity, IAbilityEntity
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public CombatEntity ParentEntity { get => GetParent<CombatEntity>(); }
        public bool Enable { get; set; }
        public ItemConfigObject ItemConfig { get; set; }


        private List<StatusAbility> ChildrenStatuses { get; set; } = new List<StatusAbility>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            ItemConfig = p1 as ItemConfigObject;

            AddComponent<AbilityEffectComponent, List<Effect>>(ItemConfig.Effects);
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
            FireEvent(nameof(ActivateAbility));
            //子状态效果
            if (ItemConfig.EnableChildrenStatuses)
            {
                foreach (var item in ItemConfig.ChildrenStatuses)
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
            if (ItemConfig.EnableChildrenStatuses)
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
        }
    }
}