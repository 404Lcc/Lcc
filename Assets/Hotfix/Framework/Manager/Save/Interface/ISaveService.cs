namespace LccHotfix
{
    public interface ISaveService : IService
    {
        bool IsSaveLoaded { get; }

        void SetSaveHelper(ISaveHelper saveHelper);
        /// <summary>
        /// 设置全局加密方式
        /// </summary>
        void SetEncryption(bool isAES);


        /// <summary>
        /// 设置存储路径 游戏启动时修改
        /// </summary>
        void SetStorePath(StoreMode mode);

        /// <summary>
        /// 校验存档
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool ValidateSaveData(GameSaveData data);

        /// <summary>
        /// 检测有没有存档
        /// </summary>
        /// <returns></returns>
        bool CheckHaveSaveData();

        /// <summary>
        /// 创建新存档
        /// </summary>
        void CreateNewSaveData();

        /// <summary>
        /// 读取存档
        /// </summary>
        void Load();

        /// <summary>
        /// 保存存档
        /// </summary>
        void Save();

        T GetSaveData<T, TSave>() where T : ISaveConverter<TSave>, new() where TSave : ISave;
    }
}