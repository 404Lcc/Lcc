using System;

namespace LccHotfix
{
    public class WRootNode : WNode
    {
        /// <summary>
        /// 在栈里的位置
        /// </summary>
        public int stackIndex;
        /// <summary>
        /// 每个根节点用于向下广播的黑板
        /// </summary>
        private WBlackboard _blackboard;
        public WBlackboard Blackboard => _blackboard;
        public WRootNode(string rootName)
        {
            this._nodeName = rootName;
            this._logicName = rootName;
            this.rootNode = this;
            this.escapeType = EscapeType.AUTO_CLOSE;
            this.releaseType = ReleaseType.AUTO;
            this._blackboard = new WBlackboard();
            this._logic = Main.WindowService.CreateLogic(rootName, null);
            if (_logic != null)
                _logic.WNode = this;
        }

        protected override void DoStart()
        {
            _logic?.OnStart();
        }
        protected override void DoUpdate()
        {
            _logic?.OnUpdate();
            _blackboard?.Update();
        }
        protected override void DoSwitch(Action<bool> callback)
        {
            if (_logic != null)
            {
                _logic.OnSwitch(callback);
            }
            else
            {
                callback(true);
            }
        }
        protected override void DoOpen(object[] param)
        {
            gameObject?.SetActive(true);
            _logic?.OnOpen(param);
        }
        protected override void DoReset(object[] param)
        {
            _logic?.OnReset(param);
        }

        protected override void DoResume()
        {
            _logic?.OnResume();
        }
        protected override void DoPause()
        {
            _logic?.OnPause();
        }

        protected override object DoClose()
        {
            gameObject?.SetActive(false);
            object backValue = null;
            if (_logic != null)
                backValue = _logic.OnClose();
            //触发关闭节点回调
            Main.WindowService.OnWindowClose(NodeName, backValue);
            //加入到释放列表
            Main.WindowService.AddToReleaseQueue(this);
            return backValue;
        }

        protected override bool DoEscape(ref EscapeType escape)
        {
            if (_logic != null)
                return _logic.OnEscape(ref escape);
            return base.DoEscape(ref escape);
        }
        //移除
        protected override void DoRemove()
        {
            _logic?.OnRemove();
            if (gameObject != null)
                UnityEngine.Object.Destroy(gameObject);
        }
        protected override void DoChildOpened(WNode child)
        {
            _logic?.OnChildOpened(child);
        }
        protected override void DoChildClosed(WNode child)
        {
            //如果根节点激活
            if (Active)
            {
                TurnNode turn = child.returnNode;

                //如果有关闭后返回窗口，尝试打开
                if (turn != null)
                {
                    //根节点肯定没有父节点，尝试根据类型打开窗口
                    if (!TryGetNodeForward(turn.nodeName, out WNode node))
                    {
                        switch (turn.nodeType)
                        {
                            case NodeType.ROOT:
                                Main.WindowService.OpenRoot(turn.nodeName, turn.nodeParam);
                                break;
                            case NodeType.WINDOW:
                                Main.WindowService.OpenWindow(turn.nodeName, turn.nodeParam);
                                break;
                        }
                    }
                }
            }


            if (_logic != null)
            {
                if (_logic.OnChildClosed(child))
                    return;
            }
            else
            {
                //根据子节点状态，检查是否需要关闭根节点
                if (DefaultChildCheck())
                {
                    Close();
                    return;
                }
            }

            //如果当前节点激活并且关闭的子节点是全屏窗口
            //这个时候_childNode里已经没有要移除的child了
            if (Active && child.IsFullScreen)
            {
                if (_childNode != null && _childNode.Count > 0)
                {
                    //找到最新的全屏窗口索引
                    int fullIndex = _childNode.Count;
                    for (int i = _childNode.Count - 1; i >= 0; i--)
                    {
                        fullIndex = i;
                        if (_childNode[i].IsFullScreen)
                        {
                            break;
                        }
                    }

                    //找到全屏窗口后面的节点，包含这个全屏窗口
                    if (fullIndex < _childNode.Count)
                    {
                        //恢复全屏界面和后面的节点（假如_childNode.count是10个节点，fullIndex是5，则恢复5到9）
                        for (int i = _childNode.Count - 1; i >= fullIndex; i--)
                        {
                            //给子节点恢复
                            _childNode[i].Resume();
                        }
                    }
                }
            }

        }
        //子节点请求退出
        protected override bool DoChildRequireEscape(WNode child)
        {
            if (_logic != null)
            {
                return _logic.OnChildRequireEscape(child);
            }

            return true;
        }



        /// <summary>
        /// 根据子节点的状态判断根节点是否需要关闭
        /// </summary>
        /// <returns></returns>
        public bool DefaultChildCheck()
        {
            if (_childNode == null || _childNode.Count == 0)
            {
                return true;
            }

            foreach (WNode wNode in _childNode)
            {
                //如果有主要节点，则不关闭
                if (wNode.IsMainNode)
                {
                    return false;
                }
            }

            return true;
        }


    }
}