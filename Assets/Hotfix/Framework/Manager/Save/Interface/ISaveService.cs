using System.Collections.Generic;

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
        /// 设置存储路径
        /// </summary>
        void SetStorePath(StoreMode mode);

        /// <summary>
        /// 校验存档
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool ValidateSaveData(GameSaveData data);

        /// <summary>
        /// 获取本地所有存档文件
        /// </summary>
        /// <returns></returns>
        List<string> GetAllSaveFiles();

        /// <summary>
        /// 检测有没有某个存档
        /// </summary>
        /// <returns></returns>
        bool CheckHaveSaveData(string name);

        /// <summary>
        /// 创建新存档
        /// </summary>
        void CreateNewSaveData(string name);

        /// <summary>
        /// 读取存档
        /// </summary>
        void Load(string name);

        /// <summary>
        /// 保存存档
        /// </summary>
        void Save();

        /// <summary>
        /// 获取存档转化数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TSave"></typeparam>
        /// <returns></returns>
        T GetSaveConverterData<T, TSave>() where T : ISaveConverter<TSave>, new() where TSave : ISave;
    }
}