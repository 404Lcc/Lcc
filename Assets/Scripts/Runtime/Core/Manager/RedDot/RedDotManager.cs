using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class RedDot : MonoBehaviour
    {
        public bool isRedDotActive;
        private GameObject redDot;
        private Text redDotCount;

        public Vector3 scale = Vector3.one;
        public Vector2 offset = Vector2.zero;

        void Start()
        {
            redDot = AssetManager.Instance.InstantiateAsset("RedDot", AssetType.Tool);
            redDotCount = redDot.GetComponentInChildren<Text>();


            redDot.transform.SetParent(transform, false);
            redDot.transform.localScale = scale;
            redDot.transform.GetComponent<RectTransform>().anchoredPosition = offset;

            Hide();

        }
        public void Show()
        {
            isRedDotActive = true;
            redDotCount.text = string.Empty;
            redDot.SetActive(isRedDotActive);
        }
        public void Hide()
        {
            isRedDotActive = false;
            redDotCount.text = string.Empty;
            redDot.SetActive(isRedDotActive);


        }

        public void RefreshRedDotCount(int count)
        {
            if (!isRedDotActive)
            {
                return;
            }
            redDotCount.text = count <= 0 ? string.Empty : count.ToString();
        }


        public void OnDestroy()
        {
            Hide();
            Destroy(redDot);


        }

    }

    public class RedDotManager : Singleton<RedDotManager>
    {
        public Dictionary<string, List<string>> parentDict = new Dictionary<string, List<string>>();//key父节点 value子节点列表
        public HashSet<string> needShowParent = new HashSet<string>();//需要显示的父节点 key父节点
        public Dictionary<string, int> redDotCountDict = new Dictionary<string, int>();//key子节点 value红点计数
        public Dictionary<string, string> childToParentDict = new Dictionary<string, string>();//key子节点 value父节点
        public Dictionary<string, int> nodeCountDict = new Dictionary<string, int>();//key子节点 value节点计数
        public Dictionary<string, RedDot> redDotDict = new Dictionary<string, RedDot>();//key子节点 value红点


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
                LogUtil.LogWarning("父节点是新节点：" + parent);
            }



            if (string.IsNullOrEmpty(target))
            {
                LogUtil.LogError($"目标不能为空");
                return;
            }
            if (string.IsNullOrEmpty(parent))
            {
                LogUtil.LogError($"父节点不能为空");
                return;
            }
            if (childToParentDict.ContainsKey(target))
            {
                LogUtil.LogError($"{target} 已存在");
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
        /// 增加红点节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="isNeedShow"></param>
        /// <param name="redDot"></param>
        public void AddRedDotNode(string parent, string target, bool isNeedShow, RedDot redDot)
        {
            AddRedDotNode(parent, target, isNeedShow);
            AddRedDot(target, redDot);
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
                LogUtil.LogError("不能删除父节点");
                return;
            }

            //减少红点计数
            UpdateNodeCount(target, false);


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
        /// 移除红点节点
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isRemoveRedDot"></param>
        public void RemoveRedDotNode(string target, bool isRemoveRedDot)
        {
            RemoveRedDotNode(target);
            if (isRemoveRedDot)
            {
                RemoveRedDot(target, out RedDot redDot);
            }
        }
        /// <summary>
        /// 增加红点
        /// </summary>
        /// <param name="target"></param>
        /// <param name="redDot"></param>
        public void AddRedDot(string target, RedDot redDot)
        {
            if (!nodeCountDict.TryGetValue(target, out int nodeCount))
            {
                LogUtil.LogError($"节点不存在 {target} 不能增加");
                return;
            }

            redDotDict[target] = redDot;

            if (nodeCount == 0)
            {
                return;
            }
            redDot.Show();
        }
        /// <summary>
        /// 移除红点
        /// </summary>
        /// <param name="target"></param>
        /// <param name="redDot"></param>
        public void RemoveRedDot(string target, out RedDot redDot)
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
        /// 显示红点节点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool ShowRedDotNode(string target)
        {
            if (IsAlreadyShow(target))
            {
                return false;
            }

            if (!IsLeafNode(target))
            {
                LogUtil.LogError("不能显示父节点 " + target);
                return false;
            }

            UpdateNodeCount(target, true);
            return true;
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
                LogUtil.LogError("不能隐藏父节点 " + target);
                return false;
            }

            UpdateNodeCount(target, false);
            return true;
        }
        /// <summary>
        /// 刷新红点计数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="Count"></param>
        public void RefreshRedDotCount(string target, int Count)
        {
            if (!IsLeafNode(target))
            {
                LogUtil.LogError("不能刷新父节点");
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




        /// <summary>
        /// 更新节点计数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isRaiseCount"></param>
        private void UpdateNodeCount(string target, bool isRaiseCount)
        {
            if (!nodeCountDict.ContainsKey(target))
            {
                LogUtil.LogError($"{target} 节点不存在");
                return;
            }

            if (!IsLeafNode(target))
            {
                LogUtil.LogError($"{target} 不能是父节点");
                return;
            }
            //提高计数
            if (isRaiseCount)
            {
                if (nodeCountDict[target] == 1)
                {
                    LogUtil.LogError($"{target} 节点计数已经是1了");
                    return;
                }

                nodeCountDict[target] += 1;
                if (nodeCountDict[target] != 1)
                {
                    LogUtil.LogError($"{target} 节点计数错误 RetainCount = {nodeCountDict[target]}");
                    return;
                }
            }
            else
            {
                if (nodeCountDict[target] != 1)
                {
                    LogUtil.LogError($"{target} 节点是不显示状态 RetainCount = {nodeCountDict[target]}");
                    return;
                }
                nodeCountDict[target] += -1;
            }


            int curr = nodeCountDict[target];
            if (curr < 0 || curr > 1)
            {
                LogUtil.LogError("红点计数错误，红点逻辑有问题");
                return;
            }
            //显示红点
            if (redDotDict.TryGetValue(target, out RedDot redDot))
            {
                if (isRaiseCount)
                {
                    redDot.Show();
                }
                else
                {
                    redDot.Hide();
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
                            redDot.Show();
                        }
                    }
                }

                if (nodeCountDict[parent] == 0 && !isRaiseCount)
                {
                    if (redDotDict.TryGetValue(parent, out redDot))
                    {
                        redDot.Hide();
                    }
                }
                isParentExist = childToParentDict.TryGetValue(parent, out parent);
            }
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
        /// 红点是否已经处于显示状态
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool IsAlreadyShow(string target)
        {
            if (!nodeCountDict.ContainsKey(target))
            {
                return false;
            }
            return nodeCountDict[target] >= 1;
        }



        public override void OnDestroy()
        {
            base.OnDestroy();


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

    }
}