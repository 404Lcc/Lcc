using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class UIManager : Module, IUIService
    {
        /// <summary>
        /// 域的栈
        /// 栈里的每个域实际是一个全屏界面和n个小窗口
        /// 作用域是自身，不能跨域修改其它界面
        /// </summary>
        private Stack<DomainNode> _domainStack = new Stack<DomainNode>();

        /// <summary>
        /// 当前活动的通用域
        /// 特殊域不受栈的限制，可以用任意方式唤醒和关闭
        /// </summary>
        private DomainNode _commonDomain;


        /// <summary>
        /// 释放队列
        /// </summary>
        private List<UINode> _releaseQueue = new List<UINode>();

        /// <summary>
        /// 被关闭界面会自动缓存多少帧然后释放
        /// 30s
        /// </summary>
        private int _autoCacheTime = 900;

        /// <summary>
        /// 节点关闭回调
        /// </summary>
        private Dictionary<string, Action<object>> _hideCallback = new Dictionary<string, Action<object>>();

        //当前切换中的节点
        private UINode _switchingNode;
        
        private Dictionary<string, Type> _uiLogics = new Dictionary<string, Type>();
        
        private AssetLoader _assetLoader = new AssetLoader();

        //需要更新的节点列表
        private List<UINode> _updateNodes = new List<UINode>();

        //释放节点
        private RectTransform _releaseRoot;
        

        
        public IUIRoot Root { get; protected set; }

        /// <summary>
        /// UI根节点
        /// </summary>
        public Transform UIRoot { get; set; }

        /// <summary>
        /// ui相机
        /// </summary>
        public Camera UICamera { get; set; }

        public DomainNode CommonDomain => _commonDomain;



        /// <summary>
        /// 异步加载GameObject
        /// </summary>
        public Action<AssetLoader, string, Action<GameObject>> LoadAsyncGameObject { get; set; }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_commonDomain != null)
            {
                _updateNodes.Clear();
                _commonDomain.GetAllChildNode(ref _updateNodes);
                foreach (var node in _updateNodes)
                {
                    node.Update();
                }
            }

            if (_domainStack.Count == 0)
                return;
            
            var topDomain = _domainStack.Peek();
            if (topDomain != null)
            {
                _updateNodes.Clear();
                topDomain.GetAllChildNode(ref _updateNodes);
                foreach (var node in _updateNodes)
                {
                    node.Update();
                }
            }
        }

        internal override void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideTopNode();
            }

            UpdateReleaseQueue();
        }

        internal override void Shutdown()
        {
            HideAllDomain();
            _commonDomain.Hide();
            _commonDomain.Destroy();
            
            ForceClearReleaseQueue(ReleaseType.Never);
            
            if (_releaseRoot != null)
            {
                GameObject.DestroyImmediate(_releaseRoot.gameObject);
                _releaseRoot = null;
            }

            Root.Finalize();
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            var uiRoot = GameObject.Find("Root");
            if (uiRoot != null)
            {
                UICamera = uiRoot.GetComponentInChildren<Camera>();
                UIRoot = ClientTools.GetChild(uiRoot, "UIRoot").transform;
            }
            
            LoadAsyncGameObject = (loader, asset, end) => { loader.LoadAssetAsync<GameObject>(asset, handle => { end?.Invoke(handle.AssetObject as GameObject); }); };

            
            foreach (Type item in GetType().Assembly.GetTypes())
            {
                if (typeof(IUILogic).IsAssignableFrom(item))
                {
                    _uiLogics[item.Name] = item;
                }
            }

            Root = new UIRoot(UIRoot.gameObject);
            Root.Initialize();
            _commonDomain = GetOrCreateDomain("UIDomainCommon");
            _commonDomain.StackIndex = 0;
            _commonDomain.Show(null);
        }

        /// <summary>
        /// 获取UI逻辑类
        /// </summary>
        /// <param name="name"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public IUILogic GetUILogic(string name, UINode node)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            IUILogic logic = null;
            if (_uiLogics.TryGetValue(name, out Type monoType))
            {
                logic = Activator.CreateInstance(monoType) as IUILogic;
                logic.Node = node;
            }

            return logic;
        }

        /// <summary>
        /// 显示域（这里只是创建，并不会改变当前栈结构，确认可打开后才会完成显示）
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="elementName"></param>
        /// <param name="args"></param>
        public void ShowDomain(string domainName, string elementName, params object[] args)
        {
            if (string.IsNullOrEmpty(domainName))
                return;

            if (string.IsNullOrEmpty(elementName))
                return;

            if (_switchingNode != null)
            {
                Log.Error($"[UI] 切换{_switchingNode.NodeName}节点时，请求显示{domainName}域{elementName}界面");
                return;
            }

            Log.Debug($"[UI] 显示界面{elementName}");

            var domain = GetOrCreateDomain(domainName);

            if (!domain.TryGetChildNode(elementName, out var element))
            {
                element = GetOrCreateElement(elementName, out var isNewCreate);
                element.DomainNode = domain;
                Root.Attach(elementName, element);
                _switchingNode = element;
                if (isNewCreate)
                {
                    element.CreateElement(_assetLoader, (node) =>
                    {
                        node.Create();
                        SwitchNode(node, args);
                    });
                }
            }
            else
            {
                element.DomainNode = domain;
                _switchingNode = element;
                SwitchNode(element, args);
            }
        }

        /// <summary>
        /// 显示域（这里只是创建，并不会改变当前栈结构，确认可打开后才会完成显示）
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public void ShowDomain(string name, params object[] args)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (_switchingNode != null)
            {
                Log.Error($"[UI] 切换{_switchingNode.NodeName}节点时，请求显示{name}域");
                return;
            }

            Log.Debug($"[UI] 显示域{name}");

            DomainNode domain = GetOrCreateDomain(name);
            domain.DomainNode = domain;
            _switchingNode = domain;
            SwitchNode(domain, args);
        }

        /// <summary>
        /// 显示界面（这里只是创建，并不会改变当前栈结构，确认可打开后才会完成显示）
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public void ShowElement(string name, params object[] args)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (_switchingNode != null)
            {
                Log.Error($"[UI] 切换{_switchingNode.NodeName}节点时，请求显示{name}域");
                return;
            }

            Log.Debug($"[UI] 显示界面{name}");

            var domain = GetOrCreateDomain(string.Empty);

            if (!domain.TryGetChildNode(name, out var element))
            {
                element = GetOrCreateElement(name, out var isNewCreate);
                element.DomainNode = domain;
                Root.Attach(name, element);
                _switchingNode = element;
                if (isNewCreate)
                {
                    element.CreateElement(_assetLoader, node =>
                    {
                        node.Create();
                        SwitchNode(node, args);
                    });
                }
                else
                {
                    SwitchNode(element, args);
                }
            }
            else
            {
                //比如当前栈顶界面是弹窗，上一个界面是全屏界面，那全屏界面也在显示中就会在域里，会走到这里
                element.DomainNode = domain;
                _switchingNode = element;
                SwitchNode(element, args);
            }
        }

        /// <summary>
        /// 关闭界面（只能关闭一个栈内已显示的界面，不能用来关闭一个不活跃界面）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object HideElement(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            //通用域尝试隐藏界面
            if (_commonDomain.TryHideChildNode(name, out var returnValue))
                return returnValue;

            if (_domainStack.Count == 0)
                return null;

            Log.Debug($"[UI] 隐藏界面{name}");

            DomainNode domain = _domainStack.Peek();

            //尝试隐藏子界面
            if (domain.TryHideChildNode(name, out returnValue))
                return returnValue;

            return null;
        }

        /// <summary>
        /// 关闭最顶层界面
        /// </summary>
        public void HideTopNode()
        {
            EscapeType escape = EscapeType.Hide;
            //处理通用域
            if (_commonDomain.Escape(ref escape))
                return;

            if (_domainStack.Count == 0)
                return;

            //处理最顶层域
            var top = _domainStack.Peek();
            top.Escape(ref escape);
        }

        /// <summary>
        /// 关闭所有域
        /// </summary>
        public void HideAllDomain()
        {
            //todo 判断如果有资源加载完成才能加进去
            //如果有切换中的节点，则加入释放列表
            if (_switchingNode != null)
            {
                AddToReleaseQueue(_switchingNode);
                _switchingNode = null;
            }

            //隐藏通用域
            CommonDomain.Hide();

            //隐藏栈里的域
            while (_domainStack.Count > 0)
            {
                _domainStack.Pop().Hide();
            }
        }

        /// <summary>
        /// 获取域
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DomainNode GetDomain(string name)
        {
            if (_domainStack.Count == 0)
                return null;

            foreach (var item in _domainStack)
            {
                if (item.NodeName == name)
                    return item;
            }

            return null;
        }
        
        /// <summary>
        /// 获取域
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetDomain<T>(string name) where T : UIDomainBase
        {
            var node = GetDomain(name);
            if (node != null)
            {
                return node.Logic as T;
            }

            return null;
        }

        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ElementNode GetElement(string name)
        {
            var topDomain = GetTopDomain();

            if (topDomain == null)
                return null;

            if (topDomain.TryGetChildNode(name, out var node))
            {
                return node;
            }

            return null;
        }
        
        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetElement<T>(string name) where T : UIElementBase
        {
            var node = GetElement(name);
            if (node != null)
            {
                return node.Logic as T;
            }

            return null;
        }

        /// <summary>
        /// 获取最顶层域
        /// </summary>
        /// <returns></returns>
        public DomainNode GetTopDomain()
        {
            if (_domainStack.Count == 0)
                return null;

            return _domainStack.Peek();
        }

        /// <summary>
        /// 获取最顶层界面
        /// </summary>
        /// <returns></returns>
        public ElementNode GetTopElement()
        {
            var topDomain = GetTopDomain();

            if (topDomain == null)
                return null;

            var topNode = topDomain.GetTopNode();
            if (topNode is ElementNode elementNode)
            {
                return elementNode;
            }

            return null;
        }

        /// <summary>
        /// 界面是否激活
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsElementActive(string name)
        {
            var node = GetElement(name);
            if (node != null)
            {
                return node.Active;
            }

            return false;
        }

        /// <summary>
        /// 从栈里移除指定域
        /// </summary>
        /// <param name="node"></param>
        public void RemoveDomainFromStack(DomainNode node)
        {
            var topDomain = GetTopDomain();

            if (topDomain == null)
                return;

            //如果参数node是栈顶，则移出去
            if (node == topDomain)
            {
                _domainStack.Pop();

                if (_domainStack.Count > 0)
                {
                    topDomain.Covered(false);
                }
            }
            //如果不是栈顶
            else
            {
                var index = 0;
                foreach (var item in _domainStack)
                {
                    if (item == node)
                        break;
                    index++;
                }

                //如果相等说明这个node不在栈里管理，不需要重新计算栈，直接return
                if (index == _domainStack.Count)
                    return;

                Stack<DomainNode> tempStack = new Stack<DomainNode>();

                while (index >= 0)
                {
                    //这里会提前pop出来，index == 0之后不会把这个塞进去（index == 0这个是要移除的）
                    var top = _domainStack.Pop();
                    index--;
                    if (index == 0)
                    {
                    }
                    else
                    {
                        tempStack.Push(top);
                    }
                }

                //按照原来栈顺序重新塞回来，并且重新计算stackIndex
                while (tempStack.Count > 0)
                {
                    var temp = tempStack.Pop();
                    temp.StackIndex = _domainStack.Count;
                    _domainStack.Push(temp);
                }
            }
        }

        /// <summary>
        /// 增加到释放队列
        /// </summary>
        /// <param name="node"></param>
        public void AddToReleaseQueue(UINode node)
        {
            if (node != null)
            {
                //如果是自动的，设置时间
                if (node.ReleaseType == ReleaseType.Auto)
                {
                    node.ReleaseTimer = _autoCacheTime;
                }
                else
                {
                    node.ReleaseTimer = 0;
                }

                //创建释放节点
                if (_releaseRoot == null)
                {
                    _releaseRoot = new GameObject("WaitForRelease").AddComponent<RectTransform>();
                    _releaseRoot.SetParent(UIRoot);
                    _releaseRoot.localScale = Vector3.one;
                    _releaseRoot.localPosition = new Vector3(30000, 0, 0);
                    //归一
                    _releaseRoot.anchorMin = Vector3.zero;
                    _releaseRoot.anchorMax = Vector3.one;
                    _releaseRoot.sizeDelta = Vector3.zero;
                }

                if (node is ElementNode element)
                {
                    if (element.RectTransform != null)
                    {
                        element.RectTransform.parent = _releaseRoot;
                    }
                }

                //增加进去
                _releaseQueue.Add(node);
            }
        }

        /// <summary>
        /// 强制清理释放队列
        /// </summary>
        /// <param name="level">筛选释放节点，小于等于这个级别的所有节点</param>
        public void ForceClearReleaseQueue(ReleaseType level = ReleaseType.Auto)
        {
            for (int i = _releaseQueue.Count - 1; i >= 0; i--)
            {
                if (_releaseQueue[i].ReleaseType <= level)
                {
                    _releaseQueue[i].Destroy();
                    _releaseQueue.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 增加隐藏节点回调
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void AddNodeHideCallback(string name, Action<object> callback)
        {
            if (_hideCallback.TryGetValue(name, out Action<object> action))
            {
                action -= callback;
                action += callback;
            }
            else
            {
                _hideCallback.Add(name, callback);
            }
        }

        /// <summary>
        /// 移除隐藏节点回调
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void RemoveNodeHideCallback(string name, Action<object> callback)
        {
            if (_hideCallback.TryGetValue(name, out Action<object> action))
            {
                action -= callback;
            }
        }

        /// <summary>
        /// 触发隐藏节点回调
        /// </summary>
        /// <param name="name"></param>
        /// <param name="returnValue"></param>
        public void DispatchNodeHide(string name, object returnValue)
        {
            if (_hideCallback.TryGetValue(name, out Action<object> action))
            {
                action?.Invoke(returnValue);
            }
        }

        /// <summary>
        /// 更新释放队列
        /// </summary>
        private void UpdateReleaseQueue()
        {
            for (int i = _releaseQueue.Count - 1; i >= 0; i--)
            {
                //如果可以释放
                if (_releaseQueue[i].CanRelease())
                {
                    _releaseQueue[i].Destroy();
                    _releaseQueue.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 切换节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="args"></param>
        private void SwitchNode(UINode node, object[] args)
        {
            //准备切换
            node.Switch((canOpen) => SwitchEnd(node, canOpen, args));
        }

        /// <summary>
        /// 切换节点完成
        /// </summary>
        /// <param name="node"></param>
        /// <param name="canOpen"></param>
        /// <param name="args"></param>
        private void SwitchEnd(UINode node, bool canOpen, object[] args)
        {
            if (_switchingNode == null)
                return;

            if (_switchingNode != node)
                return;

            //切换中的节点设为空
            _switchingNode = null;

            // 打开失败
            if (!canOpen)
            {
                //加入释放列表
                AddToReleaseQueue(node);
                return;
            }

            var domain = node.DomainNode;

            //不是通用域
            if (domain != CommonDomain)
            {
                //如果是新创建的域
                if (domain.StackIndex < 0)
                {
                    //把栈顶的节点覆盖
                    if (_domainStack.Count > 0)
                    {
                        _domainStack.Peek().Covered(true);
                    }

                    domain.StackIndex = _domainStack.Count;
                    _domainStack.Push(domain);

                    domain.Covered(false);

                    //如果参数node就是自身域，显示时把参数传进去。node不是域就只需要把域显示出来，不传参
                    if (node == domain)
                    {
                        domain.Show(args);
                    }
                    else
                    {
                        domain.Show(null);
                    }
                }
                //如果是在栈里的域
                else
                {
                    //判断当前node的域在不在栈顶
                    bool isTop = _domainStack.Count == node.DomainNode.StackIndex + 1;
                    //如果不在，按照顺序把后面的域都隐藏
                    if (!isTop)
                    {
                        while (_domainStack.Peek() != domain)
                        {
                            var top = _domainStack.Pop();
                            top.Hide();
                        }
                    }

                    domain.Covered(false);

                    //如果参数node就是自身域，显示时把参数传进去。node不是域就只需要把域显示出来，不传参
                    if (node == domain)
                    {
                        domain.ReShow(args);
                    }
                    else
                    {
                        domain.ReShow(null);
                    }
                }
            }

            //如果node是界面
            if (node is ElementNode element)
            {
                //如果域里存在这个界面
                if (domain.ContainsNode(node))
                {
                    //如果界面是全屏界面，则隐藏当前域下除node以外的所有界面
                    if (element.IsFullScreen)
                    {
                        for (int i = domain.NodeList.Count - 1; i >= 0; i--)
                        {
                            //把当前node后面的界面全部关闭
                            if (domain.NodeList[i] == node)
                                break;
                            domain.NodeList[i].Hide();
                        }
                    }

                    //重新打开界面
                    node.Covered(false);
                    node.ReShow(args);
                }
                else
                {
                    //打开界面
                    node.Covered(false);
                    node.Show(args);
                }
            }
        }

        /// <summary>
        /// 获取一个域，如果没有则创建一个新的
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private DomainNode GetOrCreateDomain(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.Assert(_domainStack.Count > 0);
                //返回最顶层域
                return _domainStack.Peek();
            }

            //如果是通用域
            if (_commonDomain != null && name == _commonDomain.NodeName)
            {
                return _commonDomain;
            }

            //从栈里找
            foreach (var item in _domainStack)
            {
                if (item.NodeName.Equals(name))
                {
                    return item;
                }
            }

            //从释放列表里找回域
            DomainNode domain = null;
            for (int i = 0; i < _releaseQueue.Count; i++)
            {
                if (_releaseQueue[i].NodeName.Equals(name))
                {
                    domain = _releaseQueue[i] as DomainNode;
                    _releaseQueue.RemoveAt(i);
                    break;
                }
            }

            //创建新的
            if (domain == null)
            {
                domain = new DomainNode(name);
                domain.DomainNode = domain;
                domain.Construct();
                Root.Attach(name, domain);
                domain.Create();
            }

            //新建的域索引一定要设置-1
            domain.StackIndex = -1;
            return domain;
        }

        /// <summary>
        /// 获取一个界面，如果没有则创建一个新的
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isNewCreate"></param>
        /// <returns></returns>
        private ElementNode GetOrCreateElement(string name, out bool isNewCreate)
        {
            isNewCreate = false;

            //从释放列表里找回界面
            for (int i = 0; i < _releaseQueue.Count; i++)
            {
                if (_releaseQueue[i].NodeName.Equals(name))
                {
                    var element = _releaseQueue[i] as ElementNode;
                    _releaseQueue.RemoveAt(i);
                    return element;
                }
            }

            var newElement = new ElementNode(name);
            newElement.Construct();
            isNewCreate = true;
            return newElement;
        }
    }
}