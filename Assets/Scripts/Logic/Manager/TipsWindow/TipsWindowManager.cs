using System;
using UnityEngine;

namespace LccModel
{
    public class TipsWindowManager : Singleton<TipsWindowManager>
    {
        [Header("TipsWindow对象池")]
        public TipsWindowPool tipsWindowPool;
        public void InitManager(TipsWindowPool tipsWindowPool)
        {
            this.tipsWindowPool = tipsWindowPool;
            tipsWindowPool.InitPool();
        }

        public TipsWindow CreateTipsWindow(string title, string info, Action<bool> callback, string confirm = "确定", string cancel = "取消", Transform parent = null)
        {
            TipsWindow tipsWindow = tipsWindowPool.Dequeue();
            tipsWindow.InitTipsWindow(title, info, callback, confirm, cancel, parent);
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