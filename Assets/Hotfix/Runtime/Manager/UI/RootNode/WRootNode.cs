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
            this._logic = Entry.GetModule<WindowManager>().CreateLogic(rootName, null);
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
            Entry.GetModule<WindowManager>().OnWindowClose(NodeName, backValue);
            Entry.GetModule<WindowManager>().AddToReleaseQueue(this);
            return backValue;
        }

        protected override bool DoEscape(ref EscapeType escape)
        {
            if (_logic != null)
                return _logic.OnEscape(ref escape);
            return base.DoEscape(ref escape);
        }

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
            if (Active)
            {
                TurnNode turn = child.returnNode;

                if (turn != null)
                {

                    if (!TryGetNodeForward(turn.nodeName, out WNode node))
                    {
                        switch (turn.nodeType)
                        {
                            case NodeType.ROOT:
                                Entry.GetModule<WindowManager>().OpenRoot(turn.nodeName, turn.nodeParam);
                                break;
                            case NodeType.WINDOW:
                                Entry.GetModule<WindowManager>().OpenWindow(turn.nodeName, turn.nodeParam);
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
                if (DefaultChildCheck())
                {
                    Close();
                    return;
                }
            }


            if (Active && child.IsFullScreen)
            {
                if (_childNode != null && _childNode.Count > 0)
                {
                    int fullIndex = _childNode.Count;
                    for (int i = _childNode.Count - 1; i >= 0; i--)
                    {
                        fullIndex = i;
                        if (_childNode[i].IsFullScreen)
                        {
                            break;
                        }
                    }

                    if (fullIndex < _childNode.Count)
                    {
                        for (int i = _childNode.Count - 1; i >= fullIndex; i--)
                        {
                            _childNode[i].Resume();
                        }
                    }
                }
            }

        }
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
                if (wNode.IsMainNode)
                {
                    return false;
                }
            }

            return true;
        }


    }
}