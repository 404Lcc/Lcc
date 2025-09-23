using System.Collections.Generic;
using LccHotfix;

public interface ISaveHelper
{
    /// <summary>
    /// 设置全局加密方式
    /// </summary>
    /// <param name="isAES"></param>
    void SetEncryption(bool isAES);

    /// <summary>
    /// 设置存储路径
    /// </summary>
    /// <param name="mode"></param>
    void SetStorePath(StoreMode mode);

    /// <summary>
    /// 获取本地所有存档文件
    /// </summary>
    /// <returns></returns>
    List<string> GetAllSaveFile();

    /// <summary>
    /// 检测有没有某个存档
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    bool CheckHaveSaveFile(string name);

    /// <summary>
    /// 保存当前加载的存档
    /// </summary>
    /// <param name="name"></param>
    /// <param name="data"></param>
    void SaveFile(string name, SaveData data);

    /// <summary>
    /// 加载某个存档
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    SaveData LoadSaveFile(string name);

    /// <summary>
    /// 解析存档数据
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    SaveData ReadGameSaveData(string text);

    /// <summary>
    /// 删除某个存档
    /// </summary>
    /// <param name="name"></param>
    void Delete(string name);
}