using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class TipsManager : MonoBehaviour
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

        public Tips OpenTips(string info, Vector2 localPosition, Vector2 offset, float duration, Transform parent = null)
        {
            Tips tips = tipsPool.Dequeue();
            tips.InitTips(info, localPosition, offset, duration, parent);
            GenerateID(tips);
            tipss.Add(tips.id, tips);
            return tips;
        }
        public Tips GetTips(int id)
        {
            Tips tips = tipss[id] as Tips;
            if (tips == null) return null;
            return tips;
        }
        public void ClearTips(int id)
        {
            Tips tips = GetTips(id);
            if (tips == null) return;
            tipsPool.Enqueue(tips);
            tipss.Remove(id);
        }
        public void ClearAllTipsWindow()
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
        public void GenerateID(Tips tips)
        {
            if (tipss.Count == 0)
            {
                tipsId++;
                tips.id = tipsId;
                return;
            }
            for (int i = 1; i <= tipss.Count; i++)
            {
                if (!tipss.ContainsKey(i))
                {
                    tips.id = i;
                    return;
                }
            }
            tipsId++;
            tips.id = tipsId;
        }
    }
}