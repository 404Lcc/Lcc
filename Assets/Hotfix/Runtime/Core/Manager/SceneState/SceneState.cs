namespace LccHotfix
{
    public enum LoadingType
    {
        None,
        Normal,
        Fast,
    }
    public abstract class SceneState : ISceneState
    {

        public SceneStateType sceneType;

        /// <summary>
        /// 开始加载时间
        /// </summary>
        public float startLoadTime;


        /// <summary>
        /// 加载方式
        /// </summary>
        public LoadingType loadType;




        //打开场景展示ui
        public JumpNode jumpNode;


        /// <summary>
        /// 正在加载
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// 加载显示回调
        /// </summary>
        public virtual bool SceneLoadHandler()
        {
            return false;
        }



        /// <summary>
        /// 加载完成回调
        /// </summary>
        public virtual void SceneLoadEndHandler()
        {
            IsLoading = false;
        }


        public virtual void OnEnter(object[] args)
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void Tick()
        {
        }
    }
}