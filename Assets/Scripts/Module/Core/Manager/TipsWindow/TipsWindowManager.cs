using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class TipsWindowManager : Singleton<TipsWindowManager>
    {
        [Header("地图中的TipsWindow")]
        public Hashtable tipsWindows = new Hashtable();
        [Header("TipsWindow对象池")]
        public TipsWindowPool tipsWindowPool;
        public int tipsWindowId;
        public void InitManager(TipsWindowPool tipsWindowPool)
        {
            this.tipsWindowPool = tipsWindowPool;
            tipsWindowPool.InitPool();
        }

        public TipsWindow CreateTipsWindow(string title, string info, string confirm, string cancel, Transform parent = null)
        {
            TipsWindow tipsWindow = tipsWindowPool.Dequeue();
            tipsWindow.InitTipsWindow(IdUtil.Generate(), true, title, info, confirm, cancel, parent);
            tipsWindows.Add(tipsWindow.id, tipsWindow);
            return tipsWindow;
        }
        public TipsWindow GetTipsWindow(int id)
        {
            TipsWindow tipsWindow = tipsWindows[id] as TipsWindow;
            if (tipsWindow == null) return null;
            return tipsWindow;
        }
        public void ClearTipsWindow(int id)
        {
            TipsWindow tipsWindow = GetTipsWindow(id);
            if (tipsWindow == null) return;
            tipsWindowPool.Enqueue(tipsWindow);
            tipsWindows.Remove(id);
        }
        public void ClearAllTipsWindow()
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
    }
}