using System.Collections;
using UnityEngine;

namespace Model
{
    public class TipsWindowManager : MonoBehaviour
    {
        [Header("地图中的TipsWindow")]
        public Hashtable tipswindows = new Hashtable();
        [Header("TipsWindow对象池")]
        public TipsWindowPool tipswindowpool;
        public int tipswindowid;
        public void InitManager(TipsWindowPool tipswindowpool)
        {
            this.tipswindowpool = tipswindowpool;
            tipswindowpool.InitPool();
        }

        public TipsWindow CreateTipsWindow(string title, string information, string confirm, string cancel, Transform parent = null)
        {
            TipsWindow tipswindow = tipswindowpool.Dequeue();
            tipswindow.InitTipsWindow(title, information, confirm, cancel, parent);
            GenerateID(tipswindow);
            tipswindows.Add(tipswindow.id, tipswindow);
            return tipswindow;
        }
        public TipsWindow GetTipsWindow(int id)
        {
            TipsWindow tipswindow = tipswindows[id] as TipsWindow;
            if (tipswindow == null) return null;
            return tipswindow;
        }
        public void DeleteTipsWindow(int id)
        {
            TipsWindow tipswindow = GetTipsWindow(id);
            if (tipswindow == null) return;
            tipswindowpool.Enqueue(tipswindow);
            tipswindows.Remove(id);
        }
        public void GenerateID(TipsWindow tipswindow)
        {
            if (tipswindows.Count == 0)
            {
                tipswindowid++;
                tipswindow.id = tipswindowid;
                return;
            }
            for (int i = 1; i <= tipswindows.Count; i++)
            {
                if (!tipswindows.ContainsKey(i))
                {
                    tipswindow.id = i;
                    return;
                }
            }
            tipswindowid++;
            tipswindow.id = tipswindowid;
        }
    }
}