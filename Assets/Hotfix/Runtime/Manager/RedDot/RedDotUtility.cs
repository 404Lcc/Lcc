using UnityEngine;

namespace LccHotfix
{
    public static class RedDotUtility
    {
        /// <summary>
        /// 增加逻辑红点节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="isNeedShowNum"></param>
        public static void AddRedDotNode(string parent, string target, bool isNeedShowNum)
        {
            if (!string.IsNullOrEmpty(parent) && !RedDotManager.Instance.parentDict.ContainsKey(parent))
            {
                Log.Warning("Runtime动态添加的红点，其父节点是新节点：" + parent);
            }

            RedDotManager.Instance.AddRedDotNode(parent, target, isNeedShowNum);
        }

        /// <summary>
        /// 移除逻辑红点
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isRemoveView"></param>
        public static void RemoveRedDotNode(string target, bool isRemoveView = true)
        {
            RedDotManager.Instance.RemoveRedDotNode(target);
            if (isRemoveView)
            {
                RedDotManager.Instance.RemoveRedDotView(target, out RedDot redDot);
            }
        }


        /// <summary>
        /// 增加红点节点显示层
        /// </summary>
        /// <param name="target"></param>
        public static void AddRedDotNodeView(string target, GameObject gameObject, Vector3 scale, Vector2 offset)
        {
            RedDot redDot = gameObject.GetComponent<RedDot>() ?? gameObject.AddComponent<RedDot>();
            redDot.scale = scale;
            redDot.offset = offset;
            RedDotManager.Instance.AddRedDotView(target, redDot);
        }


        /// <summary>
        /// 增加红点节点显示层
        /// </summary>
        /// <param name="target"></param>
        /// <param name="redDot"></param>
        public static void AddRedDotView(string target, RedDot redDot)
        {
            RedDotManager.Instance.AddRedDotView(target, redDot);
        }

        /// <summary>
        /// 移除红点节点显示层
        /// </summary>
        /// <param name="target"></param>
        /// <param name="redDot"></param>
        public static void RemoveRedDotView(string target, out RedDot redDot)
        {
            RedDotManager.Instance.RemoveRedDotView(target, out redDot);
        }

        /// <summary>
        /// 隐藏逻辑红点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool HideRedDotNode(string target)
        {
            return RedDotManager.Instance.HideRedDotNode(target);
        }

        /// <summary>
        /// 显示逻辑红点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool ShowRedDotNode(string target)
        {
            if (IsLogicAlreadyShow(target))
            {
                return false;
            }

            return RedDotManager.Instance.ShowRedDotNode(target);
        }

        /// <summary>
        /// 逻辑红点是否已经处于显示状态
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsLogicAlreadyShow(string target)
        {
            if (!RedDotManager.Instance.nodeCountDict.ContainsKey(target))
            {
                return false;
            }

            return RedDotManager.Instance.nodeCountDict[target] >= 1;
        }

        /// <summary>
        /// 刷新红点显示层的文本数量
        /// </summary>
        /// <param name="target"></param>
        /// <param name="Count"></param>
        public static void RefreshRedDotViewCount(string target, int count)
        {
            RedDotManager.Instance.RefreshRedDotViewCount(target, count);
        }
    }
}