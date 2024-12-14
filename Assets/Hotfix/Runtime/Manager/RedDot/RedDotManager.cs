using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class RedDotManager : Module
    {
        public static RedDotManager Instance => Entry.GetModule<RedDotManager>();

        public Dictionary<string, List<string>> parentDict = new Dictionary<string, List<string>>();//key父节点 value子节点列表
        public HashSet<string> needShowParent = new HashSet<string>();//需要显示的父节点 key父节点
        public Dictionary<string, int> redDotCountDict = new Dictionary<string, int>();//key子节点 value红点计数
        public Dictionary<string, string> childToParentDict = new Dictionary<string, string>();//key子节点 value父节点
        public Dictionary<string, int> nodeCountDict = new Dictionary<string, int>();//key子节点 value节点计数
        public Dictionary<string, RedDot> redDotDict = new Dictionary<string, RedDot>();//key子节点 value红点

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            foreach (var item in parentDict.Values)
            {
                item.Clear();
            }
            parentDict.Clear();
            childToParentDict.Clear();
            nodeCountDict.Clear();
            redDotDict.Clear();
            needShowParent.Clear();
            redDotCountDict.Clear();
        }
        /// <summary>
        /// 增加红点节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="isNeedShow"></param>
        public void AddRedDotNode(string parent, string target, bool isNeedShow)
        {
            if (!string.IsNullOrEmpty(parent) && !parentDict.ContainsKey(parent))
            {
                Log.Debug("父节点是新节点：" + parent);
            }



            if (string.IsNullOrEmpty(target))
            {
                Log.Error($"目标不能为空");
                return;
            }
            if (string.IsNullOrEmpty(parent))
            {
                Log.Error($"父节点不能为空");
                return;
            }
            if (childToParentDict.ContainsKey(target))
            {
                Log.Error($"{target} 已存在");
                return;
            }



            childToParentDict.Add(target, parent);


            if (!nodeCountDict.ContainsKey(target))
            {
                nodeCountDict.Add(target, 0);
            }



            if (!needShowParent.Contains(parent) && isNeedShow)
            {
                needShowParent.Add(parent);
            }


            if (!redDotCountDict.ContainsKey(target))
            {
                redDotCountDict.Add(target, 0);
            }



            if (!nodeCountDict.ContainsKey(parent))
            {
                nodeCountDict.Add(parent, 0);
            }



            if (parentDict.TryGetValue(parent, out List<string> list))
            {
                list.Add(target);
                return;
            }
            list = new List<string>();
            list.Add(target);
            parentDict.Add(parent, list);
        }

        /// <summary>
        /// 移除红点节点
        /// </summary>
        /// <param name="target"></param>
        public void RemoveRedDotNode(string target)
        {
            if (!childToParentDict.TryGetValue(target, out string parent))
            {
                return;
            }

            if (!IsLeafNode(target))
            {
                Log.Error("不能删除父节点");
                return;
            }

            //减少红点计数
            UpdateLogicNodeRetainCount(target, false);


            //移除节点
            childToParentDict.Remove(target);
            if (!string.IsNullOrEmpty(parent))
            {
                parentDict[parent].Remove(target);
                if (parentDict[parent].Count <= 0)
                {
                    parentDict[parent].Clear();
                    parentDict.Remove(parent);
                    needShowParent.Remove(parent);
                }
            }
            nodeCountDict.Remove(target);
        }

        /// <summary>
        /// 增加红点
        /// </summary>
        /// <param name="target"></param>
        /// <param name="redDot"></param>
        public void AddRedDotView(string target, RedDot redDot)
        {
            if (!nodeCountDict.TryGetValue(target, out int nodeCount))
            {
                Log.Error($"节点不存在 {target} 不能增加");
                return;
            }

            redDotDict[target] = redDot;

            if (nodeCount == 0)
            {
                return;
            }
            redDot.Show(GameObjectPoolManager.Instance.GetObject("RedDot"));
        }
        /// <summary>
        /// 移除红点
        /// </summary>
        /// <param name="target"></param>
        /// <param name="redDot"></param>
        public void RemoveRedDotView(string target, out RedDot redDot)
        {
            if (redDotDict.TryGetValue(target, out redDot))
            {
                redDotDict.Remove(target);
            }

            if (redDot == null || !redDot.isRedDotActive)
            {
                return;
            }
            Object.Destroy(redDot);
        }

        /// <summary>
        /// 判断是否是叶子节点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool IsLeafNode(string target)
        {
            return !parentDict.ContainsKey(target);
        }

        /// <summary>
        /// 隐藏红点节点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool HideRedDotNode(string target)
        {
            if (!IsLeafNode(target))
            {
                Log.Error("不能隐藏父节点 " + target);
                return false;
            }

            UpdateLogicNodeRetainCount(target, false);
            return true;
        }

        /// <summary>
        /// 显示红点节点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool ShowRedDotNode(string target)
        {
            if (!IsLeafNode(target))
            {
                Log.Error("不能显示父节点 " + target);
                return false;
            }

            UpdateLogicNodeRetainCount(target, true);
            return true;
        }

        /// <summary>
        /// 更新节点计数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isRaiseCount"></param>
        private void UpdateLogicNodeRetainCount(string target, bool isRaiseCount)
        {
            if (!nodeCountDict.ContainsKey(target))
            {
                Log.Error($"{target} 节点不存在");
                return;
            }

            if (!IsLeafNode(target))
            {
                Log.Error($"{target} 不能是父节点");
                return;
            }
            //提高计数
            if (isRaiseCount)
            {
                if (nodeCountDict[target] == 1)
                {
                    Log.Error($"{target} 节点计数已经是1了");
                    return;
                }

                nodeCountDict[target] += 1;
                if (nodeCountDict[target] != 1)
                {
                    Log.Error($"{target} 节点计数错误 RetainCount = {nodeCountDict[target]}");
                    return;
                }
            }
            else
            {
                if (nodeCountDict[target] != 1)
                {
                    Log.Error($"{target} 节点是不显示状态 RetainCount = {nodeCountDict[target]}");
                    return;
                }
                nodeCountDict[target] += -1;
            }


            int curr = nodeCountDict[target];
            if (curr < 0 || curr > 1)
            {
                Log.Error("红点计数错误，红点逻辑有问题");
                return;
            }
            //显示红点
            if (redDotDict.TryGetValue(target, out RedDot redDot))
            {
                if (isRaiseCount)
                {
                    redDot.Show(GameObjectPoolManager.Instance.GetObject("RedDot"));
                }
                else
                {
                    GameObjectPoolManager.Instance.ReleaseObject(redDot.Recovery());
                }
            }



            //获取父节点
            bool isParentExist = childToParentDict.TryGetValue(target, out string parent);
            //循环遍历上层节点
            while (isParentExist)
            {
                nodeCountDict[parent] += isRaiseCount ? 1 : -1;

                if (nodeCountDict[parent] >= 1 && isRaiseCount)
                {
                    if (redDotDict.TryGetValue(parent, out redDot))
                    {
                        if (!redDot.isRedDotActive)
                        {
                            redDot.Show(GameObjectPoolManager.Instance.GetObject("RedDot"));
                        }
                    }
                }

                if (nodeCountDict[parent] == 0 && !isRaiseCount)
                {
                    if (redDotDict.TryGetValue(parent, out redDot))
                    {
                        GameObjectPoolManager.Instance.ReleaseObject(redDot.Recovery());
                    }
                }
                isParentExist = childToParentDict.TryGetValue(parent, out parent);
            }
        }


        /// <summary>
        /// 刷新红点计数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="Count"></param>
        public void RefreshRedDotViewCount(string target, int Count)
        {
            if (!IsLeafNode(target))
            {
                Log.Error("不能刷新父节点");
                return;
            }

            redDotDict.TryGetValue(target, out RedDot redDot);

            redDotCountDict[target] = Count;

            if (needShowParent.Contains(target) && redDot != null)
            {
                redDot.RefreshRedDotCount(redDotCountDict[target]);
            }


            bool isParentExist = childToParentDict.TryGetValue(target, out string parent);

            while (isParentExist)
            {
                var viewCount = 0;

                foreach (var childNode in parentDict[parent])
                {
                    viewCount += redDotCountDict[childNode];
                }


                redDotCountDict[parent] = viewCount;

                if (redDotDict.TryGetValue(parent, out redDot))
                {
                    if (needShowParent.Contains(parent))
                    {
                        redDot.RefreshRedDotCount(redDotCountDict[parent]);
                    }
                }
                isParentExist = childToParentDict.TryGetValue(parent, out parent);
            }
        }


        #region 接口

        #endregion
    }
}