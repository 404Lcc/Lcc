using System;
using LccHotfix;
using LitJson;

public class DefaultSaveHelper : ISaveHelper
{
    public const string SavePath = "./SaveDatas/";
    public const string FileName = "saveData.lcc";
    public const string Key = "GameSaveData";

    private ES3Settings _settings;

    public DefaultSaveHelper()
    {
        _settings = new ES3Settings();
        SetEncryption(false);
        SetStorePath(StoreMode.Official);
    }

    public void SetEncryption(bool isAES)
    {
        if (isAES)
        {
            _settings.encryptionType = ES3.EncryptionType.AES;
            _settings.encryptionPassword = "xxxxxxxxxxxxxxxx";
        }
        else
        {
            _settings.encryptionType = ES3.EncryptionType.None;
        }
    }

    public void SetStorePath(StoreMode mode)
    {
        if (mode == StoreMode.Beta)
        {
            _settings.directory = ES3.Directory.DataPath;
            _settings.path = SavePath + FileName;
        }
        else if (mode == StoreMode.Official)
        {
            _settings.directory = ES3.Directory.PersistentDataPath;
            _settings.path = SavePath + FileName;
        }
    }

    public void Save(GameSaveData value)
    {
        var json = JsonMapper.ToJson(value);
        ES3.Save(Key, json, _settings);
    }

    public GameSaveData Load()
    {
        var text = ES3.Load<string>(Key, _settings);
        return ReadGameSaveData(text);
    }

    public GameSaveData ReadGameSaveData(string text)
    {
        GameSaveData gameSaveData = new GameSaveData();
        var jsonData = JsonMapper.ToObject(text);
        if (jsonData.ContainsKey("saveTime"))
        {
            gameSaveData.saveTime = DateTime.Parse(jsonData["saveTime"].ToString());
        }

        if (jsonData.ContainsKey("saveVersion"))
        {
            gameSaveData.saveVersion = int.Parse(jsonData["saveVersion"].ToString());
        }

        var saveListData = jsonData["saveList"];
        if (saveListData.IsArray)
        {
            for (int i = 0; i < saveListData.Count; i++)
            {
                var itemData = saveListData[i];
                if (itemData.ContainsKey("TypeName"))
                {
                    var typeName = itemData["TypeName"].ToString();
                    var type = Main.CodeTypesService.GetType(typeName);
                    var itemObject = JsonMapper.ToObject(itemData.ToJson(), type) as ISave;
                    gameSaveData.saveList.Add(itemObject);
                }
            }
        }

        return gameSaveData;
    }

    public bool FileExists()
    {
        return ES3.FileExists(_settings);
    }

    public void Delete()
    {
        ES3.DeleteFile(_settings);
    }
}