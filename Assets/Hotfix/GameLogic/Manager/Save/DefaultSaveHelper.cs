using System;
using System.Collections.Generic;
using System.Linq;
using ES3Internal;
using LccHotfix;
using LitJson;
using UnityEngine;

public class DefaultSaveHelper : ISaveHelper
{
    public const string SavePath = "SaveDatas/";
    public const string Key = "GameSaveData";


    private bool _isAES;
    private StoreMode _mode;

    public DefaultSaveHelper()
    {
        SetEncryption(false);
        SetStorePath(StoreMode.Official);
    }

    /// <summary>
    /// 设置全局加密方式
    /// </summary>
    /// <param name="isAES"></param>
    public void SetEncryption(bool isAES)
    {
        this._isAES = isAES;
    }

    /// <summary>
    /// 设置存储路径
    /// </summary>
    /// <param name="mode"></param>
    public void SetStorePath(StoreMode mode)
    {
        this._mode = mode;
    }


    /// <summary>
    /// 获取本地所有存档文件
    /// </summary>
    /// <returns></returns>
    public List<string> GetFiles()
    {
        var settings = new ES3Settings();
        if (_mode == StoreMode.Beta)
        {
            settings.directory = ES3.Directory.DataPath;
            settings.path = SavePath;
        }
        else if (_mode == StoreMode.Official)
        {
            settings.directory = ES3.Directory.PersistentDataPath;
            settings.path = SavePath;
        }

        if (ES3.DirectoryExists(settings))
        {
            return ES3.GetFiles(settings).ToList();
        }

        return new List<string>();
    }

    /// <summary>
    /// 检测有没有某个存档
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool CheckHaveSaveData(string name)
    {
        var names = GetFiles();
        foreach (var item in names)
        {
            if (item == name)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 保存当前加载的存档
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void Save(string name, SaveData value)
    {
        var json = JsonMapper.ToJson(value);
        var settings = new ES3Settings();
        if (_mode == StoreMode.Beta)
        {
            settings.directory = ES3.Directory.DataPath;
            settings.path = SavePath + name;
        }
        else if (_mode == StoreMode.Official)
        {
            settings.directory = ES3.Directory.PersistentDataPath;
            settings.path = SavePath + name;
        }

        if (_isAES)
        {
            settings.encryptionType = ES3.EncryptionType.AES;
            settings.encryptionPassword = "xxxxxxxxxxxxxxxx";
        }
        else
        {
            settings.encryptionType = ES3.EncryptionType.None;
        }

        ES3.Save(Key, json, settings);
    }

    /// <summary>
    /// 加载某个存档
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public SaveData Load(string name)
    {
        var settings = new ES3Settings();

        if (_mode == StoreMode.Beta)
        {
            settings.directory = ES3.Directory.DataPath;
            settings.path = SavePath + name;
        }
        else if (_mode == StoreMode.Official)
        {
            settings.directory = ES3.Directory.PersistentDataPath;
            settings.path = SavePath + name;
        }

        if (_isAES)
        {
            settings.encryptionType = ES3.EncryptionType.AES;
            settings.encryptionPassword = "xxxxxxxxxxxxxxxx";
        }
        else
        {
            settings.encryptionType = ES3.EncryptionType.None;
        }

        var text = ES3.Load<string>(Key, settings);
        return ReadGameSaveData(text);
    }

    /// <summary>
    /// 解析存档数据
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public SaveData ReadGameSaveData(string text)
    {
        SaveData saveData = new SaveData();
        var jsonData = JsonMapper.ToObject(text);
        if (jsonData.ContainsKey("saveTime"))
        {
            saveData.saveTime = DateTime.Parse(jsonData["saveTime"].ToString());
        }

        if (jsonData.ContainsKey("saveVersion"))
        {
            saveData.saveVersion = int.Parse(jsonData["saveVersion"].ToString());
        }

        if (jsonData.ContainsKey("saveList"))
        {
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
                        saveData.saveList.Add(itemObject);
                    }
                }
            }
        }

        return saveData;
    }

    /// <summary>
    /// 删除某个存档
    /// </summary>
    /// <param name="name"></param>
    public void Delete(string name)
    {
        var settings = new ES3Settings();

        if (_mode == StoreMode.Beta)
        {
            settings.directory = ES3.Directory.DataPath;
            settings.path = SavePath + name;
        }
        else if (_mode == StoreMode.Official)
        {
            settings.directory = ES3.Directory.PersistentDataPath;
            settings.path = SavePath + name;
        }

        ES3.DeleteFile(settings);
    }
}