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

    void Save(GameSaveData value);

    GameSaveData Load();

    GameSaveData ReadGameSaveData(string json);

    bool FileExists();

    void Delete();
}