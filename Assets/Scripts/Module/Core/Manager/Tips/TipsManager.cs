using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class TipsManager : Singleton<TipsManager>
    {
        [Header("地图中的Tips")]
        public Hashtable tipss = new Hashtable();
        [Header("Tips对象池")]
        public TipsPool tipsPool;
        public int tipsId;
        public void InitManager(TipsPool tipsPool)
        {
            this.tipsPool = tipsPool;
            tipsPool.InitPool();
        }

        public Tips CreateTips(string info, Vector2 localPosition, Vector2 offset, float duration, Transform parent = null)
        {
            Tips tips = tipsPool.Dequeue();
            tips.InitTips(IdUtil.Generate(), info, localPosition, offset, duration, parent);
            tipss.Add(tips.id, tips);
            return tips;
        }
        public void ClearTips(int id)
        {
            Tips tips = GetTips(id);
            if (tips == null) return;
            tipsPool.Enqueue(tips);
            tipss.Remove(id);
        }
        public void ClearAllTipss()
        {
            List<int> idList = new List<int>();
            foreach (object item in tipss.Keys)
            {
                idList.Add((int)item);
            }
            foreach (int item in idList)
            {
                Tips tips = GetTips(item);
                if (tips == null) return;
                tipsPool.Enqueue(tips);
                tipss.Remove(item);
            }
        }
        public Tips GetTips(int id)
        {
            Tips tips = (Tips)tipss[id];
            if (tips == null) return null;
            return tips;
        }
    }
}