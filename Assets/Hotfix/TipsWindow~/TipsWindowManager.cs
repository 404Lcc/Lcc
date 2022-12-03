using System;
using UnityEngine;

namespace LccModel
{
    public class TipsWindowManager : AObjectBase
    {
        [Header("TipsWindow对象池")]
        public TipsWindowPool tipsWindowPool;
        public static TipsWindowManager Instance { get; set; }

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
        public void InitManager(TipsWindowPool tipsWindowPool)
        {
            this.tipsWindowPool = tipsWindowPool;
            tipsWindowPool.InitPool();
        }

        public TipsWindow CreateTipsWindow(string title, string info, Action<bool> completed, string confirm = "确定", string cancel = "取消", Transform parent = null)
        {
            TipsWindow tipsWindow = tipsWindowPool.Dequeue();
            tipsWindow.InitTipsWindow(title, info, completed, confirm, cancel, parent);
            return tipsWindow;
        }
        public void ClearTipsWindow(long id)
        {
            GameObjectEntity gameObjectEntity = GetChildren<GameObjectEntity>(id);
            if (gameObjectEntity == null) return;
            tipsWindowPool.Enqueue(gameObjectEntity.GetComponent<TipsWindow>());
        }
    }
}