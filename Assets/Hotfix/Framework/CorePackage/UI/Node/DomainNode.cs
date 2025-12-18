using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface IDomainNode
    {
        /// <summary>
        /// 增加子节点
        /// </summary>
        void AddChildNode(ElementNode node);

        /// <summary>
        /// 移除子节点
        /// </summary>
        void RemoveChildNode(ElementNode node);

        /// <summary>
        /// 子节点请求退出
        /// </summary>
        /// <returns></returns>
        bool RequireEscape(ElementNode node);
    }

    public class DomainNode : UINode, IDomainNode
    {
        public List<ElementNode> NodeList { get; protected set; }

        public int StackIndex { get; protected set; }

        public DomainNode(string rootName)
        {
            this.NodeName = rootName;
            this.Logic = Main.WindowService.GetUILogic(rootName, this);
        }

        #region 必要流程

        public override void Covered(bool covered)
        {
            if (IsCovered == covered)
                return;

            IsCovered = covered;

            if (covered)
            {
                Log.Debug($"[UI] 覆盖 {NodeName}");
                DoCovered(covered);

                //给节点全部覆盖
                if (NodeList != null && NodeList.Count > 0)
                {
                    for (int i = NodeList.Count - 1; i >= 0; i--)
                    {
                        NodeList[i].Covered(true);
                    }
                }
            }
            else
            {
                Log.Debug($"[UI] 取消覆盖 {NodeName}");

                DoCovered(covered);

                if (NodeList != null && NodeList.Count > 0)
                {
                    //找到最新的全屏界面索引
                    int fullIndex = NodeList.Count;
                    for (int i = NodeList.Count - 1; i >= 0; i--)
                    {
                        fullIndex = i;
                        if (NodeList[i].IsFullScreen)
                        {
                            break;
                        }
                    }

                    //找到全屏界面后面的子节点，包含全屏界面，设置取消覆盖
                    if (fullIndex < NodeList.Count)
                    {
                        for (int i = fullIndex; i < NodeList.Count; i++)
                        {
                            NodeList[i].Covered(false);
                        }
                    }
                }
            }
        }

        public override void Show(object[] param)
        {
            if (NodePhase == NodePhase.Create)
            {
                Log.Debug($"[UI] 显示 {NodeName}");

                //把自己节点状态设置为显示
                NodePhase = NodePhase.Show;

                DoShow(param);
            }
        }

        public override object Hide()
        {
            if (NodePhase == NodePhase.Show)
            {
                Log.Debug($"[UI] 隐藏 {NodeName}");

                //移除当前域
                Main.WindowService.RemoveDomainFromStack(this);

                //由上向下，移除子节点
                while (NodeList != null && NodeList.Count > 0)
                {
                    var child = NodeList[NodeList.Count - 1];
                    NodeList.RemoveAt(NodeList.Count - 1);
                    child.SetDomainNode(null);
                    child.Hide();
                }

                SetStackIndex(-1);
                NodeList = null;
                NodePhase = NodePhase.Create;
                var returnValue = DoHide();
                return returnValue;
            }

            return null;
        }

        public override bool Escape(ref EscapeType escape)
        {
            //由上向下，处理子节点的返回
            if (NodeList != null && NodeList.Count > 0)
            {
                for (int i = NodeList.Count - 1; i >= 0; i--)
                {
                    if (NodeList[i].Escape(ref escape))
                    {
                        return true;
                    }
                }
            }

            //处理自己的返回
            return DoEscape(ref escape);
        }

        #endregion

        #region 域扩展流程

        /// <summary>
        /// 增加子节点
        /// </summary>
        public void AddChildNode(ElementNode node)
        {
            if (NodeList == null)
            {
                NodeList = new List<ElementNode>();
            }

            //加进子节点
            NodeList.Add(node);

            if (NodeList != null && NodeList.Count > 1)
            {
                //找到最新的全屏界面索引
                //这里遍历包含刚才加入的节点
                int fullIndex = NodeList.Count;
                for (int i = NodeList.Count - 1; i >= 0; i--)
                {
                    fullIndex = i;
                    if (NodeList[i].IsFullScreen)
                    {
                        break;
                    }
                }

                //Count-2是排除刚才加入的节点
                //假如Count是10个节点，fullIndex是5，则覆盖5以前的节点，取消覆盖5到8的节点
                for (int i = 0; i <= NodeList.Count - 2; i++)
                {
                    if (i < fullIndex)
                    {
                        NodeList[i].Covered(true);
                    }
                    else
                    {
                        NodeList[i].Covered(false);
                    }
                }
            }

            DoAddChildNode(node);
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        public void RemoveChildNode(ElementNode node)
        {
            if (NodeList == null || NodeList.Count == 0)
                return;

            //移除节点
            NodeList.Remove(node);
            node.SetDomainNode(null);

            DoRemoveChildNode(node);
        }

        /// <summary>
        /// 子节点请求退出
        /// </summary>
        /// <returns></returns>
        public bool RequireEscape(ElementNode node)
        {
            if (DoRequireEscape(node))
            {
                node.Hide();
                return true;
            }

            return false;
        }

        #endregion

        #region 必要接口

        protected override void DoConstruct()
        {
            Logic?.OnConstruct();
            if (Logic != null && Logic is IUIDomainLogic logic)
            {
                EscapeType = logic.EscapeType;
                ReleaseType = logic.ReleaseType;
            }
            else
            {
                EscapeType = EscapeType.Hide;
                ReleaseType = ReleaseType.Auto;
            }
        }

        protected override void DoCreate()
        {
            Logic?.OnCreate();
        }

        protected override void DoSwitch(Action<bool> callback)
        {
            if (Logic != null)
            {
                Logic.OnSwitch(callback);
            }
            else
            {
                callback(true);
            }
        }

        protected override void DoCovered(bool covered)
        {
            Logic?.OnCovered(covered);
        }

        protected override void DoShow(object[] param)
        {
            Logic?.OnShow(param);
        }

        protected override void DoReShow(object[] param)
        {
            Logic?.OnReShow(param);
        }

        protected override void DoUpdate()
        {
            Logic?.OnUpdate();
        }

        protected override object DoHide()
        {
            object returnValue = null;
            if (Logic != null)
            {
                returnValue = Logic.OnHide();
            }

            //触发关闭节点回调
            Main.WindowService.DispatchNodeHide(NodeName, returnValue);
            //加入到释放列表
            Main.WindowService.AddToReleaseQueue(this);
            return returnValue;
        }

        protected override void DoDestroy()
        {
            Logic?.OnDestroy();
        }

        protected override bool DoEscape(ref EscapeType escape)
        {
            escape = EscapeType;

            if (escape == EscapeType.Skip)
                return false;

            if (Logic != null)
            {
                if (!Logic.OnEscape(ref escape))
                    return false;
            }

            return true;
        }

        #endregion

        #region 域扩展接口

        protected void DoAddChildNode(ElementNode node)
        {
            if (Logic != null && Logic is IUIDomainLogic logic)
            {
                logic.OnAddChildNode(node);
            }
        }

        protected void DoRemoveChildNode(ElementNode node)
        {
            //如果根节点激活
            if (Active)
            {
                TurnNode turn = node.ReturnNode;

                //如果有返回界面，尝试打开
                if (turn != null)
                {
                    switch (turn.nodeType)
                    {
                        case NodeType.Domain:
                            Main.WindowService.ShowDomain(turn.nodeName, turn.nodeParam);
                            break;
                        case NodeType.Element:
                            Main.WindowService.ShowElement(turn.nodeName, turn.nodeParam);
                            break;
                    }
                }
            }

            if (Logic != null && Logic is IUIDomainLogic logic)
            {
                logic.OnRemoveChildNode(node);
            }

            //根据子节点状态，检查是否需要关闭域
            if (NodeList == null || NodeList.Count == 0)
            {
                Hide();
                return;
            }

            //如果当前节点激活并且关闭的子节点是全屏界面
            if (Active && node.IsFullScreen)
            {
                if (NodeList != null && NodeList.Count > 0)
                {
                    //找到最新的全屏界面索引
                    int fullIndex = NodeList.Count;
                    for (int i = NodeList.Count - 1; i >= 0; i--)
                    {
                        fullIndex = i;
                        if (NodeList[i].IsFullScreen)
                        {
                            break;
                        }
                    }

                    //找到全屏界面后面的子节点，包含全屏界面，设置取消覆盖（count是10个节点，fullIndex是5，则从5到9）
                    if (fullIndex < NodeList.Count)
                    {
                        for (int i = fullIndex; i < NodeList.Count; i++)
                        {
                            NodeList[i].Covered(false);
                        }
                    }
                }
            }

        }

        protected bool DoRequireEscape(ElementNode node)
        {
            if (Logic != null && Logic is IUIDomainLogic logic)
            {
                return logic.OnRequireEscape(node);
            }

            return true;
        }

        #endregion

        #region 外部调用

        public void SetStackIndex(int index)
        {
            StackIndex = index;
        }

        /// <summary>
        /// 判断是否包含某个子节点
        /// </summary>
        public bool ContainsNode(UINode node)
        {
            if (NodeList == null || NodeList.Count == 0)
                return false;

            foreach (UINode item in NodeList)
            {
                if (item == node)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取自身及所有子节点
        /// </summary>
        public void GetAllChildNode(ref List<UINode> list)
        {
            if (list == null)
            {
                list = new List<UINode>();
            }

            list.Add(this);

            if (NodeList == null || NodeList.Count == 0)
                return;

            foreach (var item in NodeList)
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// 尝试获取某个子节点
        /// </summary>
        public bool TryGetChildNode(string nodeName, out ElementNode node)
        {
            node = null;

            if (NodeList == null || NodeList.Count == 0)
                return false;

            foreach (var item in NodeList)
            {
                if (item.NodeName.Equals(nodeName))
                {
                    node = item;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试根据名称隐藏子节点
        /// </summary>
        public bool TryHideChildNode(string nodeName, out object returnValue)
        {
            returnValue = null;
            //如果是自己是关闭
            if (NodeName == nodeName)
            {
                returnValue = Hide();
                return true;
            }
            else
            {
                if (NodeList == null || NodeList.Count == 0)
                    return false;

                for (int i = NodeList.Count - 1; i >= 0; i--)
                {
                    if (NodeList[i].NodeName == nodeName)
                    {
                        returnValue = NodeList[i].Hide();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取当前域最新的节点
        /// </summary>
        public UINode GetTopNode()
        {
            if (NodeList == null || NodeList.Count == 0)
                return this;

            return NodeList[NodeList.Count - 1];
        }

        #endregion
    }
}