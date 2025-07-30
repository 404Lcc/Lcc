using System;
using System.Collections.Generic;
using ES3Internal;
using ES3Types;

using UnityEngine;

namespace LccHotfix
{
    public enum StoreMode
    {
        Beta,
        Official
    }

    // 存档数据
    public interface ISave
    {
        void Init();
    }

    //存档管线
    public interface ISavePipeline
    {
        void InitData(GameSaveData gameSaveData);
    }

    //运行时数据
    public interface ISaveData
    {
        /// <summary>
        /// 从存档初始化数据
        /// </summary>
        void Init();

        /// <summary>
        /// 填充数据到存档
        /// </summary>
        void Flush();
    }

    // 数据转换接口
    public interface ISaveConverter<T> : ISaveData where T : ISave
    {
        T Save { get; set; }
    }

    [Serializable]
    public class GameSaveData
    {
        //元数据
        public DateTime saveTime;
        public int saveVersion; // 数据版本控制

        // 模块数据存储
        public Dictionary<Type, ISave> saveDict;

        private Dictionary<Type, ISaveData> _dataDict;

        public void InitData()
        {
            saveTime = DateTime.Now;
            saveVersion = 1;
            saveDict = new Dictionary<Type, ISave>();
            _dataDict = new Dictionary<Type, ISaveData>();
        }

        public T GetSaveData<T, TSave>() where T : ISaveConverter<TSave>, new() where TSave : ISave
        {
            Type type = typeof(T);
            if (_dataDict.ContainsKey(type))
            {
                return (T)_dataDict[type];
            }
            else
            {
                T data = new T();
                data.Save = GetSave<TSave>();
                data.Init();
                _dataDict.Add(data.GetType(), data);
                return data;
            }
        }

        private T GetSave<T>() where T : ISave
        {
            saveDict.TryGetValue(typeof(T), out var module);
            if (module == null)
            {
                module = Activator.CreateInstance<T>();
                module.Init();
            }

            return (T)module;
        }

        public Dictionary<Type, ISaveData> GetRunDataDict()
        {
            return _dataDict;
        }
    }

    internal class SaveManager : Module, ISaveService
    {
        public Dictionary<Type, ISavePipeline> saveDict = new Dictionary<Type, ISavePipeline>();

        private readonly string SavePath = "../SaveDatas/";
        private readonly string FileName = "saveData.lcc";
        private ES3Settings _settings;

        private GameSaveData _gameSaveData;

        public bool IsSaveLoaded { get; private set; }


        public SaveManager()
        {
            _settings = new ES3Settings();
            SetEncryption(true);
            SetStorePath(StoreMode.Official);

            foreach (Type item in Main.CodeTypesService.GetTypes(typeof(ModelAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(ModelAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    ModelAttribute modelAttribute = (ModelAttribute)atts[0];

                    var obj = Main.ModelService.GetModel(item);
                    if (obj != null && obj as ISavePipeline != null)
                    {
                        saveDict.Add(item, obj as ISavePipeline);
                    }
                }
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }




        /// <summary>
        /// 设置全局加密方式
        /// </summary>
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


        /// <summary>
        /// 设置存储路径 游戏启动时修改
        /// </summary>
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

        /// <summary>
        /// 校验存档
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool ValidateSaveData(GameSaveData data)
        {
            //校验逻辑示例
            return data != null && data.saveTime < DateTime.Now.AddDays(1) && data.saveDict.Count > 0;
        }

        /// <summary>
        /// 检测有没有存档
        /// </summary>
        /// <returns></returns>
        public bool CheckHaveSaveData()
        {
            return KeyExists("GameSaveData");
        }

        /// <summary>
        /// 创建新存档
        /// </summary>
        public void CreateNewSaveData()
        {
            _gameSaveData = new GameSaveData();
            _gameSaveData.InitData();
            IsSaveLoaded = true;
            foreach (var item in saveDict.Values)
            {
                item.InitData(_gameSaveData);
            }
        }

        /// <summary>
        /// 读取存档
        /// </summary>
        public void Load()
        {
            if (CheckHaveSaveData())
            {
                _gameSaveData = Load<GameSaveData>("GameSaveData");
                IsSaveLoaded = true;
                foreach (var item in saveDict.Values)
                {
                    item.InitData(_gameSaveData);
                }
            }
        }

        /// <summary>
        /// 保存存档
        /// </summary>
        public void Save()
        {
            _gameSaveData.saveTime = DateTime.Now;

            foreach (var item in _gameSaveData.GetRunDataDict().Values)
            {
                item.Flush();
            }

            Save("GameSaveData", _gameSaveData);
        }

        public T GetSaveData<T, TSave>() where T : ISaveConverter<TSave>, new() where TSave : ISave
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("Save file has not been loaded yet");
                return default;
            }

            return _gameSaveData.GetSaveData<T, TSave>();
        }

        #region ES3接口

        public void Save<T>(string key, T value)
        {
            ES3.Save<T>(key, value, _settings);
        }



        public T Load<T>(string key)
        {
            return ES3.Load<T>(key, _settings);
        }

        public T Load<T>(string key, T defaultValue)
        {
            return ES3.Load<T>(key, defaultValue, _settings);
        }

        //加载到
        public void LoadInto<T>(string key, T obj) where T : class
        {
            ES3.LoadInto<T>(key, obj, _settings);
        }


        public void DeleteFile()
        {
            ES3.DeleteFile(_settings);
        }

        public void DeleteDirectory()
        {
            ES3.DeleteDirectory(_settings);
        }

        public void DeleteKey(string key)
        {
            ES3.DeleteKey(key, _settings);
        }

        public bool KeyExists(string key)
        {
            return ES3.KeyExists(key, _settings);
        }

        public bool FileExists()
        {
            return ES3.FileExists(_settings);
        }

        public bool DirectoryExists()
        {
            return ES3.DirectoryExists(_settings);
        }

        public string[] GetKeys()
        {
            return ES3.GetKeys(_settings);
        }

        public string[] GetFiles()
        {
            return ES3.GetFiles(_settings);
        }

        public string[] GetDirectories()
        {
            return ES3.GetDirectories(_settings);
        }

        #endregion
    }
}