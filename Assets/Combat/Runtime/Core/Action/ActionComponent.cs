using System;

namespace LccModel
{
    public class ActionComponent : Component
    {
        private Type _actionType;

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            _actionType = p1 as Type;
        }
    }
}