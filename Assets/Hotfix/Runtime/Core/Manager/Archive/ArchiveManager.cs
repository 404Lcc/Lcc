using DG.Tweening;
using DG.Tweening.Core;
using LccModel;
using System;
using System.IO;

namespace LccHotfix
{
    public class ArchiveManager : AObjectBase
    {
        public static ArchiveManager Instance { get; set; }

        private const string Res = "Res";
        private string _key;
        private UserData _userData;
        private UserSetData _userSetData;

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            _key = string.Empty;
            _userData = null;
            _userSetData = null;

            Instance = null;
        }


        public void ReadArchive(string key)
        {
            _key = key;
            _userData = GetUserData();
            _userSetData = GetUserSetData();
        }


        public bool UserDataExist(string name = "user")
        {
            FileInfo fileInfo = new FileInfo($"{PathHelper.GetPersistentDataPath(Res)}/{name}{AssetSuffix.Lcc}");
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
                string value = RijndaelHelper.RijndaelDecrypt(_key, FileHelper.GetAsset($"{PathHelper.GetPersistentDataPath(Res)}/{name}{AssetSuffix.Lcc}").Utf8ToStr());
                _userData = JsonHelper.ToObject<UserData>(value);
                return _userData;
            }
            return _userData = _userData ?? new UserData();
        }
        public void SaveUserData(string name = "user")
        {
            string value = RijndaelHelper.RijndaelEncrypt(_key, JsonHelper.ToJson(_userData));
            FileHelper.SaveAsset($"{PathHelper.GetPersistentDataPath(Res)}/{name}{AssetSuffix.Lcc}", value);
        }
        public void DeleteUserData(string name = "user")
        {
            if (UserDataExist(name))
            {
                FileInfo fileInfo = new FileInfo($"{PathHelper.GetPersistentDataPath(Res)}/{name}{AssetSuffix.Lcc}");
                fileInfo.Delete();
                _userData = null;
            }
        }
        public bool UserSetDataExist()
        {
            FileInfo fileInfo = new FileInfo($"{PathHelper.GetPersistentDataPath(Res)}/UserSet{AssetSuffix.Lcc}");
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
                string value = RijndaelHelper.RijndaelDecrypt(_key, FileHelper.GetAsset($"{PathHelper.GetPersistentDataPath(Res)}/UserSet{AssetSuffix.Lcc}").Utf8ToStr());
                _userSetData = JsonHelper.ToObject<UserSetData>(value);
                return _userSetData;
            }
            return _userSetData = _userSetData ?? new UserSetData(20, 100, CVType.Chinese, LanguageType.Chinese, DisplayModeType.FullScreen, ResolutionType.Resolution1920x1080);
        }
        public void SaveUserSetData()
        {
            string value = RijndaelHelper.RijndaelEncrypt(_key, JsonHelper.ToJson(_userSetData));
            FileHelper.SaveAsset($"{PathHelper.GetPersistentDataPath(Res)}/UserSet{AssetSuffix.Lcc}", value);
        }
        public void DeleteUserSetData()
        {
            if (UserSetDataExist())
            {
                FileInfo fileInfo = new FileInfo($"{PathHelper.GetPersistentDataPath(Res)}/UserSet{AssetSuffix.Lcc}");
                fileInfo.Delete();
                _userSetData = null;
            }
        }
        public void DeleteAll()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(PathHelper.GetPersistentDataPath(Res));
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete();
                _userData = null;
                _userSetData = null;
            }
        }
        public void SetValue(DOGetter<float> getter, DOSetter<float> setter, int value, int timer, bool isSave, bool isOpenPanel = false, Action completed = null)
        {
            if (isOpenPanel)
            {
            }
            DOTween.To(getter, setter, value, timer).OnComplete(() =>
            {
                completed?.Invoke();
                if (isSave)
                {
                }
            });
        }
    }
}