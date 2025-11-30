namespace LccHotfix
{
    public enum LoadingType
    {
        None,
        Normal,
        Fast,
        AfterLogin, //考虑登录之后重新回login的情况，首次进入登录进度条和重新进入登录进度条不一样
    }

    public abstract class LoadProcedureHandler : ICoroutine
    {
        public int procedureType;

        /// <summary>
        /// 加载方式
        /// </summary>
        public LoadingType loadType;

        /// <summary>
        /// 正在加载
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// 清理完成
        /// </summary>
        public bool IsCleanup { get; set; }

        /// <summary>
        /// 加载显示回调
        /// </summary>
        public virtual void ProcedureLoadHandler()
        {
        }

        /// <summary>
        /// 验证流程是否可进入
        /// </summary>
        public virtual bool ProcedureEnterStateHandler()
        {
            return true;
        }

        /// <summary>
        /// 进入回调
        /// </summary>
        public virtual void ProcedureStartHandler()
        {
        }

        /// <summary>
        /// 加载完成回调
        /// </summary>
        public virtual void ProcedureLoadEndHandler()
        {
            IsLoading = false;
        }

        /// <summary>
        /// 退出回调
        /// </summary>
        public virtual void ProcedureExitHandler()
        {
        }

        /// <summary>
        /// 开始加载时间
        /// </summary>
        public float startLoadTime;

        /// <summary>
        ///	深度清理
        /// </summary>
        public bool deepClean;

        /// <summary>
        /// 开启流程后跳转到界面
        /// </summary>
        public TurnNode turnNode;

        /// <summary>
        /// 更新
        /// </summary>
        public virtual void Tick()
        {
        }

        public virtual void LateUpdate()
        {
        }
    }
}