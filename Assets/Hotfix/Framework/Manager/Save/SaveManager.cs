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

    [Serializable]
    public class GameSaveData
    {
        //元数据
        public DateTime saveTime;
        public int saveVersion; // 数据版本控制

        //存档数据
        public List<ISave> saveList;

        //运行时数据
        private Dictionary<Type, ISaveData> _dataDict;

        public GameSaveData()
        {
            saveList = new List<ISave>();
            _dataDict = new Dictionary<Type, ISaveData>();
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
        /// 获取存档转化数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TSave"></typeparam>
        /// <returns></returns>
        public T GetSaveConverterData<T, TSave>() where T : ISaveConverter<TSave>, new() where TSave : ISave
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

        /// <summary>
        /// 获取存档类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetSave<T>() where T : ISave
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

        public Dictionary<Type, ISaveData> GetRunDataDict()
        {
            return _dataDict;
        }
    }

    internal class SaveManager : Module, ISaveService
    {
        public Dictionary<Type, ISavePipeline> saveDict = new Dictionary<Type, ISavePipeline>();

        private ISaveHelper _saveHelper;
        private string _currentGameSaveName;
        private GameSaveData _currentGameSaveData;

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
        public bool ValidateSaveData(GameSaveData data)
        {
            //校验逻辑示例
            return data != null && data.saveTime < DateTime.Now.AddDays(1) && data.saveList.Count > 0;
        }

        /// <summary>
        /// 获取本地所有存档文件
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllSaveFiles()
        {
            return _saveHelper.GetFiles();
        }

        /// <summary>
        /// 检测有没有某个存档
        /// </summary>
        /// <returns></returns>
        public bool CheckHaveSaveData(string name)
        {
            return _saveHelper.CheckHaveSaveData(name);
        }

        /// <summary>
        /// 创建新存档
        /// </summary>
        public void CreateNewSaveData(string name)
        {
            _currentGameSaveName = name;
            _currentGameSaveData = new GameSaveData();
            _currentGameSaveData.CreateNewSaveData();
            IsSaveLoaded = true;
            foreach (var item in saveDict.Values)
            {
                item.InitData(_currentGameSaveData);
            }

            Save();
        }

        /// <summary>
        /// 读取存档
        /// </summary>
        public void Load(string name)
        {
            if (CheckHaveSaveData(name))
            {
                _currentGameSaveData = _saveHelper.Load(name);
                IsSaveLoaded = true;
                foreach (var item in saveDict.Values)
                {
                    item.InitData(_currentGameSaveData);
                }
            }
        }

        /// <summary>
        /// 保存存档
        /// </summary>
        public void Save()
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

            _saveHelper.Save(_currentGameSaveName, _currentGameSaveData);
        }

        /// <summary>
        /// 获取存档转化数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TSave"></typeparam>
        /// <returns></returns>
        public T GetSaveConverterData<T, TSave>() where T : ISaveConverter<TSave>, new() where TSave : ISave
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("Save file has not been loaded yet");
                return default;
            }

            return _currentGameSaveData.GetSaveConverterData<T, TSave>();
        }
    }
}