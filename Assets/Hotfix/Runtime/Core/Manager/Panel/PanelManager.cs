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
        public PanelData CreatePanel(PanelType type, object data = null)
        {
            PanelData panelData = handler.CreatePanel(type, data);
            panels.Add(type, panelData);
            return panelData;
        }
        /// <summary>
        /// 删除面板
        /// </summary>
        /// <param name="type"></param>
        public void ClearPanel(PanelType type)
        {
            if (PanelExist(type))
            {
                PanelData panelData = GetPanelInfo(type);
                panelData.gameObject.SafeDestroy();
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
                PanelData panelData = (PanelData)enumerator.Value;
                if (panelData.state == PanelState.Open)
                {
                    ClearPanel(panelData.type);
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
        public PanelData OpenPanel(PanelType type, object data = null)
        {
            if (PanelExist(type))
            {
                PanelData panelData = GetPanelInfo(type);
                panelData.OpenPanel();
                return panelData;
            }
            else
            {
                PanelData panelData = CreatePanel(type, data);
                panelData.OpenPanel();
                return panelData;
            }
        }
        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public PanelData[] OpenPanel(PanelType[] types)
        {
            List<PanelData> panelDataList = new List<PanelData>();
            foreach (PanelType item in types)
            {
                panelDataList.Add(OpenPanel(item));
            }
            return panelDataList.ToArray();
        }
        /// <summary>
        /// 打开剩下所有面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public PanelData[] OpenExceptPanel(PanelType[] types)
        {
            List<PanelData> panelDataList = new List<PanelData>();
            List<PanelType> typeList = new List<PanelType>(types);
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                if (!typeList.Contains(item))
                {
                    panelDataList.Add(OpenPanel(item));
                }
            }
            return panelDataList.ToArray();
        }
        /// <summary>
        /// 打开全部面板
        /// </summary>
        public PanelData[] OpenAllPanels()
        {
            List<PanelData> panelDataList = new List<PanelData>();
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                panelDataList.Add(OpenPanel(item));
            }
            return panelDataList.ToArray();
        }
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PanelData ClosePanel(PanelType type, object data = null)
        {
            if (PanelExist(type))
            {
                PanelData panelData = GetPanelInfo(type);
                panelData.ClosePanel();
                return panelData;
            }
            else
            {
                PanelData panelData = CreatePanel(type, data);
                panelData.ClosePanel();
                return panelData;
            }
        }
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public PanelData[] ClosePanel(PanelType[] types)
        {
            List<PanelData> panelDataList = new List<PanelData>();
            foreach (PanelType item in types)
            {
                panelDataList.Add(ClosePanel(item));
            }
            return panelDataList.ToArray();
        }
        /// <summary>
        /// 隐藏剩下所有面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public PanelData[] CloseExceptPanel(PanelType[] types)
        {
            List<PanelData> panelDataList = new List<PanelData>();
            List<PanelType> typeList = new List<PanelType>(types);
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                if (!typeList.Contains(item))
                {
                    panelDataList.Add(ClosePanel(item));
                }
            }
            return panelDataList.ToArray();
        }
        /// <summary>
        /// 隐藏全部面板
        /// </summary>
        public PanelData[] CloseAllPanels()
        {
            List<PanelData> panelDataList = new List<PanelData>();
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                panelDataList.Add(ClosePanel(item));
            }
            return panelDataList.ToArray();
        }
        /// <summary>
        /// 获取面板
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public PanelData GetPanelInfo(PanelType type)
        {
            if (PanelExist(type))
            {
                PanelData panelData = panels[type] as PanelData;
                return panelData;
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
                PanelData panelData = panels[type] as PanelData;
                if (panelData.objectBase == null) return null;
                return (T)panelData.objectBase;
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
                PanelData panelData = GetPanelInfo(type);
                if (panelData.state == PanelState.Open)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}