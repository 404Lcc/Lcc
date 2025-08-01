﻿using System;
using System.Collections.Generic;
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

        private ISaveHelper _saveHelper;
        private GameSaveData _gameSaveData;

        public bool IsSaveLoaded { get; private set; }


        public SaveManager()
        {
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


        public void SetSaveHelper(ISaveHelper saveHelper)
        {
            _saveHelper = saveHelper;
        }

        /// <summary>
        /// 设置全局加密方式
        /// </summary>
        public void SetEncryption(bool isAES)
        {
            _saveHelper.SetEncryption(isAES);
        }


        /// <summary>
        /// 设置存储路径 游戏启动时修改
        /// </summary>
        public void SetStorePath(StoreMode mode)
        {
            _saveHelper.SetStorePath(mode);
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
            return _saveHelper.KeyExists("GameSaveData");
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
                _gameSaveData = _saveHelper.Load<GameSaveData>("GameSaveData");
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

            _saveHelper.Save("GameSaveData", _gameSaveData);
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
    }
}