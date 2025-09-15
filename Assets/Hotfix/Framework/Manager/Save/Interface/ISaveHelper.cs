using LccHotfix;

public interface ISaveHelper
{
    /// <summary>
    /// 设置全局加密方式
    /// </summary>
    void SetEncryption(bool isAES);

    /// <summary>
    /// 设置存储路径 游戏启动时修改
    /// </summary>
    void SetStorePath(StoreMode mode);

    void Save<T>(string key, T value);
    GameSaveData Load(string key);
    GameSaveData ReadGameSaveData(string json);
    void DeleteKey(string key);
    bool KeyExists(string key);
}