using System.Collections.Generic;

namespace LccModel
{
    public class SkillAbility : Entity, IAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();




        public SkillConfigObject skillConfigObject;
        public ExecutionConfigObject executionConfigObject;
        public bool spelling;

        private List<StatusAbility> _statusList = new List<StatusAbility>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            skillConfigObject = p1 as SkillConfigObject;
            AddComponent<AbilityEffectComponent, List<Effect>>(skillConfigObject.EffectList);

            executionConfigObject = AssetManager.Instance.LoadRes<ExecutionConfigObject>(CombatContext.Instance.loader, $"Execution_{skillConfigObject.Id}");
            
        }





        public void ActivateAbility()
        {
            Enable = true;

            if (skillConfigObject.EnableChildStatus)
            {
                foreach (var item in skillConfigObject.StatusList)
                {
                    var status = Owner.AttachStatus(item.StatusConfigObject.Id);
                    status.Creator = Owner;
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
            var execution = Owner.AddChildren<SkillExecution, SkillAbility>(this);
            execution.executionConfigObject = executionConfigObject;
            execution.LoadExecutionEffect();
            return execution;
        }
    }
}