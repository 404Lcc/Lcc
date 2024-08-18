using System.Collections.Generic;

namespace LccModel
{
    public class StatusComponent : Component
    {
        public Combat Combat => GetParent<Combat>();


        public List<StatusAbility> statusList = new List<StatusAbility>();
        public Dictionary<int, List<StatusAbility>> statusDict = new Dictionary<int, List<StatusAbility>>();


        public StatusAbility AttachStatus(int statusId)
        {
            StatusConfigObject statusConfigObject = AssetManager.Instance.LoadRes<StatusConfigObject>(CombatContext.Instance.loader, $"Status_{statusId}");
            
            if (statusConfigObject == null)
            {
                return null;
            }

            var status = Combat.AttachAbility<StatusAbility>(statusConfigObject);
            if (!statusDict.ContainsKey(status.statusConfig.Id))
            {
                statusDict.Add(status.statusConfig.Id, new List<StatusAbility>());
            }
            statusDict[status.statusConfig.Id].Add(status);
            statusList.Add(status);
            return status;
        }

        public StatusAbility GetStatus(int statusId, int index = 0)
        {
            if (HasStatus(statusId))
            {
                return statusDict[statusId][index];
            }
            return null;
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

        public bool HasStatus(int statusId)
        {
            return statusDict.ContainsKey(statusId);
        }




        public void OnStatusesChanged(StatusAbility statusAbility)
        {
            var tempActionControl = ActionControlType.None;
            foreach (var item in Combat.Children.Values)
            {
                if (item is StatusAbility status)
                {
                    if (!status.Enable)
                    {
                        continue;
                    }
                    foreach (var effect in status.GetComponent<AbilityEffectComponent>().abilityEffectList)
                    {
                        var actionControlComponent = effect.GetComponent<AbilityEffectActionControlComponent>();
                        if (actionControlComponent != null)
                        {
                            tempActionControl = tempActionControl | actionControlComponent.ActionControlEffect.ActionControlType;
                        }

                    }
                }
            }

            Combat.actionControlType = tempActionControl;
            var moveForbid = Combat.actionControlType.HasFlag(ActionControlType.MoveForbid);
            Combat.GetComponent<MotionComponent>().SetEnable(!moveForbid);
        }
    }
}