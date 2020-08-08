//using DG.Tweening;
//using DG.Tweening.Core;
using System;
using System.IO;
using UnityEngine;

namespace Hotfix
{
    public class GameDataManager : MonoBehaviour
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
            GameUtil.CreateDirectory(Application.persistentDataPath, "Res");
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
            GameUtil.CreateDirectory(Application.persistentDataPath, "Res");
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
                return DataList.datalist.Length;
            }
            return 0;
        }

        public bool UserExist(string dataname)
        {
            GameUtil.CreateDirectory(Application.persistentDataPath, "Res");
            FileInfo info = new FileInfo(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + dataname + ".lcc");
            if (info.Exists)
            {
                return true;
            }
            return false;
        }
        public void GetUserData(string dataname)
        {
            if (UserExist(dataname))
            {
                JsonUtil.ToUserData(JsonUtil.GetUserData(GameUtil.GetPath(PathType.PersistentDataPath, "Res"), dataname + ".lcc"));
            }
        }
        public void SaveUserData(string dataname)
        {
            GameUtil.CreateDirectory(Application.persistentDataPath, "Res");
            JsonUtil.SetUserData(GameUtil.GetPath(PathType.PersistentDataPath, "Res"), dataname + ".lcc", JsonUtil.ToUserDataInstance());
        }
        public void DeleteUserData(string dataname)
        {
            if (UserExist(dataname))
            {
                FileInfo info = new FileInfo(GameUtil.GetPath(PathType.PersistentDataPath, "Res") + dataname + ".lcc");
                info.Delete();
            }
        }

        public bool UserSetExist()
        {
            GameUtil.CreateDirectory(Application.persistentDataPath, "Res");
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
            GameUtil.CreateDirectory(Application.persistentDataPath, "Res");
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

        //public void SetValue(DOGetter<float> getter, DOSetter<float> setter, int value, int timer, bool bsave, bool bpanel = false, PanelType[] types = null, Action action = null)
        //{
        //    if (bpanel)
        //    {
        //    }
        //    DOTween.To(getter, setter, value, timer).OnComplete(() =>
        //    {
        //        action();
        //        if (bsave)
        //        {
        //        //SaveUserData(UserData.dataname);
        //    }
        //    });
        //}
    }
}