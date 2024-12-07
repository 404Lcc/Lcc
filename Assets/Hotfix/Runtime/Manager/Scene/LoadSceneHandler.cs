namespace LccHotfix
{
    public enum LoadingType
    {
        None,
        Normal,
        Fast,
        AfterLogin,//考虑登录之后重新回login的情况，首次进入登录进度条和重新进入登录进度条不一样
    }
    public abstract class LoadSceneHandler
    {

        public SceneType sceneType;



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
        public virtual void SceneLoadHandler()
        {
            SceneManager.Instance.StartCoroutine(SceneManager.Instance.ShowSceneLoading(loadType));
        }

        /// <summary>
        /// 验证场景是否可进入
        /// </summary>
        public virtual bool SceneEnterStateHandler()
        {
            return true;
        }

        /// <summary>
        /// 进入回调
        /// </summary>
        public virtual void SceneStartHandler()
        {
        }


        /// <summary>
        /// 加载完成回调
        /// </summary>
        public virtual void SceneLoadEndHandler()
        {
            IsLoading = false;
        }

        /// <summary>
        /// 退出回调
        /// </summary>
        public virtual void SceneExitHandler()
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
        /// 传递的参数缓存
        /// </summary>
        public object param;

        /// <summary>
        /// 开启场景后跳转到界面
        /// </summary>
        public WNode.TurnNode turnNode;



        /// <summary>
        /// 更新
        /// </summary>
        public virtual void Tick()
        {
        }
    }
}