using UnityEngine;

namespace LccModel
{
    public class TipsManager : AObjectBase
    {
        [Header("Tips对象池")]
        public TipsPool tipsPool;

        public static TipsManager Instance { get; set; }

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
        public void InitManager(TipsPool tipsPool)
        {
            this.tipsPool = tipsPool;
            tipsPool.InitPool();
        }

        public Tips CreateTips(string info, Vector2 localPosition, Vector2 offset, float duration, Transform parent = null)
        {
            Tips tips = tipsPool.Dequeue();
            tips.InitTips(info, localPosition, offset, duration, parent);
            return tips;
        }
        public void ClearTips(long id)
        {
            GameObjectEntity gameObjectEntity = GetChildren<GameObjectEntity>(id);
            if (gameObjectEntity == null) return;
            tipsPool.Enqueue(gameObjectEntity.GetComponent<Tips>());
        }
    }
}