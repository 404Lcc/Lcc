using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Model
{
    public class PanelManager : Singleton<PanelManager>
    {
        public Hashtable panels = new Hashtable();
        /// <summary>
        /// 面板是否存在
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool PanelExist(PanelType type)
        {
            if (panels.ContainsKey(type))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 创建面板
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PanelInfo CreatePanel(PanelType type)
        {
            PanelInfo info = new PanelInfo();
            info.state = InfoState.Close;
            info.container = ContainerManager.Instance.CreateContainer(GameUtil.ConvertPanelTypeToString(type), false);
            if (info.container == null) return null;
            info.type = type;
            Assembly assembly = type.GetType().Assembly;
            Type classType = assembly.GetType("Model." + GameUtil.ConvertPanelTypeToString(type));
            if (classType != null)
            {
                LccViewFactory.CreateView(classType, info.container);
            }
            info.ClosePanel();
            panels.Add(type, info);
            return info;
        }
        /// <summary>
        /// 删除面板
        /// </summary>
        /// <param name="type"></param>
        public void ClearPanel(PanelType type)
        {
            if (PanelExist(type))
            {
                ContainerManager.Instance.RemoveContainer(GameUtil.ConvertPanelTypeToString(type));
                PanelInfo info = GetPanelInfo(type);
                GameUtil.SafeDestroy(info.container);
                panels.Remove(type);
            }
        }
        /// <summary>
        /// 删除面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public void ClearPanel(PanelType[] types)
        {
            foreach (PanelType item in types)
            {
                ClearPanel(item);
            }
        }
        /// <summary>
        /// 删除剩下所有面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public void ClearExceptPanel(PanelType[] types)
        {
            List<PanelType> typeList = new List<PanelType>(types);
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                if (!typeList.Contains(item))
                {
                    ClearPanel(item);
                }
            }
        }
        /// <summary>
        /// 删除全部面板
        /// </summary>
        public void ClearAllPanels()
        {
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                ClearPanel(item);
            }
        }
        /// <summary>
        /// 删除全部打开的面板
        /// </summary>
        /// <returns></returns>
        public int ClearOpenPanels()
        {
            int number = 0;
            IDictionaryEnumerator enumerator = panels.GetEnumerator();
            while (enumerator.MoveNext())
            {
                PanelInfo info = enumerator.Value as PanelInfo;
                if (info.state == InfoState.Open)
                {
                    ClearPanel(info.type);
                    number++;
                }
            }
            return number;
        }
        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PanelInfo OpenPanel(PanelType type)
        {
            if (PanelExist(type))
            {
                PanelInfo info = GetPanelInfo(type);
                info.OpenPanel();
                return info;
            }
            else
            {
                PanelInfo info = CreatePanel(type);
                info.OpenPanel();
                return info;
            }
        }
        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public PanelInfo[] OpenPanel(PanelType[] types)
        {
            List<PanelInfo> infoList = new List<PanelInfo>();
            foreach (PanelType item in types)
            {
                infoList.Add(OpenPanel(item));
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 打开剩下所有面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public PanelInfo[] OpenExceptPanel(PanelType[] types)
        {
            List<PanelInfo> infoList = new List<PanelInfo>();
            List<PanelType> typeList = new List<PanelType>(types);
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                if (!typeList.Contains(item))
                {
                    infoList.Add(OpenPanel(item));
                }
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 打开全部面板
        /// </summary>
        public PanelInfo[] OpenAllPanels()
        {
            List<PanelInfo> infoList = new List<PanelInfo>();
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                infoList.Add(OpenPanel(item));
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PanelInfo ClosePanel(PanelType type)
        {
            if (PanelExist(type))
            {
                PanelInfo info = GetPanelInfo(type);
                info.ClosePanel();
                return info;
            }
            else
            {
                PanelInfo info = CreatePanel(type);
                info.ClosePanel();
                return info;
            }
        }
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public PanelInfo[] ClosePanel(PanelType[] types)
        {
            List<PanelInfo> infoList = new List<PanelInfo>();
            foreach (PanelType item in types)
            {
                infoList.Add(ClosePanel(item));
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 隐藏剩下所有面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public PanelInfo[] CloseExceptPanel(PanelType[] types)
        {
            List<PanelInfo> infoList = new List<PanelInfo>();
            List<PanelType> typeList = new List<PanelType>(types);
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                if (!typeList.Contains(item))
                {
                    infoList.Add(ClosePanel(item));
                }
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 隐藏全部面板
        /// </summary>
        public PanelInfo[] CloseAllPanels()
        {
            List<PanelInfo> infoList = new List<PanelInfo>();
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                infoList.Add(ClosePanel(item));
            }
            return infoList.ToArray();
        }
        /// <summary>
        /// 获取面板
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PanelInfo GetPanelInfo(PanelType type)
        {
            if (PanelExist(type))
            {
                PanelInfo info = panels[type] as PanelInfo;
                return info;
            }
            return null;
        }
        /// <summary>
        /// 面板是否打开
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsOpenPanel(PanelType type)
        {
            if (PanelExist(type))
            {
                PanelInfo info = GetPanelInfo(type);
                if (info.state == InfoState.Open)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}