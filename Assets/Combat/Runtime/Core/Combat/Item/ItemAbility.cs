using System.Collections.Generic;

namespace LccModel
{
    public partial class ItemAbility : Entity, IAbilityEntity
    {
        public CombatEntity OwnerEntity { get => GetParent<CombatEntity>(); set { } }
        public CombatEntity ParentEntity => GetParent<CombatEntity>();
        public bool Enable { get; set; }


        public ItemConfigObject itemConfig;
        private List<StatusAbility> _statusList = new List<StatusAbility>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            itemConfig = p1 as ItemConfigObject;

            AddComponent<AbilityEffectComponent, List<Effect>>(itemConfig.EffectList);
        }


        public void ActivateAbility()
        {
            Enable = true;

            if (itemConfig.EnableChildStatus)
            {
                foreach (var item in itemConfig.StatusList)
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

        public void EndAbility()
        {
            Enable = false;

            if (itemConfig.EnableChildStatus)
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
            return null;
        }
    }
}