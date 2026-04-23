namespace LccHotfix
{
    public delegate bool WaitCheckFunc(CustomNode node);

    public class WaitCheckBhvCfg : ICustomNodeCfg
    {
        public WaitCheckFunc CheckFunc { get; protected set; }

        public System.Type NodeType()
        {
            return typeof(WaitCheckBhv);
        }

        public WaitCheckBhvCfg(WaitCheckFunc func)
        {
            CheckFunc = func;
        }
    }

    /// <summary>
    /// 运行时：调用简单函数 等待阻塞
    /// </summary>
    public class WaitCheckBhv : BehaviorNode<WaitCheckBhvCfg>, INeedStopCheck
    {
        public bool CanStop()
        {
            if (_cfg?.CheckFunc != null)
            {
                return _cfg.CheckFunc(this);
            }

            return true;
        }
    }
}