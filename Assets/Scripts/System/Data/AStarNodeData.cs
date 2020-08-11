using UnityEngine;

namespace Model
{
    public class AStarNodeData
    {
        public bool bwall;
        public Vector3 pos;
        public int x;
        public int y;
        public int gcost;
        public int hcost;
        public int fcost
        {
            get { return gcost + hcost; }
        }
        public AStarNodeData parent;
        //g代表与开始节点距离
        //h代表与end节点的距离
        //f = g + h
        public AStarNodeData(bool bwall, Vector3 pos, int x, int y)
        {
            this.bwall = bwall;
            this.pos = pos;
            this.x = x;
            this.y = y;
        }
    }
}