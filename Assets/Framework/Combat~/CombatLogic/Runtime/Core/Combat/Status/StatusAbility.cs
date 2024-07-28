using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LccModel
{
    public partial class StatusAbility : Entity, IAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();
        public Combat Creator;




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
            this.paramsDict = (Dictionary<string, string>)Clone(paramsDict);
            this.paramsDict.Add("自身生命值", Owner.GetComponent<AttributeComponent>().HealthPoint.Value.ToString());
            this.paramsDict.Add("自身攻击力", Owner.GetComponent<AttributeComponent>().Attack.Value.ToString());
        }
        public object Clone(object obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, obj);
            memoryStream.Position = 0;
            return formatter.Deserialize(memoryStream);
        }
        public void ActivateAbility()
        {
            Enable = true;
            GetComponent<AbilityEffectComponent>().EnableEffect();
            if (statusConfig.EnableChildStatus)
            {
                foreach (var childStatusData in statusConfig.StatusList)
                {
                    var status = Owner.AttachStatus(childStatusData.StatusConfigObject.Id);
                    status.Creator = Creator;
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

            Owner.OnStatusRemove(this);

            Dispose();
        }


        public Entity CreateExecution()
        {
            return null;
        }
    }
}