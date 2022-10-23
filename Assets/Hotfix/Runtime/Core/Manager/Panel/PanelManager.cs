using BM;
using ET;
using LccModel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class PanelManager : Singleton<PanelManager>
    {
        private readonly string _suff = AssetSuffix.Prefab;
        private readonly string[] _types = new string[] { AssetType.Prefab, AssetType.Panel };



        public bool isPopStackWndStatus;


        public Dictionary<int, Panel> allPanelDict = new Dictionary<int, Panel>();
        public Dictionary<int, Panel> panelVisibleDict = new Dictionary<int, Panel>();
    



        public Queue<PanelType> panelTypeQueue = new Queue<PanelType>();

        public List<PanelType> panelTypeCachedList = new List<PanelType>();






        public Dictionary<int, string> typeToNameDict = new Dictionary<int, string>();//type 名字
        public Dictionary<string, int> nameToTypeDict = new Dictionary<string, int>();//名字 type


        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            isPopStackWndStatus = false;


            allPanelDict.Clear();
            panelVisibleDict.Clear();
            panelTypeQueue.Clear();
            panelTypeCachedList.Clear();
            typeToNameDict.Clear();
            nameToTypeDict.Clear();

            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                string name = item.ToPanelString();
                typeToNameDict.Add((int)item, name);
                nameToTypeDict.Add(name, (int)item);
            }
        }





        public bool IsPanelVisible(PanelType type)
        {
            return panelVisibleDict.ContainsKey((int)type);
        }



        public PanelType GetPanelByGeneric<T>() where T : IPanelHandler
        {
            if (nameToTypeDict.TryGetValue(typeof(T).Name, out int type))
            {
                return (PanelType)type;
            }

            return PanelType.None;
        }
        private Panel GetPanel(PanelType type)
        {
            if (allPanelDict.ContainsKey((int)type))
            {
                return allPanelDict[(int)type];
            }
            return null;
        }
        public T GetPanel<T>(bool isNeedShowState = false) where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            Panel panel = GetPanel(type);
            if (panel == null)
            {
                return default;
            }
            if (!panel.IsLoad)
            {
                return default;
            }
            if (isNeedShowState)
            {
                if (!IsPanelVisible(type))
                {
                    return default;
                }
            }
            return (T)panel.logic;
        }


 












        public void ShowStackPanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            ShowStackPanel(type);
        }




        public void ShowStackPanel(PanelType type)
        {
            panelTypeQueue.Enqueue(type);

            if (isPopStackWndStatus)
            {
                return;
            }
            isPopStackWndStatus = true;
            PopStackPanel();
        }
        private void PopNextStackPanel(PanelType type)
        {
            Panel panel = GetPanel(type);
            if (panel != null && !panel.IsDisposed && isPopStackWndStatus && panel.IsInStackQueue)
            {
                panel.IsInStackQueue = false;
                PopStackPanel();
            }
        }
        private void PopStackPanel()
        {
            if (panelTypeQueue.Count > 0)
            {
                PanelType type = panelTypeQueue.Dequeue();
                ShowPanel(type);
                Panel panel = GetPanel(type);
                panel.IsInStackQueue = true;
            }
            else
            {
                isPopStackWndStatus = false;
            }
        }









        public void ShowPanel(PanelType type, ShowPanelData data = null)
        {
            Panel panel = LoadPanel(type);
            if (panel != null)
            {
                InternalShowPanel(panel, type, data);
            }
        }
        public void ShowPanel<T>(ShowPanelData data = null) where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            ShowPanel(type, data);
        }
        public async ETTask ShowPanelAsync(PanelType type, ShowPanelData data = null)
        {
            Panel panel = await LoadPanelAsync(type);
            if (panel != null)
            {
                InternalShowPanel(panel, type, data);
            }
        }
        public async ETTask ShowPanelAsync<T>(ShowPanelData data = null) where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            await ShowPanelAsync(type, data);
        }
        private void InternalShowPanel(Panel panel, PanelType type, ShowPanelData data = null)
        {
            AObjectBase contextData = data == null ? null : data.contextData;
            panel.GameObject.SetActive(true);
            panel.logic.OnShow(panel, contextData);

            panelVisibleDict[(int)type] = panel;
        }













        private Panel LoadPanel(PanelType type)
        {
            Panel panel = GetPanel(type);
            if (panel == null)
            {
                panel = AddChildren<Panel>();
                panel.Type = type;
                InternalLoadPanel(panel);
            }

            if (!panel.IsLoad)
            {
                InternalLoadPanel(panel);
            }
            return panel;
        }
        private void InternalLoadPanel(Panel panel)
        {
            if (!typeToNameDict.TryGetValue((int)panel.Type, out string name))
            {
                return;
            }
            GameObject go = AssetManager.Instance.LoadAsset<GameObject>(name, _suff, _types);
            panel.GameObject = UnityEngine.Object.Instantiate(go);
            panel.GameObject.name = go.name;

            panel.logic.OnInitData(panel);

            panel.SetRoot(GetRoot(panel.data.type));
            panel.Transform.SetAsLastSibling();

            panel.logic.OnInitComponent(panel);
            panel.logic.OnRegisterUIEvent(panel);

            allPanelDict[(int)panel.Type] = panel;
        }
        private async ETTask<Panel> LoadPanelAsync(PanelType type)
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.LoadUI, (int)type);
                Panel panel = GetPanel(type);
                if (panel == null)
                {
                    if (typeToNameDict.ContainsKey((int)type))
                    {
                        panel = AddChildren<Panel>();
                        panel.Type = type;
                        await InternalLoadPanelAsync(panel);
                    }
                }

                if (!panel.IsLoad)
                {
                    await InternalLoadPanelAsync(panel);
                }
                return panel;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        private async ETTask InternalLoadPanelAsync(Panel panel)
        {
            if (!typeToNameDict.TryGetValue((int)panel.Type, out string name))
            {
                return;
            }
            LoadHandler handler = await AssetManager.Instance.LoadAssetAsync<GameObject>(name, _suff, _types);
            GameObject go = (GameObject)handler.Asset;
            panel.GameObject = UnityEngine.Object.Instantiate(go);
            panel.GameObject.name = go.name;

            panel.logic.OnInitData(panel);

            panel?.SetRoot(GetRoot(panel.data.type));
            panel.Transform.SetAsLastSibling();

            panel.logic.OnInitComponent(panel);
            panel.logic.OnRegisterUIEvent(panel);

            allPanelDict[(int)panel.Type] = panel;
        }








        public void HidePanel(PanelType type)
        {
            if (!panelVisibleDict.ContainsKey((int)type))
            {
                return;
            }

            Panel panel = panelVisibleDict[(int)type];
            if (panel == null || panel.IsDisposed)
            {
                return;
            }

            panel.GameObject.SetActive(false);
            panel.logic.OnHide(panel);

            panelVisibleDict.Remove((int)type);

            PopNextStackPanel(type);
        }
        public void HidePanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            HidePanel(type);
        }
        public void HideAllShownPanel(bool includeFixed = false)
        {
            isPopStackWndStatus = false;
            panelTypeCachedList.Clear();
            foreach (KeyValuePair<int, Panel> item in panelVisibleDict)
            {
                if (item.Value.data.type == UIType.Fixed && !includeFixed)
                {
                    continue;
                }
                if (item.Value.IsDisposed)
                {
                    continue;
                }

                panelTypeCachedList.Add((PanelType)item.Key);
                item.Value.GameObject.SetActive(false);
                item.Value.logic.OnHide(item.Value);
            }
            if (panelTypeCachedList.Count > 0)
            {
                for (int i = 0; i < panelTypeCachedList.Count; i++)
                {
                    panelVisibleDict.Remove((int)panelTypeCachedList[i]);
                }
            }
            panelTypeQueue.Clear();
        }











        public void ClosePanel(PanelType type)
        {
            if (!panelVisibleDict.ContainsKey((int)type))
            {
                return;
            }
            HidePanel(type);
            UnPanel(type);

        }
        public void ClosePanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            ClosePanel(type);
        }
        public void CloseAllPanel()
        {
            isPopStackWndStatus = false;
            if (allPanelDict == null)
            {
                return;
            }
            foreach (KeyValuePair<int, Panel> item in allPanelDict)
            {
                Panel panel = item.Value;
                if (panel == null || panel.IsDisposed)
                {
                    continue;
                }
                HidePanel(panel.Type);
                UnPanel(panel.Type, false);
                panel?.Dispose();
            }
            allPanelDict.Clear();
            panelVisibleDict.Clear();
            panelTypeQueue.Clear();
            panelTypeCachedList.Clear();
        }






        public void UnPanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            UnPanel(type);
        }


        public void UnPanel(PanelType type, bool isDispose = true)
        {
            Panel panel = GetPanel(type);
            if (panel == null)
            {
                return;
            }
            panel.logic.BeforeUnload(panel);
            if (panel.IsLoad)
            {
                AssetManager.Instance.UnLoadAsset(panel.GameObject.name, _suff, _types);

                UnityEngine.Object.Destroy(panel.GameObject);
                panel.GameObject = null;
            }
            if (isDispose)
            {
                allPanelDict.Remove((int)type);
                panelVisibleDict.Remove((int)type);
                panel.Dispose();
            }
        }







        private Transform GetRoot(UIType type)
        {
            if (type == UIType.Normal)
            {
                return GlobalManager.Instance.NormalRoot;
            }
            else if (type == UIType.Fixed)
            {
                return GlobalManager.Instance.FixedRoot;
            }
            else if (type == UIType.Popup)
            {
                return GlobalManager.Instance.PopupRoot;
            }
            else if (type == UIType.Other)
            {
                return GlobalManager.Instance.OtherRoot;
            }


            return null;
        }







        public override void OnDestroy()
        {
            base.OnDestroy();

            typeToNameDict.Clear();
            nameToTypeDict.Clear();

            CloseAllPanel();
        }
    }
}