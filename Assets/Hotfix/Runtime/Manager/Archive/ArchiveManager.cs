using DG.Tweening;
using DG.Tweening.Core;
using LccModel;
using System;
using System.IO;

namespace LccHotfix
{
    internal class ArchiveManager : Module
    {
        public static ArchiveManager Instance { get; } = Entry.GetModule<ArchiveManager>();

        private const string Res = "Res";
        private string _key;
        private UserData _userData;
        private UserSetData _userSetData;



        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
            _key = string.Empty;
            _userData = null;
            _userSetData = null;
        }

        public void ReadArchive(string key)
        {
            _key = key;
            _userData = GetUserData();
            _userSetData = GetUserSetData();
        }


        public bool UserDataExist(string name = "user")
        {
            FileInfo fileInfo = new FileInfo($"{PathUtility.GetPersistentDataPath(Res)}/{name}{AssetSuffix.Lcc}");
            if (fileInfo.Exists)
            {
                return true;
            }
            return false;
        }
        public UserData GetUserData(string name = "user")
        {
            if (_userData != null) return _userData;
            if (UserDataExist(name))
            {
                string value = RijndaelUtility.RijndaelDecrypt(_key, FileUtility.GetAsset($"{PathUtility.GetPersistentDataPath(Res)}/{name}{AssetSuffix.Lcc}").Utf8ToStr());
                _userData = JsonUtility.ToObject<UserData>(value);
                return _userData;
            }
            return _userData = _userData ?? new UserData();
        }
        public void SaveUserData(string name = "user")
        {
            string value = RijndaelUtility.RijndaelEncrypt(_key, JsonUtility.ToJson(_userData));
            FileUtility.SaveAsset($"{PathUtility.GetPersistentDataPath(Res)}/{name}{AssetSuffix.Lcc}", value);
        }
        public void DeleteUserData(string name = "user")
        {
            if (UserDataExist(name))
            {
                FileInfo fileInfo = new FileInfo($"{PathUtility.GetPersistentDataPath(Res)}/{name}{AssetSuffix.Lcc}");
                fileInfo.Delete();
                _userData = null;
            }
        }
        public bool UserSetDataExist()
        {
            FileInfo fileInfo = new FileInfo($"{PathUtility.GetPersistentDataPath(Res)}/UserSet{AssetSuffix.Lcc}");
            if (fileInfo.Exists)
            {
                return true;
            }
            return false;
        }
        public UserSetData GetUserSetData()
        {
            if (_userSetData != null) return _userSetData;
            if (UserSetDataExist())
            {
                string value = RijndaelUtility.RijndaelDecrypt(_key, FileUtility.GetAsset($"{PathUtility.GetPersistentDataPath(Res)}/UserSet{AssetSuffix.Lcc}").Utf8ToStr());
                _userSetData = JsonUtility.ToObject<UserSetData>(value);
                return _userSetData;
            }
            return _userSetData = _userSetData ?? new UserSetData(20, 100, CVType.Chinese, LanguageType.Chinese, DisplayModeType.FullScreen, ResolutionType.Resolution1920x1080);
        }
        public void SaveUserSetData()
        {
            string value = RijndaelUtility.RijndaelEncrypt(_key, JsonUtility.ToJson(_userSetData));
            FileUtility.SaveAsset($"{PathUtility.GetPersistentDataPath(Res)}/UserSet{AssetSuffix.Lcc}", value);
        }
        public void DeleteUserSetData()
        {
            if (UserSetDataExist())
            {
                FileInfo fileInfo = new FileInfo($"{PathUtility.GetPersistentDataPath(Res)}/UserSet{AssetSuffix.Lcc}");
                fileInfo.Delete();
                _userSetData = null;
            }
        }
        public void DeleteAll()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(PathUtility.GetPersistentDataPath(Res));
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete();
                _userData = null;
                _userSetData = null;
            }
        }
    }
}