using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    /// <summary>
    /// 保存关闭节点时的处理数据
    /// </summary>
    public class TurnNode
    {
        public string nodeName;
        public NodeType nodeType;
        public object[] nodeParam;
    }

    public class ElementNode : UINode
    {
        //UI根
        public IUIRoot UIRoot { get; protected set; }
        public GameObject GameObject { get; protected set; }
        public RectTransform RectTransform { get; protected set; }
        public Canvas Canvas { get; protected set; }
        public GraphicRaycaster Raycaster { get; protected set; }
        public CanvasGroup CanvasGroup { get; protected set; }

        public TurnNode ReturnNode { get; protected set; }

        //渲染顺序
        public int SortingOrder { get; protected set; }

        #region 配置字段

        //层级ID
        public UILayerID LayerID { get; protected set; }

        //是否全屏
        public bool IsFullScreen { get; protected set; }

        //返回节点类型
        public NodeType ReturnNodeType { get; protected set; }

        //返回节点名称
        public string ReturnNodeName { get; protected set; }

        //返回节点参数
        public int ReturnNodeParam { get; protected set; }

        #endregion

        public ElementNode(string nodeName)
        {
            NodeName = nodeName;
            Logic = Main.WindowService.GetUILogic(nodeName, this);
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
            }
            else
            {
                if (DomainNode != null && DomainNode.IsCovered)
                    return;

                Log.Debug($"[UI] 取消覆盖 {NodeName}");

                DoCovered(covered);
            }
        }

        public override void Show(object[] param)
        {
            if (NodePhase == NodePhase.Create)
            {
                if (DomainNode != null && DomainNode.NodePhase < NodePhase.Show)
                    return;

                Log.Debug($"[UI] 显示 {NodeName}");

                //把自己节点状态设置为显示
                NodePhase = NodePhase.Show;

                if (DomainNode != null)
                {
                    DomainNode.AddChildNode(this);
                }

                //内部打开
                GameObject.SetActive(true);

                DoShow(param);
            }
        }

        public override object Hide()
        {
            if (NodePhase == NodePhase.Show)
            {
                Log.Debug($"[UI] 隐藏 {NodeName}");

                if (DomainNode != null)
                {
                    //从域中移除当前节点
                    DomainNode.RemoveChildNode(this);
                }

                GetAttachedLayer().DetachElementWidget(this);

                UIRoot.Detach(this);

                ReturnNode = null;
                NodePhase = NodePhase.Create;
                var returnValue = DoHide();
                return returnValue;
            }

            return null;
        }

        public override bool Escape(ref EscapeType escape)
        {
            return DoEscape(ref escape);
        }

        #endregion

        #region 元素扩展流程

        //挂载到UI根
        public void AttachedToRoot(IUIRoot uiRoot)
        {
            DoAttachedToRoot(uiRoot);
        }

        //从UI根移除
        public void DetachedFromRoot()
        {
            DoDetachedFromRoot();
        }

        #endregion

        #region 接口

        protected override void DoConstruct()
        {
            Logic.OnConstruct();
            if (Logic is IUIElementLogic logic)
            {
                EscapeType = logic.EscapeType;
                ReleaseType = logic.ReleaseType;
                LayerID = logic.LayerID;
                IsFullScreen = logic.IsFullScreen;
                ReturnNodeType = logic.ReturnNodeType;
                ReturnNodeName = logic.ReturnNodeName;
                ReturnNodeParam = logic.ReturnNodeParam;
            }
        }

        protected override void DoCreate()
        {
            Canvas = GameObject.AddComponent<Canvas>();
            Raycaster = GameObject.AddComponent<GraphicRaycaster>();
            CanvasGroup = GameObject.AddComponent<CanvasGroup>();
            Logic.OnCreate();
        }

        protected override void DoSwitch(Action<bool> callback)
        {
            Logic.OnSwitch(callback);
        }

        protected override void DoCovered(bool covered)
        {
            if (covered)
            {
                GameObject.SetActive(false);
            }
            else
            {
                GameObject.SetActive(true);
            }

            Logic.OnCovered(covered);
        }

        protected override void DoShow(object[] param)
        {
            if (!string.IsNullOrEmpty(ReturnNodeName) && ReturnNode == null)
            {
                ReturnNode = new TurnNode()
                {
                    nodeName = ReturnNodeName,
                    nodeType = ReturnNodeType,
                };
                if (ReturnNodeParam >= 0)
                {
                    ReturnNode.nodeParam = new object[] { ReturnNodeParam };
                }
            }

            GetAttachedLayer().AttachElementWidget(this);
            Logic.OnShow(param);
        }

        protected override void DoReShow(object[] param)
        {
            Logic.OnReShow(param);
        }

        protected override void DoUpdate()
        {
            Logic.OnUpdate();
        }

        protected override object DoHide()
        {
            //内部隐藏
            GameObject.SetActive(false);

            var returnValue = Logic.OnHide();

            //触发关闭节点回调
            Main.WindowService.DispatchNodeHide(NodeName, returnValue);
            //加入到释放列表
            Main.WindowService.AddToReleaseQueue(this);
            return returnValue;
        }

        protected override void DoDestroy()
        {
            Logic.OnDestroy();
            Object.Destroy(GameObject);
            GameObject = null;
        }

        protected override bool DoEscape(ref EscapeType escape)
        {
            escape = EscapeType;

            if (escape == EscapeType.Skip)
                return false;

            if (!Logic.OnEscape(ref escape))
                return false;

            if (DomainNode != null)
            {
                if (!DomainNode.RequireEscape(this))
                    return false;
            }

            return true;
        }

        #endregion

        #region 元素扩展接口

        protected virtual void DoAttachedToRoot(IUIRoot uiRoot)
        {
            UIRoot = uiRoot;
            GetAttachedLayer().AttachElement(this);
        }

        protected virtual void DoDetachedFromRoot()
        {
            GetAttachedLayer().DetachElement(this);
            UIRoot = null;
        }

        #endregion

        private UILayer GetAttachedLayer()
        {
            return UIRoot.GetLayerByID(LayerID);
        }

        #region 外部调用

        public void CreateElement(AssetLoader loader, Action<ElementNode> callback)
        {
            Main.WindowService.LoadAsyncGameObject?.Invoke(loader, NodeName, (obj) =>
            {
                GameObject = GameObject.Instantiate(obj);
                GameObject.name = NodeName;

                if (GameObject != null)
                {
                    RectTransform = GameObject.transform as RectTransform;
                }

                callback?.Invoke(this);
            });
        }

        public void SetSortingOrder(int sortingOrder)
        {
            SortingOrder = sortingOrder;
        }

        #endregion
    }
}