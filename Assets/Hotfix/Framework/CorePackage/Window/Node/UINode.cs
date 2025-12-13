using System;

namespace LccHotfix
{
    public abstract class UINode
    {
        //UI根
        public IUIRoot UIRoot { get; protected set; }

        //节点名
        public string NodeName { get; protected set; }

        //是否被遮挡
        public bool IsCovered { get; protected set; }

        //窗口的状态
        public NodePhase NodePhase { get; protected set; }

        //是否激活
        public bool Active => NodePhase == NodePhase.Show;

        //逻辑接口
        public IUILogic Logic { get; set; }

        //域
        public DomainNode DomainNode { get; set; }

        //回退类型
        public EscapeType EscapeType { get; set; }

        //释放类型
        public ReleaseType ReleaseType { get; set; } = ReleaseType.AUTO;

        //释放计时器
        public int ReleaseTimer { get; set; }


        public bool AutoRemove()
        {
            if (ReleaseType > ReleaseType.AUTO)
                return false;
            ReleaseTimer--;
            if (ReleaseTimer <= 0)
            {
                return true;
            }

            return false;
        }

        #region 必要流程

        public void Construct()
        {
            DoConstruct();
        }

        public void AttachedToRoot(IUIRoot uiRoot)
        {
            DoAttachedToRoot(uiRoot);
        }

        public void Create()
        {
            NodePhase = NodePhase.Create;
            DoCreate();
        }

        public void Switch(Action<bool> callback)
        {
            DoSwitch(callback);
        }

        public abstract void Covered(bool covered);
        public abstract void Show(object[] param);

        public void ReShow(object[] param)
        {
            if (NodePhase == NodePhase.Show)
            {
                DoReShow(param);
            }
        }

        public void Update()
        {
            if (NodePhase == NodePhase.Show)
            {
                DoUpdate();
            }
        }

        public abstract object Hide();

        public void Destroy()
        {
            DoDestroy();
        }

        public void DetachedFromRoot()
        {
            DoDetachedFromRoot();
        }

        public abstract bool Escape(ref EscapeType escape);

        #endregion

        #region 接口

        //初始化
        protected abstract void DoConstruct();

        //挂载到UI根
        protected abstract void DoAttachedToRoot(IUIRoot uiRoot);

        //创建
        protected abstract void DoCreate();

        //切换窗口
        protected abstract void DoSwitch(Action<bool> callback);

        //覆盖
        protected abstract void DoCovered(bool covered);

        //打开
        protected abstract void DoShow(object[] param);

        //重新打开
        protected abstract void DoReShow(object[] param);

        //更新
        protected abstract void DoUpdate();

        //隐藏
        protected abstract object DoHide();

        //删除
        protected abstract void DoDestroy();

        //从UI根移除
        protected abstract void DoDetachedFromRoot();

        //处理窗口返回
        protected abstract bool DoEscape(ref EscapeType escape);

        #endregion
    }
}