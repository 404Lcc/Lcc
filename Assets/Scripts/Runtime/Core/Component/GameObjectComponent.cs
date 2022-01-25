using UnityEngine;

namespace LccModel
{
    public class GameObjectComponent : AObjectBase
    {
        public GameObject gameObject;
        public override void Awake<P1>(P1 p1)
        {
            gameObject = p1 as GameObject;
        }
        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            base.Dispose();
            gameObject.SafeDestroy();
        }
    }
}