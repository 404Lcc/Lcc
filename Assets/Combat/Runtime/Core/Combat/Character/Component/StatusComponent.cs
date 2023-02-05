using System.Collections.Generic;

namespace LccModel
{
    public class StatusComponent : Component
    {
        public CombatEntity CombatEntity => GetParent<CombatEntity>();


        public List<StatusAbility> statusList = new List<StatusAbility>();
        public Dictionary<int, List<StatusAbility>> statusDict = new Dictionary<int, List<StatusAbility>>();


        public StatusAbility AttachStatus(object configObject)
        {
            var status = CombatEntity.AttachAbility<StatusAbility>(configObject);
            if (!statusDict.ContainsKey(status.statusConfig.Id))
            {
                statusDict.Add(status.statusConfig.Id, new List<StatusAbility>());
            }
            statusDict[status.statusConfig.Id].Add(status);
            statusList.Add(status);
            return status;
        }

        public void OnStatusRemove(StatusAbility statusAbility)
        {
            statusDict[statusAbility.statusConfig.Id].Remove(statusAbility);
            if (statusDict[statusAbility.statusConfig.Id].Count == 0)
            {
                statusDict.Remove(statusAbility.statusConfig.Id);
            }
            statusList.Remove(statusAbility);
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
                    foreach (var effect in status.GetComponent<AbilityEffectComponent>().abilityEffectList)
                    {
                        if (effect.enable && effect.TryGetComponent(out AbilityEffectActionControlComponent actionControlComponent))
                        {
                            tempActionControl = tempActionControl | actionControlComponent.actionControlEffect.ActionControlType;
                        }
                    }
                }
            }

            parentEntity.actionControlType = tempActionControl;
            var moveForbid = parentEntity.actionControlType.HasFlag(ActionControlType.MoveForbid);
            parentEntity.GetComponent<MotionComponent>().SetEnable(!moveForbid);
        }
    }
}