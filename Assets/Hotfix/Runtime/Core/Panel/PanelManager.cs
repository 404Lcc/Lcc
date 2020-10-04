using System;
using System.Collections;
using System.Collections.Generic;

namespace Hotfix
{
    public class PanelManager : Singleton<PanelManager>
    {
        public Hashtable panels = new Hashtable();
        public PanelObjectBaseHandler handler;
        public void InitManager(PanelObjectBaseHandler handler)
        {
            this.handler = handler;
        }
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
        /// <param name="data"></param>
        /// <returns></returns>
        public Panel CreatePanel(PanelType type, object data = null)
        {
            Panel info = handler.CreatePanel(type, data);
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
                Panel info = GetPanelInfo(type);
                Util.SafeDestroy(info.container);
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
                Panel info = enumerator.Value as Panel;
                if (info.state == PanelState.Open)
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
        /// <param name="data"></param>
        /// <returns></returns>
        public Panel OpenPanel(PanelType type, object data = null)
        {
            if (PanelExist(type))
            {
                Panel info = GetPanelInfo(type);
                info.OpenPanel();
                return info;
            }
            else
            {
                Panel info = CreatePanel(type, data);
                info.OpenPanel();
                return info;
            }
        }
        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public Panel[] OpenPanel(PanelType[] types)
        {
            List<Panel> infoList = new List<Panel>();
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
        public Panel[] OpenExceptPanel(PanelType[] types)
        {
            List<Panel> infoList = new List<Panel>();
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
        public Panel[] OpenAllPanels()
        {
            List<Panel> infoList = new List<Panel>();
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
        public Panel ClosePanel(PanelType type, object data = null)
        {
            if (PanelExist(type))
            {
                Panel info = GetPanelInfo(type);
                info.ClosePanel();
                return info;
            }
            else
            {
                Panel info = CreatePanel(type, data);
                info.ClosePanel();
                return info;
            }
        }
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public Panel[] ClosePanel(PanelType[] types)
        {
            List<Panel> infoList = new List<Panel>();
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
        public Panel[] CloseExceptPanel(PanelType[] types)
        {
            List<Panel> infoList = new List<Panel>();
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
        public Panel[] CloseAllPanels()
        {
            List<Panel> infoList = new List<Panel>();
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
        public Panel GetPanelInfo(PanelType type)
        {
            if (PanelExist(type))
            {
                Panel info = panels[type] as Panel;
                return info;
            }
            return null;
        }
        /// <summary>
        /// 获取面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public T GetPanelInfo<T>(PanelType type) where T : ObjectBase
        {
            if (PanelExist(type))
            {
                Panel info = panels[type] as Panel;
                if (info.objectBase == null) return null;
                return (T)info.objectBase;
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
                Panel info = GetPanelInfo(type);
                if (info.state == PanelState.Open)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}