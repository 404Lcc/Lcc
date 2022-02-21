using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class TipsWindowManager : Singleton<TipsWindowManager>
    {
        [Header("场景中的TipsWindow")]
        public Hashtable tipsWindows = new Hashtable();
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
            tipsWindow.InitTipsWindow(IdUtil.Generate(), title, info, callback, confirm, cancel, parent);
            tipsWindows.Add(tipsWindow.id, tipsWindow);
            return tipsWindow;
        }
        public void ClearTipsWindow(long id)
        {
            TipsWindow tipsWindow = GetTipsWindow(id);
            if (tipsWindow == null) return;
            tipsWindowPool.Enqueue(tipsWindow);
            tipsWindows.Remove(id);
        }
        public void ClearAllTipsWindows()
        {
            List<int> idList = new List<int>();
            foreach (object item in tipsWindows.Keys)
            {
                idList.Add((int)item);
            }
            foreach (int item in idList)
            {
                TipsWindow tipsWindow = GetTipsWindow(item);
                if (tipsWindow == null) return;
                tipsWindowPool.Enqueue(tipsWindow);
                tipsWindows.Remove(item);
            }
        }
        public TipsWindow GetTipsWindow(long id)
        {
            TipsWindow tipsWindow = (TipsWindow)tipsWindows[id];
            if (tipsWindow == null) return null;
            return tipsWindow;
        }
    }
}