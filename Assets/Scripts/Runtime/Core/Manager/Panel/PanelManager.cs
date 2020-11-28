using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LccModel
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
        public bool PanelExist(PanelType type)
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
        public async Task<Panel> CreatePanelAsync(PanelType type, object data = null)
        {
            Panel panel = await handler.CreatePanelAsync(type, data);
            panels.Add(type, panel);
            return panel;
        }
        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Panel> OpenPanelAsync(PanelType type, object data = null)
        {
            if (PanelExist(type))
            {
                Panel panel = GetPanel(type);
                panel.OpenPanel();
                return panel;
            }
            else
            {
                Panel panel = await CreatePanelAsync(type, data);
                panel.OpenPanel();
                return panel;
            }
        }
        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public async Task<Panel[]> OpenPanelsAsync(PanelType[] types)
        {
            List<Panel> panelList = new List<Panel>();
            foreach (PanelType item in types)
            {
                panelList.Add(await OpenPanelAsync(item));
            }
            return panelList.ToArray();
        }
        /// <summary>
        /// 打开剩下所有面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public async Task<Panel[]> OpenExceptPanelsAsync(PanelType[] types)
        {
            List<Panel> panelList = new List<Panel>();
            List<PanelType> typeList = new List<PanelType>(types);
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                if (!typeList.Contains(item))
                {
                    panelList.Add(await OpenPanelAsync(item));
                }
            }
            return panelList.ToArray();
        }
        /// <summary>
        /// 打开全部面板
        /// </summary>
        public async Task<Panel[]> OpenAllPanelsAsync()
        {
            List<Panel> panelList = new List<Panel>();
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                panelList.Add(await OpenPanelAsync(item));
            }
            return panelList.ToArray();
        }
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Panel> ClosePanelAsync(PanelType type, object data = null)
        {
            if (PanelExist(type))
            {
                Panel panel = GetPanel(type);
                panel.ClosePanel();
                return panel;
            }
            else
            {
                Panel panel = await CreatePanelAsync(type, data);
                panel.ClosePanel();
                return panel;
            }
        }
        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public async Task<Panel[]> ClosePanelsAsync(PanelType[] types)
        {
            List<Panel> panelList = new List<Panel>();
            foreach (PanelType item in types)
            {
                panelList.Add(await ClosePanelAsync(item));
            }
            return panelList.ToArray();
        }
        /// <summary>
        /// 隐藏剩下所有面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public async Task<Panel[]> CloseExceptPanelsAsync(PanelType[] types)
        {
            List<Panel> panelList = new List<Panel>();
            List<PanelType> typeList = new List<PanelType>(types);
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                if (!typeList.Contains(item))
                {
                    panelList.Add(await ClosePanelAsync(item));
                }
            }
            return panelList.ToArray();
        }
        /// <summary>
        /// 隐藏全部面板
        /// </summary>
        public async Task<Panel[]> CloseAllPanelsAsync()
        {
            List<Panel> panelList = new List<Panel>();
            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                panelList.Add(await ClosePanelAsync(item));
            }
            return panelList.ToArray();
        }
        /// <summary>
        /// 删除面板
        /// </summary>
        /// <param name="type"></param>
        public void ClearPanel(PanelType type)
        {
            if (PanelExist(type))
            {
                Panel panel = GetPanel(type);
                panel.AObjectBase.gameObject.SafeDestroy();
                panels.Remove(type);
            }
        }
        /// <summary>
        /// 删除面板
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public void ClearPanels(PanelType[] types)
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
        public void ClearExceptPanels(PanelType[] types)
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
                Panel panel = (Panel)enumerator.Value;
                if (panel.State == PanelState.Open)
                {
                    ClearPanel(panel.Type);
                    number++;
                }
            }
            return number;
        }
        /// <summary>
        /// 获取面板
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Panel GetPanel(PanelType type)
        {
            if (PanelExist(type))
            {
                Panel panel = (Panel)panels[type];
                return panel;
            }
            return null;
        }
        /// <summary>
        /// 获取面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public T GetPanel<T>(PanelType type) where T : AObjectBase
        {
            if (PanelExist(type))
            {
                Panel panel = (Panel)panels[type];
                return (T)panel.AObjectBase;
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
                Panel panel = GetPanel(type);
                if (panel.State == PanelState.Open)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}