using System.Collections;
using UnityEngine;

namespace Model
{
    public class TipsManager : MonoBehaviour
    {
        [Header("地图中的Tips")]
        public Hashtable tipss = new Hashtable();
        [Header("Tips对象池")]
        public TipsPool tipspool;
        public int tipsid;
        public void InitManager(TipsPool tipspool)
        {
            this.tipspool = tipspool;
            tipspool.InitPool();
        }

        public Tips CreateTips(string information, Vector2 position, Vector2 offset, float duration, Transform parent = null)
        {
            Tips tips = tipspool.Dequeue();
            tips.InitTips(information, position, offset, duration, parent);
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
        public void DeleteTips(int id)
        {
            Tips tips = GetTips(id);
            if (tips == null) return;
            tipspool.Enqueue(tips);
            tipss.Remove(id);
        }
        public void GenerateID(Tips tips)
        {
            if (tipss.Count == 0)
            {
                tipsid++;
                tips.id = tipsid;
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
            tipsid++;
            tips.id = tipsid;
        }
    }
}