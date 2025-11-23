namespace LccHotfix
{
    public interface IMainService : IService
    {
        void OnInstall();

        /// <summary>
        /// 增加游戏框架模块。
        /// </summary>
        T AddModule<T>() where T : Module, IService;
    }
}