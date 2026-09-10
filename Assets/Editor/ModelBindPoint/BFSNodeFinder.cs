using UnityEngine;
using System.Collections.Generic;

namespace LccEditor.ModelBindPoint
{
    public static class BFSBindPointFinder
    {
        public static SearchMode searchMode = SearchMode.BreadthFirst;

        public enum SearchMode
        {
            BreadthFirst,
            DepthFirst
        }

        public static Transform foundNode;
        public static int searchSteps = 0;
        public static float searchTime = 0f;
        public static string Search(Transform searchRoot, string targetName)
        {
            if (string.IsNullOrEmpty(targetName))
            {
                Debug.LogWarning("请输入目标节点名称");
                return "";
            }

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            if (searchRoot == null)
                return "";
            Transform startNode = searchRoot;
            
            foundNode = searchMode == SearchMode.BreadthFirst ? BFS(startNode, targetName) : DFS(startNode, targetName);

            stopwatch.Stop();
            searchTime = (float)stopwatch.Elapsed.TotalMilliseconds;

            if (foundNode != null)
            {
                Debug.Log($"{searchMode} 找到节点: {foundNode.name} (搜索步数: {searchSteps}, 耗时: {searchTime}ms)");
                string path = foundNode.name;
                GetPath(foundNode, ref path);
                return path;
            }
            else
            {
                Debug.LogWarning($"{searchMode} 未找到节点: {targetName}");
                return "";
            }
        }

        // 广度优先搜索
        private static Transform BFS(Transform root, string targetName)
        {
            searchSteps = 0;
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(root);

            Transform lastNode = null;
            
            while (queue.Count > 0)
            {
                Transform current = queue.Dequeue();
                searchSteps++;

                if (current.name == targetName)
                {
                    if (lastNode != null)
                    {
                        string lastPath = targetName;
                        string curPath = targetName;
                        GetPath(lastNode, ref lastPath);
                        GetPath(current, ref curPath);
                        Debug.LogWarning($"挂点有同名但不同路径，保留先找到的：{lastPath}，忽略：{curPath}");
                    }
                    else
                    {
                        lastNode = current;
                    }
                }

                foreach (Transform child in current)
                {
                    queue.Enqueue(child);
                }
            }

            return lastNode;
        }
        
        public static void GetPath(Transform node, ref string path)
        {
            if (node.transform.parent == null)
                return;
            if (node.transform.parent.parent == null)
                return;

            var parent = node.transform.parent;
            path = parent.name + "/" + path;
        
            GetPath(parent, ref path);
        }

        // 深度优先搜索
        private static Transform DFS(Transform root, string targetName)
        {
            searchSteps = 0;
            return DFSRecursive(root, targetName);
        }

        private static Transform DFSRecursive(Transform node, string targetName)
        {
            searchSteps++;

            if (node.name == targetName)
                return node;

            foreach (Transform child in node)
            {
                Transform result = DFSRecursive(child, targetName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}