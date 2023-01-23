using System.Collections.Generic;

namespace LccModel
{
    public class RemoveStatusEvent
    {
        public CombatEntity CombatEntity { get; set; }
        public StatusAbility Status { get; set; }
        public long StatusId { get; set; }
    }
    public class StatusComponent : Component
	{
        public CombatEntity CombatEntity => GetParent<CombatEntity>();
        public List<StatusAbility> Statuses { get; set; } = new List<StatusAbility>();
        public Dictionary<string, List<StatusAbility>> TypeIdStatuses { get; set; } = new Dictionary<string, List<StatusAbility>>();


        public StatusAbility AttachStatus(object configObject)
        {
            var status = CombatEntity.AttachAbility<StatusAbility>(configObject);
            if (!TypeIdStatuses.ContainsKey(status.StatusConfig.ID))
            {
                TypeIdStatuses.Add(status.StatusConfig.ID, new List<StatusAbility>());
            }
            TypeIdStatuses[status.StatusConfig.ID].Add(status);
            Statuses.Add(status);
            return status;
        }

        public void OnStatusRemove(StatusAbility statusAbility)
        {
            TypeIdStatuses[statusAbility.StatusConfig.ID].Remove(statusAbility);
            if (TypeIdStatuses[statusAbility.StatusConfig.ID].Count == 0)
            {
                TypeIdStatuses.Remove(statusAbility.StatusConfig.ID);
            }
            Statuses.Remove(statusAbility);
            this.Publish(new RemoveStatusEvent() 
            {
                CombatEntity = CombatEntity,
                Status = statusAbility,
                StatusId = statusAbility.Id
            });
        }

        public void OnStatusesChanged(StatusAbility statusAbility)
        {
            var parentEntity = CombatEntity;

            var tempActionControl = ActionControlType.None;
            foreach (var item in CombatEntity.Children.Values)
            {
                if (item is StatusAbility status)
                {
                    if (!status.Enable)
                    {
                        continue;
                    }
                    foreach (var effect in status.GetComponent<AbilityEffectComponent>().AbilityEffects)
                    {
                        if (effect.Enable && effect.TryGetComponent(out EffectActionControlComponent actionControlComponent))
                        {
                            tempActionControl = tempActionControl | actionControlComponent.ActionControlEffect.ActionControlType;
                        }
                    }
                }
            }

            parentEntity.ActionControlType = tempActionControl;
            var moveForbid = parentEntity.ActionControlType.HasFlag(ActionControlType.MoveForbid);
            //parentEntity.GetComponent<MotionComponent>().Enable = !moveForbid;
        }
	}
}
