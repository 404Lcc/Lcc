using System;
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
        public string TypeName { get; }
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
        ISave Flush();
    }

    // 数据转换接口
    public interface ISaveConverter<T> : ISaveData where T : ISave
    {
        T Save { get; set; }
    }

    public class SaveData
    {
        //元数据
        public DateTime saveTime;
        public int saveVersion; // 数据版本控制

        //存档数据
        public List<ISave> saveList;

        public SaveData()
        {
            saveList = new List<ISave>();
        }

        /// <summary>
        /// 创建新存档
        /// </summary>
        public void CreateNewSaveData()
        {
            saveTime = DateTime.Now;
            saveVersion = 1;
        }

        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="data"></param>
        public void LoadSaveData(SaveData data)
        {
            saveTime = data.saveTime;
            saveVersion = data.saveVersion;
            saveList = data.saveList;
        }

        /// <summary>
        /// 获取存档类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSave<T>() where T : ISave
        {
            ISave save = null;
            foreach (var item in saveList)
            {
                if (item.GetType() == typeof(T))
                {
                    save = (T)item;
                    break;
                }
            }

            if (save == null)
            {
                save = Activator.CreateInstance<T>();
                save.Init();
            }

            return (T)save;
        }
    }

    [Serializable]
    public class GameSaveData : SaveData
    {
        //运行时数据
        private Dictionary<Type, ISaveData> _runDataDict; //运行时类型-运行时数据

        public GameSaveData() : base()
        {
            _runDataDict = new Dictionary<Type, ISaveData>();
        }

        /// <summary>
        /// 获取存档转化数据
        /// </summary>
        /// <typeparam name="TRunData"></typeparam>
        /// <typeparam name="TSave"></typeparam>
        /// <returns></returns>
        public TRunData GetSaveConverterData<TRunData, TSave>() where TRunData : ISaveConverter<TSave>, new() where TSave : ISave
        {
            Type type = typeof(TRunData);
            if (_runDataDict.ContainsKey(type))
            {
                return (TRunData)_runDataDict[type];
            }
            else
            {
                TRunData data = new TRunData();
                data.Save = GetSave<TSave>();
                data.Init();
                _runDataDict.Add(data.GetType(), data);
                return data;
            }
        }

        public Dictionary<Type, ISaveData> GetRunDataDict()
        {
            return _runDataDict;
        }
    }

    [Serializable]
    public class GlobalGameSaveData : SaveData
    {
    }

    internal class SaveManager : Module, ISaveService
    {
        public Dictionary<Type, ISavePipeline> saveDict = new Dictionary<Type, ISavePipeline>();

        private ISaveHelper _saveHelper;

        //当前加载存档
        private string _currentGameSaveName;

        private GameSaveData _currentGameSaveData;

        //通用默认存档 一般是游戏设置之类的
        private const string GlobalGameSaveName = "globalSave.dat";
        private GlobalGameSaveData _globalGameSaveData;

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
        /// 设置存储路径
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
        public bool ValidateSaveFile(SaveData data)
        {
            //校验逻辑示例
            return data != null && data.saveTime < DateTime.Now.AddDays(1) && data.saveList.Count > 0;
        }

        /// <summary>
        /// 获取本地所有存档文件
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllSaveFile()
        {
            return _saveHelper.GetAllSaveFile();
        }

        /// <summary>
        /// 检测有没有某个存档
        /// </summary>
        /// <returns></returns>
        public bool CheckHaveSaveFile(string name)
        {
            return _saveHelper.CheckHaveSaveFile(name);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (CheckHaveSaveFile(GlobalGameSaveName))
            {
                LoadGlobalGameSaveFile();
            }
            else
            {
                CreateGlobalGameSaveFile();
            }
        }

        /// <summary>
        /// 创建全局存档
        /// </summary>
        private void CreateGlobalGameSaveFile()
        {
            _globalGameSaveData = new GlobalGameSaveData();
            _globalGameSaveData.CreateNewSaveData();
            SaveGlobalGameSaveFile();
        }

        /// <summary>
        /// 读取全局存档
        /// </summary>
        private void LoadGlobalGameSaveFile()
        {
            if (CheckHaveSaveFile(GlobalGameSaveName))
            {
                _globalGameSaveData = new GlobalGameSaveData();
                _globalGameSaveData.LoadSaveData(_saveHelper.LoadSaveFile(GlobalGameSaveName));
            }
        }

        /// <summary>
        /// 保存全局存档
        /// </summary>
        public void SaveGlobalGameSaveFile()
        {
            _globalGameSaveData.saveTime = DateTime.Now;
            _saveHelper.SaveFile(GlobalGameSaveName, _globalGameSaveData);
        }

        /// <summary>
        /// 获取全局存档的某个存档数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetGlobalGameSaveFileSave<T>() where T : ISave
        {
            return _globalGameSaveData.GetSave<T>();
        }

        /// <summary>
        /// 创建新存档
        /// </summary>
        public void CreateSaveFile(string name)
        {
            _currentGameSaveName = name;
            _currentGameSaveData = new GameSaveData();
            _currentGameSaveData.CreateNewSaveData();
            IsSaveLoaded = true;
            foreach (var item in saveDict.Values)
            {
                item.InitData(_currentGameSaveData);
            }

            SaveFile();
        }

        /// <summary>
        /// 读取存档
        /// </summary>
        public void LoadSaveFile(string name)
        {
            if (CheckHaveSaveFile(name))
            {
                _currentGameSaveName = name;
                _currentGameSaveData = new GameSaveData();
                _currentGameSaveData.LoadSaveData(_saveHelper.LoadSaveFile(name));
                IsSaveLoaded = true;
                foreach (var item in saveDict.Values)
                {
                    item.InitData(_currentGameSaveData);
                }
            }
        }

        /// <summary>
        /// 保存当前存档
        /// </summary>
        public void SaveFile()
        {
            if (!IsSaveLoaded)
                return;

            _currentGameSaveData.saveTime = DateTime.Now;
            _currentGameSaveData.saveList.Clear();

            foreach (var item in _currentGameSaveData.GetRunDataDict().Values)
            {
                var save = item.Flush();
                _currentGameSaveData.saveList.Add(save);
            }

            _saveHelper.SaveFile(_currentGameSaveName, _currentGameSaveData);
        }

        /// <summary>
        /// 获取存档转化数据
        /// </summary>
        /// <typeparam name="TRunData"></typeparam>
        /// <typeparam name="TSave"></typeparam>
        /// <returns></returns>
        public TRunData GetSaveConverterData<TRunData, TSave>() where TRunData : ISaveConverter<TSave>, new() where TSave : ISave
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("Save file has not been loaded yet");
                return default;
            }

            return _currentGameSaveData.GetSaveConverterData<TRunData, TSave>();
        }
    }
}