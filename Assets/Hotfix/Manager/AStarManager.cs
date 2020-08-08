using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class AStarManager : MonoBehaviour
    {
        public float noderadius;
        public LayerMask layer;
        public bool bshowwall;
        public bool bshowpath;
        private int w;
        private int h;
        private AStarNode[,] nodes;

        private Transform walls;
        private Transform paths;
        public void InitManager(float noderadius, LayerMask layer, bool bshowwall, bool bshowpath, GameObject map, float ratio = 1)
        {
            this.noderadius = noderadius;
            this.layer = layer;
            this.bshowwall = bshowwall;
            this.bshowpath = bshowpath;

            w = Mathf.RoundToInt(GameUtil.GetComponent<BoxCollider2D>(map).bounds.size.x * ratio);
            h = Mathf.RoundToInt(GameUtil.GetComponent<BoxCollider2D>(map).bounds.size.y * ratio);
            nodes = new AStarNode[w * 2, h * 2];
            walls = new GameObject("Walls").transform;
            paths = new GameObject("Paths").transform;
            //将墙的信息写入格子中
            for (int x = 0; x <= w; x++)
            {
                for (int y = 0; y <= h; y++)
                {
                    Vector3 pos = new Vector3(x * 0.5f, y * 0.5f, 0);
                    //通过节点中心发射圆形射线,检测当前位置是否可以行走
                    bool bwall = false;
                    //bwall = Physics.CheckSphere(pos, noderadius, layer); 3D
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, noderadius, layer);
                    if (colliders.Length > 0)
                    {
                        bwall = true;
                    }
                    nodes[x, y] = new AStarNode(bwall, pos, x, y);
                    if (bwall)
                    {
                        //InitWallGrid(Model.IO.assetManager.LoadGameObject("Wall", false, walls, true, AssetType.UI, AssetType.Tool), pos, bshowwall);
                    }
                }
            }
            for (int x = -w + 1; x < 0; x++)
            {
                for (int y = 0; y <= h; y++)
                {
                    Vector3 pos = new Vector3(x * 0.5f, y * 0.5f, 0);
                    bool bwall = false;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, noderadius, layer);
                    if (colliders.Length > 0)
                    {
                        bwall = true;
                    }
                    nodes[w - x, y] = new AStarNode(bwall, pos, x, y);
                    if (bwall)
                    {
                        //InitWallGrid(Model.IO.assetManager.LoadGameObject("Wall", false, walls, true, AssetType.UI, AssetType.Tool), pos, bshowwall);
                    }
                }
            }
            for (int x = 0; x <= w; x++)
            {
                for (int y = -h + 1; y < 0; y++)
                {
                    Vector3 pos = new Vector3(x * 0.5f, y * 0.5f, 0);
                    bool bwall = false;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, noderadius, layer);
                    if (colliders.Length > 0)
                    {
                        bwall = true;
                    }
                    nodes[x, h - y] = new AStarNode(bwall, pos, x, y);
                    if (bwall)
                    {
                        //InitWallGrid(Model.IO.assetManager.LoadGameObject("Wall", false, walls, true, AssetType.UI, AssetType.Tool), pos, bshowwall);
                    }
                }
            }
            for (int x = -w + 1; x < 0; x++)
            {
                for (int y = -h + 1; y < 0; y++)
                {
                    Vector3 pos = new Vector3(x * 0.5f, y * 0.5f, 0);
                    bool bwall = false;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, noderadius, layer);
                    if (colliders.Length > 0)
                    {
                        bwall = true;
                    }
                    nodes[w - x, h - y] = new AStarNode(bwall, pos, x, y);
                    if (bwall)
                    {
                        //InitWallGrid(Model.IO.assetManager.LoadGameObject("Wall", false, walls, true, AssetType.UI, AssetType.Tool), pos, bshowwall);
                    }
                }
            }
        }
        public GameObject InitWallGrid(GameObject nodewall, Vector3 pos, bool bshowwall)
        {
            nodewall.transform.localPosition = pos;
            nodewall.SetActive(bshowwall);
            return nodewall;
        }
        public GameObject InitPathGrid(GameObject nodepath, Vector3 pos, bool bshowpath)
        {
            nodepath.transform.localPosition = pos;
            nodepath.SetActive(bshowwall);
            return nodepath;
        }
        /// <summary>
        /// 根据坐标获得一个节点
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public AStarNode GetNode(Vector3 pos)
        {
            int x = Mathf.RoundToInt(pos.x * 2);
            int y = Mathf.RoundToInt(pos.y * 2);
            if (x < 0)
            {
                x = w - x;
                x = Mathf.Clamp(x, w, w * 2 - 1);
            }
            else
            {
                x = Mathf.Clamp(x, 0, w);
            }
            if (y < 0)
            {
                y = h - y;
                y = Mathf.Clamp(y, h, h * 2 - 1);
            }
            else
            {
                y = Mathf.Clamp(y, 0, h);
            }
            return nodes[x, y];
        }
        /// <summary>
        /// 取得周围的节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<AStarNode> GetNodeAround(AStarNode node)
        {
            List<AStarNode> list = new List<AStarNode>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    //自己
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    int x = node.x + i;
                    int y = node.y + j;
                    if (x < 0)
                    {
                        x = w - x;
                        if (!(x < w * 2 && x > w))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!(x <= w && x >= 0))
                        {
                            continue;
                        }
                    }
                    if (y < 0)
                    {
                        y = h - y;
                        if (!(y < h * 2 && y > h))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!(y <= h && y >= 0))
                        {
                            continue;
                        }
                    }
                    list.Add(nodes[x, y]);
                }
            }
            return list;
        }
        /// <summary>
        /// 获取两个节点之间的距离
        /// </summary>
        /// <param name="startnode"></param>
        /// <param name="endnode"></param>
        /// <returns></returns>
        public int GetNodeDistance(AStarNode startnode, AStarNode endnode)
        {
            return Diagonal(startnode, endnode);
        }
        /// <summary>
        /// 对角线估价法
        /// </summary>
        /// <param name="startnode"></param>
        /// <param name="endnode"></param>
        /// <param name="diagcost"></param>
        /// <param name="straightcost"></param>
        /// <returns></returns>
        public int Diagonal(AStarNode startnode, AStarNode endnode, int diagcost = 14, int straightcost = 10)
        {
            int dx = Mathf.Abs(startnode.x - endnode.x);
            int dy = Mathf.Abs(startnode.y - endnode.y);
            int diag = Mathf.Min(dx, dy);
            int straight = dx + dy;
            return diagcost * diag + straightcost * (straight - 2 * diag);
        }
        /// <summary>
        /// 更新路径
        /// </summary>
        /// <param name="nodes"></param>
        public void UpdatePath(List<AStarNode> nodes)
        {
            foreach (Transform item in paths)
            {
                GameUtil.SafeDestroy(item.gameObject);
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                //InitWallGrid(Model.IO.assetManager.LoadGameObject("Path", false, paths, true, AssetType.UI, AssetType.Tool), nodes[i].pos, bshowpath);
            }
        }
        /// <summary>
        /// 生成路径
        /// </summary>
        /// <param name="startnode"></param>
        /// <param name="endnode"></param>
        /// <returns></returns>
        public List<AStarNode> GeneratePath(AStarNode startnode, AStarNode endnode)
        {
            List<AStarNode> path = new List<AStarNode>();
            if (endnode != null)
            {
                AStarNode temp = endnode;
                while (temp != startnode)
                {
                    path.Add(temp);
                    temp = temp.parent;
                }
                path.Reverse();
            }
            UpdatePath(path);
            return path;
        }
        public List<AStarNode> AStarFindPath(Vector3 start, Vector3 end)
        {
            AStarNode startnode = GetNode(start);
            AStarNode endnode = GetNode(end);
            List<AStarNode> openlist = new List<AStarNode>();//等待检查列表
            HashSet<AStarNode> closelist = new HashSet<AStarNode>();//检查完成列表
            openlist.Add(startnode);
            while (openlist.Count > 0)
            {
                AStarNode curnode = openlist[0];
                for (int i = 0; i < openlist.Count; i++)
                {
                    //等待检查列表里最短距离的节点
                    if (openlist[i].fcost <= curnode.fcost && openlist[i].hcost < curnode.hcost)
                    {
                        curnode = openlist[i];
                    }
                }
                openlist.Remove(curnode);
                closelist.Add(curnode);
                if (curnode == endnode)
                {
                    return GeneratePath(startnode, endnode);
                }
                foreach (AStarNode item in GetNodeAround(curnode))
                {
                    if (item.bwall || closelist.Contains(item))
                    {
                        continue;
                    }
                    //计算与开始节点的距离
                    int gcost = curnode.gcost + GetNodeDistance(curnode, item);
                    //如果不在等待检查列表中或者与开始节点的距离更小
                    if (!openlist.Contains(item) || gcost < item.gcost)
                    {
                        //更新与开始节点的距离
                        item.gcost = gcost;
                        //更新与end节点的距离
                        item.hcost = GetNodeDistance(item, endnode);
                        //更新父节点为当前选定的节点
                        //在考查从一个节点移动到另一个节点时,总是拿自身节点周围的8个相邻节点来说事儿,相对于周边的节点来讲,自身节点称为它们的父节点.
                        item.parent = curnode;
                        if (!openlist.Contains(item))
                        {
                            openlist.Add(item);
                        }
                    }
                }
            }
            return GeneratePath(startnode, null);
        }
    }
}