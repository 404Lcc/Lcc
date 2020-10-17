using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class AStarManager : Singleton<AStarManager>
    {
        public float radius;
        public LayerMask layer;
        public bool isShowWall;
        public bool isShowPath;
        public int w;
        public int h;
        public AStarNodeData[,] aStarNodeDatas;

        public Transform wall;
        public Transform path;
        public void InitManager(float radius, LayerMask layer, bool isShowWall, bool isShowPath, GameObject gameObject, float ratio = 1)
        {
            this.radius = radius;
            this.layer = layer;
            this.isShowWall = isShowWall;
            this.isShowPath = isShowPath;

            w = Mathf.RoundToInt(gameObject.GetComponent<BoxCollider2D>().bounds.size.x * ratio);
            h = Mathf.RoundToInt(gameObject.GetComponent<BoxCollider2D>().bounds.size.y * ratio);
            aStarNodeDatas = new AStarNodeData[w * 2, h * 2];
            wall = new GameObject("Wall").transform;
            path = new GameObject("Path").transform;
            //将墙的信息写入格子中
            for (int x = 0; x <= w; x++)
            {
                for (int y = 0; y <= h; y++)
                {
                    Vector3 localPosition = new Vector3(x * 0.5f, y * 0.5f, 0);
                    //通过节点中心发射圆形射线,检测当前位置是否可以行走
                    bool wall = false;
                    //bwall = Physics.CheckSphere(localPosition, nodeRadius, layer); 3D
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(localPosition, radius, layer);
                    if (colliders.Length > 0)
                    {
                        wall = true;
                    }
                    aStarNodeDatas[x, y] = new AStarNodeData(wall, localPosition, x, y);
                    if (wall)
                    {
                        //InitWallGrid(AssetManager.Instance.LoadGameObject("Wall", false, false, this.wall, AssetType.UI, AssetType.Tool), localPosition, isShowWall);
                    }
                }
            }
            for (int x = -w + 1; x < 0; x++)
            {
                for (int y = 0; y <= h; y++)
                {
                    Vector3 localPosition = new Vector3(x * 0.5f, y * 0.5f, 0);
                    bool wall = false;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(localPosition, radius, layer);
                    if (colliders.Length > 0)
                    {
                        wall = true;
                    }
                    aStarNodeDatas[w - x, y] = new AStarNodeData(wall, localPosition, x, y);
                    if (wall)
                    {
                        //InitWallGrid(AssetManager.Instance.LoadGameObject("Wall", false, false, this.wall, AssetType.UI, AssetType.Tool), localPosition, isShowWall);
                    }
                }
            }
            for (int x = 0; x <= w; x++)
            {
                for (int y = -h + 1; y < 0; y++)
                {
                    Vector3 localPosition = new Vector3(x * 0.5f, y * 0.5f, 0);
                    bool wall = false;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(localPosition, radius, layer);
                    if (colliders.Length > 0)
                    {
                        wall = true;
                    }
                    aStarNodeDatas[x, h - y] = new AStarNodeData(wall, localPosition, x, y);
                    if (wall)
                    {
                        //InitWallGrid(AssetManager.Instance.LoadGameObject("Wall", false, false, this.wall, AssetType.UI, AssetType.Tool), localPosition, isShowWall);
                    }
                }
            }
            for (int x = -w + 1; x < 0; x++)
            {
                for (int y = -h + 1; y < 0; y++)
                {
                    Vector3 localPosition = new Vector3(x * 0.5f, y * 0.5f, 0);
                    bool wall = false;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(localPosition, radius, layer);
                    if (colliders.Length > 0)
                    {
                        wall = true;
                    }
                    aStarNodeDatas[w - x, h - y] = new AStarNodeData(wall, localPosition, x, y);
                    if (wall)
                    {
                        //InitWallGrid(AssetManager.Instance.LoadGameObject("Wall", false, false, this.wall, AssetType.UI, AssetType.Tool), localPosition, isShowWall);
                    }
                }
            }
        }
        public GameObject InitWallGrid(GameObject wall, Vector3 localPosition, bool isShowWall)
        {
            wall.transform.localPosition = localPosition;
            wall.SetActive(isShowWall);
            return wall;
        }
        public GameObject InitPathGrid(GameObject path, Vector3 localPosition, bool isShowPath)
        {
            path.transform.localPosition = localPosition;
            path.SetActive(isShowPath);
            return path;
        }
        /// <summary>
        /// 根据坐标获得一个节点
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public AStarNodeData GetAStarNodeData(Vector3 localPosition)
        {
            int x = Mathf.RoundToInt(localPosition.x * 2);
            int y = Mathf.RoundToInt(localPosition.y * 2);
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
            return aStarNodeDatas[x, y];
        }
        /// <summary>
        /// 取得周围的节点
        /// </summary>
        /// <param name="aStarNodeData"></param>
        /// <returns></returns>
        public List<AStarNodeData> GetAStarNodeDataAround(AStarNodeData data)
        {
            List<AStarNodeData> dataList = new List<AStarNodeData>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    //自己
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    int x = data.x + i;
                    int y = data.y + j;
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
                    dataList.Add(aStarNodeDatas[x, y]);
                }
            }
            return dataList;
        }
        /// <summary>
        /// 获取两个节点之间的距离
        /// </summary>
        /// <param name="startAStarNodeData"></param>
        /// <param name="endAStarNodeData"></param>
        /// <returns></returns>
        public int GetAStarNodeDataDistance(AStarNodeData startAStarNodeData, AStarNodeData endAStarNodeData)
        {
            return Diagonal(startAStarNodeData, endAStarNodeData);
        }
        /// <summary>
        /// 对角线估价法
        /// </summary>
        /// <param name="startAStarNodeData"></param>
        /// <param name="endAStarNodeData"></param>
        /// <param name="diagCost"></param>
        /// <param name="straightCost"></param>
        /// <returns></returns>
        public int Diagonal(AStarNodeData startAStarNodeData, AStarNodeData endAStarNodeData, int diagCost = 14, int straightCost = 10)
        {
            int dx = Mathf.Abs(startAStarNodeData.x - endAStarNodeData.x);
            int dy = Mathf.Abs(startAStarNodeData.y - endAStarNodeData.y);
            int diag = Mathf.Min(dx, dy);
            int straight = dx + dy;
            return diagCost * diag + straightCost * (straight - 2 * diag);
        }
        /// <summary>
        /// 更新路径
        /// </summary>
        /// <param name="endAStarNodeDataList"></param>
        public void UpdatePath(List<AStarNodeData> endAStarNodeDataList)
        {
            foreach (Transform item in path)
            {
                item.SafeDestroy();
            }
            for (int i = 0; i < endAStarNodeDataList.Count; i++)
            {
                //InitWallGrid(AssetManager.Instance.LoadGameObject("Path", false, false, path, AssetType.UI, AssetType.Tool), endAStarNodeDataList[i].localPosition, isShowPath);
            }
        }
        /// <summary>
        /// 生成路径
        /// </summary>
        /// <param name="startAStarNodeData"></param>
        /// <param name="endAStarNodeData"></param>
        /// <returns></returns>
        public List<AStarNodeData> GeneratePath(AStarNodeData startAStarNodeData, AStarNodeData endAStarNodeData)
        {
            List<AStarNodeData> pathList = new List<AStarNodeData>();
            if (endAStarNodeData != null)
            {
                AStarNodeData temp = endAStarNodeData;
                while (temp != startAStarNodeData)
                {
                    pathList.Add(temp);
                    temp = temp.parent;
                }
                pathList.Reverse();
            }
            UpdatePath(pathList);
            return pathList;
        }
        public List<AStarNodeData> AStarFindPath(Vector3 start, Vector3 end)
        {
            AStarNodeData startAStarNodeData = GetAStarNodeData(start);
            AStarNodeData endAStarNodeData = GetAStarNodeData(end);
            List<AStarNodeData> openList = new List<AStarNodeData>();//等待检查列表
            HashSet<AStarNodeData> closeList = new HashSet<AStarNodeData>();//检查完成列表
            openList.Add(startAStarNodeData);
            while (openList.Count > 0)
            {
                AStarNodeData current = openList[0];
                for (int i = 0; i < openList.Count; i++)
                {
                    //等待检查列表里最短距离的节点
                    if (openList[i].fCost <= current.fCost && openList[i].hCost < current.hCost)
                    {
                        current = openList[i];
                    }
                }
                openList.Remove(current);
                closeList.Add(current);
                if (current == endAStarNodeData)
                {
                    return GeneratePath(startAStarNodeData, endAStarNodeData);
                }
                foreach (AStarNodeData item in GetAStarNodeDataAround(current))
                {
                    if (item.isWall || closeList.Contains(item))
                    {
                        continue;
                    }
                    //计算与开始节点的距离
                    int gCost = current.gCost + GetAStarNodeDataDistance(current, item);
                    //如果不在等待检查列表中或者与开始节点的距离更小
                    if (!openList.Contains(item) || gCost < item.gCost)
                    {
                        //更新与开始节点的距离
                        item.gCost = gCost;
                        //更新与end节点的距离
                        item.hCost = GetAStarNodeDataDistance(item, endAStarNodeData);
                        //更新父节点为当前选定的节点
                        //在考查从一个节点移动到另一个节点时,总是拿自身节点周围的8个相邻节点来说事儿,相对于周边的节点来讲,自身节点称为它们的父节点.
                        item.parent = current;
                        if (!openList.Contains(item))
                        {
                            openList.Add(item);
                        }
                    }
                }
            }
            return GeneratePath(startAStarNodeData, null);
        }
    }
}