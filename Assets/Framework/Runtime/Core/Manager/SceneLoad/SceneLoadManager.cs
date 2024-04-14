using ET;
using UnityEngine.SceneManagement;

namespace LccModel
{
    public class SceneLoadManager : AObjectBase
    {
        public static SceneLoadManager Instance { get; set; }

        public override void Awake()
        {
            base.Awake();


            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
        }
    }
}