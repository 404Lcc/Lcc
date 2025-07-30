using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using cfg;
using LccModel;
using Luban;
using RVO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace LccHotfix
{
    public interface ISaveService : IService
    {
        bool IsSaveLoaded { get; }


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

        #region ES3接口

        void Save<T>(string key, T value);


        T Load<T>(string key);

        T Load<T>(string key, T defaultValue);

        //加载到
        void LoadInto<T>(string key, T obj) where T : class;


        void DeleteFile();

        void DeleteDirectory();

        void DeleteKey(string key);

        bool KeyExists(string key);

        bool FileExists();

        bool DirectoryExists();

        string[] GetKeys();

        string[] GetFiles();

        string[] GetDirectories();

        #endregion
    }
}