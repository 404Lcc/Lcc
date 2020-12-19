//using DG.Tweening;
//using DG.Tweening.Core;
using LitJson;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace LccHotfix
{
    public class GameDataManager : Singleton<GameDataManager>
    {
        public string key = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
        public UserData userData;
        public UserSetData userSetData;
        public void InitManager()
        {
            userData = GetUserData();
            userSetData = GetUserSetData();
        }
        public bool UserDataExist(string name = "user")
        {
            FileInfo info = new FileInfo($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}{name}.lcc");
            if (info.Exists)
            {
                return true;
            }
            return false;
        }
        public UserData GetUserData(string name = "user")
        {
            if (UserDataExist(name))
            {
                string value = RijndaelUtil.RijndaelDecrypt(key, Encoding.UTF8.GetString(FileUtil.GetAsset($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}{name}.lcc")));
                return JsonMapper.ToObject<UserData>(value);
            }
            return new UserData();
        }
        public void SaveUserData(string name = "user")
        {
            string value = RijndaelUtil.RijndaelEncrypt(key, JsonMapper.ToJson(userData));
            FileUtil.SaveAsset($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}{name}.lcc", Encoding.UTF8.GetBytes(value));
        }
        public void DeleteUserData(string name = "user")
        {
            if (UserDataExist(name))
            {
                FileInfo info = new FileInfo($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}{name}.lcc");
                info.Delete();
                userData = null;
            }
        }
        public bool UserSetDataExist()
        {
            FileInfo info = new FileInfo($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}UserSet.lcc");
            if (info.Exists)
            {
                return true;
            }
            return false;
        }
        public UserSetData GetUserSetData()
        {
            if (UserSetDataExist())
            {
                string value = RijndaelUtil.RijndaelDecrypt(key, Encoding.UTF8.GetString(FileUtil.GetAsset($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}UserSet.lcc")));
                return JsonMapper.ToObject<UserSetData>(value);
            }
            return new UserSetData(20, 100, CVType.Chinese, LccModel.LanguageType.Chinese, DisplayModeType.FullScreen, ResolutionType.Resolution1920x1080);
        }
        public void SaveUserSetData()
        {
            string value = RijndaelUtil.RijndaelEncrypt(key, JsonMapper.ToJson(userSetData));
            FileUtil.SaveAsset($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}UserSet.lcc", Encoding.UTF8.GetBytes(value));
        }
        public void DeleteUserSetData()
        {
            if (UserSetDataExist())
            {
                FileInfo info = new FileInfo($"{PathUtil.GetPath(PathType.PersistentDataPath, "Res")}UserSet.lcc");
                info.Delete();
                userSetData = null;
            }
        }
        public void DeleteAll()
        {
            DirectoryInfo info = new DirectoryInfo($"{Application.persistentDataPath}/Res");
            if (info.Exists)
            {
                info.Delete();
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