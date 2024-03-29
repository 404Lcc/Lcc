﻿using System.Collections.Generic;

namespace LccModel
{
    public partial class ItemAbility : Entity, IAbility
    {
        public bool Enable { get; set; }
        public Combat Owner => GetParent<Combat>();




        public ItemConfigObject itemConfigObject;
        private List<StatusAbility> _statusList = new List<StatusAbility>();

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            itemConfigObject = p1 as ItemConfigObject;

            AddComponent<AbilityEffectComponent, List<Effect>>(itemConfigObject.EffectList);
        }


        public void ActivateAbility()
        {
            Enable = true;

            if (itemConfigObject.EnableChildStatus)
            {
                foreach (var item in itemConfigObject.StatusList)
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

            if (itemConfigObject.EnableChildStatus)
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