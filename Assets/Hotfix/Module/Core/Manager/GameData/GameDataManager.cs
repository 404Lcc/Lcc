//using DG.Tweening;
//using DG.Tweening.Core;
using System;
using System.IO;

namespace LccHotfix
{
    public class GameDataManager : Singleton<GameDataManager>
    {
        public string key;
        public UserData userData;
        public UserSetData userSetData;
        public void InitManager(string key)
        {
            this.key = key;
            userData = GetUserData();
            userSetData = GetUserSetData();
        }
        public bool UserDataExist(string name = "user")
        {
            FileInfo fileInfo = new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/{name}.lcc");
            if (fileInfo.Exists)
            {
                return true;
            }
            return false;
        }
        public UserData GetUserData(string name = "user")
        {
            if (UserDataExist(name))
            {
                string value = RijndaelUtil.RijndaelDecrypt(key, FileUtil.GetAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/{name}.lcc")).GetString();
                userData = JsonUtil.ToObject<UserData>(value);
                return userData;
            }
            return userData = userData ?? new UserData();
        }
        public void SaveUserData(string name = "user")
        {
            string value = RijndaelUtil.RijndaelEncrypt(key, JsonUtil.ToJson(userData));
            FileUtil.SaveAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/{name}.lcc", value);
        }
        public void DeleteUserData(string name = "user")
        {
            if (UserDataExist(name))
            {
                FileInfo fileInfo = new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/{name}.lcc");
                fileInfo.Delete();
                userData = null;
            }
        }
        public bool UserSetDataExist()
        {
            FileInfo fileInfo = new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/UserSet.lcc");
            if (fileInfo.Exists)
            {
                return true;
            }
            return false;
        }
        public UserSetData GetUserSetData()
        {
            if (UserSetDataExist())
            {
                string value = RijndaelUtil.RijndaelDecrypt(key, FileUtil.GetAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/UserSet.lcc")).GetString();
                userSetData = JsonUtil.ToObject<UserSetData>(value);
                return userSetData;
            }
            return userSetData = userSetData ?? new UserSetData(20, 100, CVType.Chinese, LccModel.LanguageType.Chinese, DisplayModeType.FullScreen, ResolutionType.Resolution1920x1080);
        }
        public void SaveUserSetData()
        {
            string value = RijndaelUtil.RijndaelEncrypt(key, JsonUtil.ToJson(userSetData));
            FileUtil.SaveAsset($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/UserSet.lcc", value);
        }
        public void DeleteUserSetData()
        {
            if (UserSetDataExist())
            {
                FileInfo fileInfo = new FileInfo($"{PathUtil.GetPersistentDataPath(LccConst.Res)}/UserSet.lcc");
                fileInfo.Delete();
                userSetData = null;
            }
        }
        public void DeleteAll()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(PathUtil.GetPersistentDataPath(LccConst.Res));
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete();
                userData = null;
                userSetData = null;
            }
        }
        //public void SetValue(DOGetter<float> getter, DOSetter<float> setter, int value, int timer, bool isSave, bool isOpenPanel = false, PanelType[] types = null, Action callback = null)
        //{
        //    if (isOpenPanel)
        //    {
        //    }
        //    DOTween.To(getter, setter, value, timer).OnComplete(() =>
        //    {
        //        callback?.Invoke();
        //        if (isSave)
        //        {
        //        }
        //    });
        //}
    }
}