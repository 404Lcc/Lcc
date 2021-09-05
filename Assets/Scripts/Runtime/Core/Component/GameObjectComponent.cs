using UnityEngine;

namespace LccModel
{
    public class GameObjectComponent : AObjectBase
    {
        public GameObject gameObject;
        public override void Awake<P1>(P1 p1)
        {
            gameObject = p1 as GameObject;
            ShowView(gameObject);
        }
        public override void Awake<P1, P2>(P1 p1, P2 p2)
        {
            gameObject = p1 as GameObject;
            ShowView(gameObject, p2 as GameObject);
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