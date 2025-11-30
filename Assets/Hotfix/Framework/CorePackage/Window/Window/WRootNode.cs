using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class WRootNode : WNode
    {
        /// <summary>
        /// 在栈里的位置
        /// </summary>
        public int stackIndex;
        
        /// <summary>
        /// 全屏界面可以包含许多子界面
        /// </summary>
        protected List<Window> _childNode;
        
        
        public List<Window> ChildNode => _childNode;
        public WRootNode(string rootName)
        {
            this._nodeName = rootName;
            this._logicName = rootName;
            this.rootNode = this;
            this.escapeType = EscapeType.AUTO_CLOSE;
            this.releaseType = ReleaseType.AUTO;
            this._logic = Main.WindowService.CreateLogic(rootName, null);
            if (_logic != null)
                _logic.WNode = this;
        }

        public override void Open(object[] param)
        {
            if (NodePhase == NodePhase.DEACTIVE)
            {

                Log.Debug($"ui open window {NodeName}");

                //把自己节点状态设置为激活
                NodePhase = NodePhase.ACTIVE;


                DoOpen(param);

            }
            
            
        }

        //判断是否包含节点（包含自身）
        public  bool Contains(WNode node)
        {
            if (node == this)
                return true;

            if (_childNode == null)
                return false;

            foreach (WNode childNode in _childNode)
            {
                if (childNode == node)
                {
                    return true;
                }
            }

            return false;
        }
        

        
        //获取节点（包含自身）
        public bool TryGetNode(string windowName, out WNode node)
        {
            node = null;
            if (_nodeName.Equals(windowName))
            {
                node = this;
                return true;
            }

            if (_childNode == null || _childNode.Count == 0)
                return false;
            foreach (var childNode in _childNode)
            {
                if (childNode.NodeName.Equals(windowName))
                {
                    node = childNode;
                    return true;
                }
            }

            return false;
        }
        
        

        // //从下向上，判断这个节点是否存在，存在则返回节点的父节点
        // public bool TryGetNodeForward(string windowName, out WNode node)
        // {
        //     node = null;
        //
        //     if (parentNode == null)
        //         return false;
        //     if (parentNode.NodeName == windowName)
        //     {
        //         node = parentNode;
        //         return true;
        //     }
        //
        //     if (parentNode._childNode != null && parentNode._childNode.Count > 0)
        //     {
        //         foreach (var child in parentNode._childNode)
        //         {
        //             if (child.NodeName == windowName)
        //             {
        //                 node = parentNode;
        //                 return true;
        //             }
        //         }
        //     }
        //
        //     return parentNode.TryGetNodeForward(windowName, out node);
        // }

        //获取当前节点下最新的节点
        public WNode GetTopWindow()
        {
            if (_childNode == null || _childNode.Count == 0)
                return this;
            return _childNode[_childNode.Count - 1];
        }


        //父级调用打开孩子
        public void ChildOpened(Window child)
        {
            if (_childNode == null)
            {
                _childNode = new List<Window>();
            }

            //加进子节点
            _childNode.Add(child);

            if (_childNode != null && _childNode.Count > 1)
            {
                //找到全屏窗口索引，这里遍历包含刚才加入的节点
                int fullIndex = _childNode.Count;
                for (int i = _childNode.Count - 1; i >= 0; i--)
                {
                    fullIndex = i;
                    if (_childNode[i].IsFullScreen)
                    {
                        break;
                    }
                }

                //如果没找到全屏窗口fullIndex就是_childNode.Count。-2是排除刚才加入的节点
                //遍历子节点，排除刚才加入的节点（排除顶层节点，假如_childNode.count是10个节点，fullIndex是5，则暂停5以前的节点，恢复5到8节点。）
                //刚才加入的节点在后面调用Resume
                for (int i = _childNode.Count - 2; i >= 0; i--)
                {
                    //小于全屏索引的节点并且不是顶层窗口的节点全部暂停，否则调用恢复窗口
                    //用小于是因为全屏窗口不要暂停
                    //主要节点也会暂停
                    if (i < fullIndex)
                    {
                        _childNode[i].SetCovered(true);
                    }
                    else
                    {
                        _childNode[i].SetCovered(false);
                    }
                }

            }

            DoChildOpened(child);
            
        }

        public void ChildClosed(Window child)
        {
            if (_childNode == null)
                return;
            //移除节点
            _childNode.Remove(child);
            child.rootNode = null;

            DoChildClosed(child);


        }
        /// <summary>
        /// 返回键请求关闭窗口处理
        /// </summary>
        /// <param name="escape"></param>
        /// <returns></returns>
        public override bool Escape(ref EscapeType escape)
        {
            // 首先处理子节点的返回
            if (_childNode != null && _childNode.Count > 0)
            {
                for (int i = _childNode.Count - 1; i >= 0; i--)
                {
                    if (_childNode[i].Escape(ref escape))
                    {
                        return true;
                    }
                }
            }

            // 处理自己的返回
            return DoEscape(ref escape);
        }
        
        
        //处理子节点退出
        public bool ChildRequireEscape(WNode child)
        {
            //判断条件
            if (DoChildRequireEscape(child))
            {
                child.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 递归的获取自身及所有子节点
        /// </summary>
        /// <param name="childList"></param>
        public void GetAllChild(List<WNode> childList)
        {
            childList.Add(this);
            if (_childNode != null)
            {
                foreach (var child in _childNode)
                {
                    childList.Add(child);
                }
            }
        }
        
        

        //关闭所有子窗口
        public void CloseAllChild()
        {
            if (_childNode == null || _childNode.Count == 0)
                return;
            for (int i = _childNode.Count - 1; i >= 0; i--)
            {
                _childNode[i].Close();
            }
        }
        
        //尝试关闭子窗口，根据名称
        public bool TryCloseChild(string windowClose, out object val)
        {
            val = null;
            //如果是自己是关闭
            if (NodeName == windowClose)
            {
                val = Close();
                return true;
            }
            else
            {
                //如果不是就找子节点
                if (_childNode == null || _childNode.Count == 0)
                    return false;
                for (int i = _childNode.Count - 1; i >= 0; i--)
                {
                    if (_childNode[i].NodeName == windowClose)
                    {
                        val = _childNode[i].Close();
                        return true;
                    }
                }
            }

            return false;
        }

        public override object Close()
        {
            //如果是暂停状态
            if (NodePhase == NodePhase.ACTIVE)
            {
                Log.Debug($"ui close window {NodeName}");
                
                // //如果有父级
                // // 由下向上
                // if (parentNode != null)
                // {
                //     //移除从父级移除当前节点
                //     parentNode.ChildClosed(this);
                // }
                
                //如果是当前节点是根节点，则移除根节点
                Main.WindowService.RemoveRoot(this);

                //移除当前节点的子节点
                // 由上向下
                while (_childNode != null && _childNode.Count > 0)
                {
                    var child = _childNode[_childNode.Count - 1];
                    _childNode.RemoveAt(_childNode.Count - 1);
                    child.rootNode = null;
                    child.Close();
                }

                _childNode = null;
                returnNode = null;
                //设置关闭状态
                NodePhase = NodePhase.DEACTIVE;
                var returnValue = DoClose();

                return returnValue;
            }

            return null;
            
            
        }


        public override void SetCovered(bool covered)
        {
            if (IsCovered == covered)
                return;

            IsCovered = covered;

            if (covered)
            {
                Log.Debug($"ui pause window {NodeName}");
                DoCovered(covered);

                //给子节点全部暂停
                if (_childNode != null && _childNode.Count > 0)
                {
                    for (int i = _childNode.Count - 1; i >= 0; i--)
                    {
                        _childNode[i].SetCovered(true);
                    }
                }
            }
            else
            {
                // if (parentNode != null && parentNode._isCovered)
                //     return;

                Log.Debug($"ui resume window {NodeName}");

                DoCovered(covered);

                if (_childNode != null && _childNode.Count > 0)
                {
                    //找到全屏窗口索引
                    int fullIndex = _childNode.Count;
                    for (int i = _childNode.Count - 1; i >= 0; i--)
                    {
                        fullIndex = i;
                        if (_childNode[i].IsFullScreen)
                        {
                            break;
                        }
                    }

                    //找到全屏窗口后面的节点，包含全屏窗口
                    for (int i = fullIndex; i < _childNode.Count; i++)
                    {
                        //给子节点恢复
                        _childNode[i].SetCovered(false);
                    }
                }
            }
            
            
        }

        protected override void DoStart()
        {
            _logic?.OnStart();
        }

        protected override void DoUpdate()
        {
            _logic?.OnUpdate();
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
            // gameObject?.SetActive(true);
            _logic?.OnOpen(param);
        }

        protected override void DoReset(object[] param)
        {
            _logic?.OnReset(param);
        }

        protected override void DoCovered(bool covered)
        {
            _logic?.DoCovered(covered);
        }


        protected override object DoClose()
        {
            // gameObject?.SetActive(false);
            object backValue = null;
            if (_logic != null)
            {
                backValue = _logic.OnClose();
            }

            //触发关闭节点回调
            Main.WindowService.OnWindowClose(NodeName, backValue);
            //加入到释放列表
            Main.WindowService.AddToReleaseQueue(this);
            return backValue;
        }

        protected override bool DoEscape(ref EscapeType escape)
        {
            if (_logic != null)
            {
                return _logic.OnEscape(ref escape);
            }
            
            escape = this.escapeType;
            
            if (escape == EscapeType.SKIP_OVER)
                return false;


            return true;
        }

        //移除
        protected override void DoRemove()
        {
            _logic?.OnRemove();
            // if (gameObject != null)
            // {
            //     UnityEngine.Object.Destroy(gameObject);
            // }
        }

        /// <summary>
        /// 子节点打开
        /// </summary>
        /// <param name="child"></param>
        protected void DoChildOpened(WNode child)
        {
            _logic?.OnChildOpened(child);
        }

        /// <summary>
        /// 子节点关闭
        /// </summary>
        /// <param name="child"></param>
        protected void DoChildClosed(Window child)
        {
            //如果根节点激活
            if (Active)
            {
                TurnNode turn = child.returnNode;

                //如果有关闭后返回窗口，尝试打开
                if (turn != null)
                {
                    //根节点肯定没有父节点，尝试根据类型打开窗口
                    //if (!TryGetNodeForward(turn.nodeName, out WNode node))
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
                _logic.OnChildClosed(child);
            }

            //根据子节点状态，检查是否需要关闭根节点
            if (DefaultChildCheck())
            {
                Close();
                return;
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
                            _childNode[i].SetCovered(false);
                        }
                    }
                }
            }

        }


        
        //子节点请求退出
        protected  bool DoChildRequireEscape(WNode child)
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

            return false;
        }


    }
}