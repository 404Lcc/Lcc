using System.Collections.Generic;

namespace LccModel
{
    public partial class StatusAbility : Entity, IAbilityEntity
    {
        public CombatEntity OwnerEntity { get; set; }
        public CombatEntity ParentEntity => GetParent<CombatEntity>();
        public bool Enable { get; set; }


        public StatusConfigObject statusConfig;

        public Dictionary<string, string> paramsDict;

        public bool isChildStatus;
        public int duration;
        public ChildStatus childStatusData;
        private List<StatusAbility> _statusList = new List<StatusAbility>();


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            statusConfig = p1 as StatusConfigObject;

            if (statusConfig.EffectList.Count > 0)
            {
                AddComponent<AbilityEffectComponent, List<Effect>>(statusConfig.EffectList);
            }
        }

        public void SetParams(Dictionary<string, string> paramsDict)
        {
            this.paramsDict = paramsDict;
        }
        public void ActivateAbility()
        {
            Enable = true;
            GetComponent<AbilityEffectComponent>().EnableEffect();
            if (statusConfig.EnableChildStatus)
            {
                foreach (var childStatusData in statusConfig.StatusList)
                {
                    var status = ParentEntity.AttachStatus(childStatusData.StatusConfigObject);
                    status.OwnerEntity = OwnerEntity;
                    status.isChildStatus = true;
                    status.childStatusData = childStatusData;
                    status.SetParams(childStatusData.ParamsDict);
                    status.ActivateAbility();
                    _statusList.Add(status);
                }
            }
        }


        public void EndAbility()
        {
            Enable = false;
            if (statusConfig.EnableChildStatus)
            {
                foreach (var item in _statusList)
                {
                    item.EndAbility();
                }
                _statusList.Clear();
            }

            foreach (var effect in statusConfig.EffectList)
            {
                if (!effect.Enabled)
                {
                    continue;
                }
            }

            ParentEntity.OnStatusRemove(this);

            Dispose();
        }


        public Entity CreateExecution()
        {
            return null;
        }
    }
}