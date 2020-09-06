//using DG.Tweening;
//using DG.Tweening.Core;
using System;
using System.IO;
using UnityEngine;

namespace Hotfix
{
    public class GameDataManager : Singleton<GameDataManager>
    {
        public void InitDataList()
        {
            if (DataListExist())
            {
                GetDataList();
            }
        }

        public bool DataListExist()
        {
            FileInfo info = new FileInfo(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + "DataList.lcc");
            if (info.Exists)
            {
                return true;
            }
            return false;
        }
        public void GetDataList()
        {
            if (DataListExist())
            {
                JsonUtil.ToDataList(JsonUtil.GetDataList(GameUtil.GetPath(PathType.PersistentDataPath, "Res"), "DataList.lcc"));
            }
        }
        public void SaveDataList()
        {
            JsonUtil.SetDataList(GameUtil.GetPath(PathType.PersistentDataPath, "Res"), "DataList.lcc", JsonUtil.ToDataListInstance());
        }
        public void DeleteDataList()
        {
            if (DataListExist())
            {
                FileInfo info = new FileInfo(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + "DataList.lcc");
                info.Delete();
            }
        }
        public int GetDataListCount()
        {
            if (DataListExist())
            {
                GetDataList();
                return DataList.dataList.Count;
            }
            return 0;
        }

        public bool UserExist(string name)
        {
            FileInfo info = new FileInfo(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + name + ".lcc");
            if (info.Exists)
            {
                return true;
            }
            return false;
        }
        public void GetUserData(string name)
        {
            if (UserExist(name))
            {
                JsonUtil.ToUserData(JsonUtil.GetUserData(GameUtil.GetPath(PathType.PersistentDataPath, "Res"), name + ".lcc"));
            }
        }
        public void SaveUserData(string name)
        {
            JsonUtil.SetUserData(GameUtil.GetPath(PathType.PersistentDataPath, "Res"), name + ".lcc", JsonUtil.ToUserDataInstance());
        }
        public void DeleteUserData(string name)
        {
            if (UserExist(name))
            {
                FileInfo info = new FileInfo(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + name + ".lcc");
                info.Delete();
            }
        }

        public bool UserSetExist()
        {
            FileInfo info = new FileInfo(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + "UserSet.lcc");
            if (info.Exists)
            {
                return true;
            }
            return false;
        }
        public void GetUserSetData()
        {
            if (UserSetExist())
            {
                JsonUtil.ToUserSetData(JsonUtil.GetUserSetData(GameUtil.GetPath(PathType.PersistentDataPath, "Res"), "UserSet.lcc"));
            }
        }
        public void SaveUserSetData()
        {
            JsonUtil.SetUserSetData(GameUtil.GetPath(PathType.PersistentDataPath, "Res"), "UserSet.lcc", JsonUtil.ToUserSetDataInstance());
        }
        public void DeleteUserSetData()
        {
            if (UserSetExist())
            {
                FileInfo info = new FileInfo(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + "UserSet.lcc");
                info.Delete();
            }
        }

        public void DeleteFileAll()
        {
            DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/Res");
            if (info.Exists)
            {
                info.Delete();
            }
        }

        //public void SetValue(DOGetter<float> getter, DOSetter<float> setter, int value, int timer, bool save, bool panel = false, PanelType[] types = null, Action action = null)
        //{
        //    if (panel)
        //    {
        //    }
        //    DOTween.To(getter, setter, value, timer).OnComplete(() =>
        //    {
        //        action();
        //        if (save)
        //        {
        //        }
        //    });
        //}
    }
}