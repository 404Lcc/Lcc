using System;

namespace LccHotfix
{
    public abstract class UINode
    {
        //节点名
        public string NodeName { get; protected set; }

        //是否被遮挡
        public bool IsCovered { get; protected set; }

        //节点的状态
        public NodePhase NodePhase { get; protected set; }

        //是否激活
        public bool Active => NodePhase == NodePhase.Show;

        //逻辑接口
        public IUILogic Logic { get; set; }

        //释放计时器
        public int ReleaseTimer { get; set; }
        
        //域
        public DomainNode DomainNode { get; set; }

        #region 可在OnConstruct配置
        //回退类型
        public EscapeType EscapeType { get; set; }

        //释放类型
        public ReleaseType ReleaseType { get; set; } = ReleaseType.Auto;
        #endregion

        public bool CanRelease()
        {
            if (ReleaseType > ReleaseType.Auto)
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

        public abstract bool Escape(ref EscapeType escape);

        #endregion

        #region 接口

        //初始化
        protected abstract void DoConstruct();

        //创建
        protected abstract void DoCreate();

        //切换节点
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

        //处理节点返回
        protected abstract bool DoEscape(ref EscapeType escape);

        #endregion
    }
}