using System;
using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public class RedDotNode
    {
        public string key;
        public Dictionary<int, RedDotRuntimeData> runData = new Dictionary<int, RedDotRuntimeData>();

        /// <summary>
        /// 红点改变通知
        /// </summary>
        private Action<string, int, int> _onChangedAction;

        public RedDotNode(string key)
        {
            this.key = key;
        }

        public bool HaveRuntimeData(int id)
        {
            return runData.ContainsKey(id);
        }

        public void AddRuntimeData(int id)
        {
            var info = new RedDotRuntimeData();
            runData[id] = info;
        }

        public bool RemoveRuntimeData(int id)
        {
            return runData.Remove(id);
        }

        public List<int> GetRuntimeIdList()
        {
            return runData.Keys.ToList();
        }

        /// <summary>
        /// 获取红点运行时数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RedDotRuntimeData GetRuntimeData(int id)
        {
            if (runData.TryGetValue(id, out var data))
            {
                return data;
            }
            data = new RedDotRuntimeData();
            runData[id] = data;
            return data;
        }

        /// <summary>
        /// 添加一个回调
        /// string = key
        /// int = id
        /// int = 当前红点数量
        /// 并马上设置相关数据
        /// (因为界面初始化时都是需要刷新界面的 所以默认调用一次)
        /// </summary>
        public void AddOnChanged(int id, Action<string, int, int> action)
        {
            _onChangedAction += action;
            InvokeOnChanged(id);
        }

        /// <summary>
        /// 移除回调
        /// </summary>
        /// <param name="action"></param>
        public void RemoveChanged(int id, Action<string, int, int> action)
        {
            _onChangedAction -= action;
        }

        //try回调
        public void InvokeOnChanged(int id)
        {
            _onChangedAction?.Invoke(key, id, GetRuntimeData(id).count);
        }
    }

    /// <summary>
    /// 红点运行时数据
    /// </summary>
    public class RedDotRuntimeData
    {
        public int count;
    }

    internal class RedDotManager : Module
    {
        public static RedDotManager Instance => Entry.GetModule<RedDotManager>();

        public Dictionary<string, List<string>> parentDict = new Dictionary<string, List<string>>();//key父节点 value子节点列表
        public HashSet<string> needShowParent = new HashSet<string>();//需要显示的父节点 key父节点
        public Dictionary<string, string> childToParentDict = new Dictionary<string, string>();//key子节点 value父节点
        public Dictionary<string, RedDotNode> nodeCountDict = new Dictionary<string, RedDotNode>();//key子节点 value节点计数

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
            needShowParent.Clear();
        }


        /// <summary>
        /// 增加红点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="isNeedShow"></param>
        public void AddRedDotNode(string parent, string target, bool isNeedShow)
        {
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
                RedDotNode node = new RedDotNode(target);
                nodeCountDict.Add(target, node);
            }



            if (!needShowParent.Contains(parent) && isNeedShow)
            {
                needShowParent.Add(parent);
            }






            if (!nodeCountDict.ContainsKey(parent))
            {
                RedDotNode node = new RedDotNode(parent);
                nodeCountDict.Add(parent, node);
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
        /// 移除红点
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

            //减少节点计数
            UpdateLogicNodeRetainCount(target, false, 0);
            foreach (var item in nodeCountDict[target].GetRuntimeIdList())
            {
                UpdateLogicNodeRetainCount(target, false, item);
            }


            //移除红点
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
        /// 增加运行时红点数据
        /// </summary>
        /// <param name="target"></param>
        public void AddRuntimeData(string target, int id = 0)
        {
            if (!childToParentDict.TryGetValue(target, out string parent))
            {
                return;
            }

            if (!IsLeafNode(target))
            {
                Log.Error("不能在父节点增加");
                return;
            }

            if (nodeCountDict[target].HaveRuntimeData(id))
            {
                Log.Error($"{target} {id}已存在");
                return;
            }

            nodeCountDict[target].AddRuntimeData(id);
        }



        /// <summary>
        /// 移除运行时红点数据
        /// </summary>
        /// <param name="target"></param>
        public void RemoveRuntimeData(string target, int id = 0)
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

            //减少节点计数
            UpdateLogicNodeRetainCount(target, false, id);


            nodeCountDict[target].RemoveRuntimeData(id);
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
        /// 隐藏红点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool HideRedDotNode(string target, int id = 0)
        {
            if (!IsLeafNode(target))
            {
                Log.Error("不能隐藏父节点 " + target);
                return false;
            }

            UpdateLogicNodeRetainCount(target, false, id);
            return true;
        }

        /// <summary>
        /// 显示红点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool ShowRedDotNode(string target, int id = 0)
        {
            if (IsLogicAlreadyShow(target, id))
            {
                return false;
            }

            if (!IsLeafNode(target))
            {
                Log.Error("不能显示父节点 " + target);
                return false;
            }

            UpdateLogicNodeRetainCount(target, true, id);
            return true;
        }

        /// <summary>
        /// 更新节点计数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isRaiseCount"></param>
        private void UpdateLogicNodeRetainCount(string target, bool isRaiseCount, int id)
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
                if (nodeCountDict[target].GetRuntimeData(id).count == 1)
                {
                    Log.Error($"{target} 节点计数已经是1了");
                    return;
                }

                nodeCountDict[target].GetRuntimeData(id).count += 1;
                if (nodeCountDict[target].GetRuntimeData(id).count != 1)
                {
                    Log.Error($"{target} 节点计数错误 RetainCount = {nodeCountDict[target]}");
                    return;
                }
            }
            else
            {
                if (nodeCountDict[target].GetRuntimeData(id).count != 1)
                {
                    //Log.Error($"{target} 节点是不显示状态 RetainCount = {nodeCountDict[target]}");
                    return;
                }
                nodeCountDict[target].GetRuntimeData(id).count += -1;
            }


            int curr = nodeCountDict[target].GetRuntimeData(id).count;
            if (curr < 0 || curr > 1)
            {
                Log.Error("节点计数错误，节点逻辑错误");
                return;
            }


            //刷新节点
            nodeCountDict[target].InvokeOnChanged(id);




            //获取父节点
            bool isParentExist = childToParentDict.TryGetValue(target, out string parent);
            //循环遍历上层节点
            while (isParentExist)
            {
                nodeCountDict[parent].GetRuntimeData(0).count += isRaiseCount ? 1 : -1;

                //刷新父节点
                nodeCountDict[parent].InvokeOnChanged(0);

                isParentExist = childToParentDict.TryGetValue(parent, out parent);
            }
        }


        #region 接口

        /// <summary>
        /// 节点是否已经处于显示状态
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsLogicAlreadyShow(string target, int id)
        {
            if (!nodeCountDict.ContainsKey(target))
            {
                return false;
            }

            return nodeCountDict[target].GetRuntimeData(id).count >= 1;
        }

        /// <summary>
        /// 获取这个红点数据 
        /// </summary>
        public RedDotNode GetData(string key)
        {
            nodeCountDict.TryGetValue(key, out var data);
            if (data == null)
            {
                Log.Error($"没有获取到这个红点数据 {key}");
            }

            return data;
        }

        /// <summary>
        /// 添加变化监听
        /// </summary>
        public bool AddChanged(string key, int id, Action<string, int, int> action)
        {
            var data = GetData(key);
            if (data == null)
            {
                return false;
            }

            data.AddOnChanged(id, action);
            return true;
        }

        /// <summary>
        /// 移除变化监听
        /// </summary>
        public bool RemoveChanged(string key, int id, Action<string, int, int> action)
        {
            var data = GetData(key);
            if (data == null)
            {
                return false;
            }

            data.RemoveChanged(id, action);
            return true;
        }

        #endregion
    }
}