using UnityEngine;

namespace LccModel
{
    public class AStarNodeData
    {
        public bool isWall;
        public Vector3 localPosition;
        public int x;
        public int y;
        public int gCost;
        public int hCost;
        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }
        public AStarNodeData parent;
        //g代表与开始节点距离
        //h代表与end节点的距离
        //f = g + h
        public AStarNodeData(bool isWall, Vector3 localPosition, int x, int y)
        {
            this.isWall = isWall;
            this.localPosition = localPosition;
            this.x = x;
            this.y = y;
        }
    }
}