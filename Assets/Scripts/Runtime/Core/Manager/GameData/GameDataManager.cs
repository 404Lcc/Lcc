using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.IO;

namespace LccModel
{
    public class GameDataManager : Singleton<GameDataManager>
    {
        public const string Suffix = ".lcc";
        private string _key;
        private UserData _userData;
        private UserSetData _userSetData;
        public void InitManager(string key)
        {
            _key = key;
            _userData = GetUserData();
            _userSetData = GetUserSetData();
        }
        public bool UserDataExist(string name = "user")
        {
            FileInfo fileInfo = new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/{name}{Suffix}");
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
                string value = RijndaelUtil.RijndaelDecrypt(_key, FileUtil.GetAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/{name}{Suffix}").GetString());
                _userData = JsonUtil.ToObject<UserData>(value);
                return _userData;
            }
            return _userData = _userData ?? new UserData();
        }
        public void SaveUserData(string name = "user")
        {
            string value = RijndaelUtil.RijndaelEncrypt(_key, JsonUtil.ToJson(_userData));
            FileUtil.SaveAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/{name}{Suffix}", value);
        }
        public void DeleteUserData(string name = "user")
        {
            if (UserDataExist(name))
            {
                FileInfo fileInfo = new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/{name}{Suffix}");
                fileInfo.Delete();
                _userData = null;
            }
        }
        public bool UserSetDataExist()
        {
            FileInfo fileInfo = new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/UserSet{Suffix}");
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
                string value = RijndaelUtil.RijndaelDecrypt(_key, FileUtil.GetAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/UserSet{Suffix}").GetString());
                _userSetData = JsonUtil.ToObject<UserSetData>(value);
                return _userSetData;
            }
            return _userSetData = _userSetData ?? new UserSetData(20, 100, CVType.Chinese, LccModel.LanguageType.Chinese, DisplayModeType.FullScreen, ResolutionType.Resolution1920x1080);
        }
        public void SaveUserSetData()
        {
            string value = RijndaelUtil.RijndaelEncrypt(_key, JsonUtil.ToJson(_userSetData));
            FileUtil.SaveAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/UserSet{Suffix}", value);
        }
        public void DeleteUserSetData()
        {
            if (UserSetDataExist())
            {
                FileInfo fileInfo = new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/UserSet{Suffix}");
                fileInfo.Delete();
                _userSetData = null;
            }
        }
        public void DeleteAll()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(PathUtil.GetPersistentDataPath(LccConst.Res));
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete();
                _userData = null;
                _userSetData = null;
            }
        }
        public void SetValue(DOGetter<float> getter, DOSetter<float> setter, int value, int timer, bool isSave, bool isOpenPanel = false, PanelType[] types = null, Action completed = null)
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