using BM;
using ET;
using LccModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public class PanelManager : Singleton<PanelManager>
    {
        private readonly string _suff = AssetSuffix.Prefab;
        private readonly string[] _types = new string[] { AssetType.Prefab, AssetType.Panel };

        public Dictionary<int, Panel> allPanelDict = new Dictionary<int, Panel>();
        public Dictionary<int, Panel> shownPanelDict = new Dictionary<int, Panel>();

        public Stack<NavigationData> backSequence = new Stack<NavigationData>();

        public Dictionary<int, string> typeToNameDict = new Dictionary<int, string>();//type 名字
        public Dictionary<string, int> nameToTypeDict = new Dictionary<string, int>();//名字 type

        public Dictionary<int, IPanelHandler> typeToLogicDict = new Dictionary<int, IPanelHandler>();

        public Panel curNavigation = null;
        public Panel lastNavigation = null;

        public List<PanelType> cachedList = new List<PanelType>();

        public PanelCompare compare = new PanelCompare();


        public override void InitData(object[] datas)
        {
            base.InitData(datas);


            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                string name = item.ToPanelString();
                typeToNameDict.Add((int)item, name);
                nameToTypeDict.Add(name, (int)item);


                Type type = Manager.Instance.GetType(name);
                if (type != null)
                {
                    object ui = Activator.CreateInstance(type);
                    if (ui is IPanelHandler logic)
                    {
                        typeToLogicDict[(int)item] = logic;
                    }
                    else
                    {
                        LogUtil.LogError($"{name} 未继承IPanelHandler");
                    }
                }
                else
                {
                    LogUtil.LogError($"UI逻辑未找到 {name}");
                }
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



            return null;
        }
        public bool IsPanelVisible(PanelType type)
        {
            return shownPanelDict.ContainsKey((int)type);
        }
        private PanelType GetPanelByGeneric<T>() where T : IPanelHandler
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
            return (T)panel.Logic;
        }
        public IPanelHandler GetPanelLogic(PanelType type)
        {
            if (typeToLogicDict.ContainsKey((int)type))
            {
                return typeToLogicDict[(int)type];
            }
            return null;
        }

        public void ShowPanel(PanelType type, ShowPanelData showData = null)
        {
            Panel panel = LoadPanel(type);
            if (panel != null)
            {
                InternalShowPanel(panel, type, showData);
            }
        }
        public void ShowPanel<T>(ShowPanelData showData = null) where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            ShowPanel(type, showData);
        }
        public async ETTask ShowPanelAsync(PanelType type, ShowPanelData showData = null)
        {
            Panel panel = await LoadPanelAsync(type);
            if (panel != null)
            {
                InternalShowPanel(panel, type, showData);
            }
        }
        public async ETTask ShowPanelAsync<T>(ShowPanelData showData = null) where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            await ShowPanelAsync(type, showData);
        }
        private void InternalShowPanel(Panel panel, PanelType type, ShowPanelData showData = null)
        {
            if (showData != null && showData.forceReset)
            {
                panel.Logic.OnReset(panel);
            }

            if (showData == null || (showData != null && showData.executeNavigationLogic))
            {
                ExecuteNavigationLogic(panel, showData);
            }


            AObjectBase contextData = showData == null ? null : showData.contextData;
            panel.IsShown = true;
            panel.Logic.OnShow(panel, contextData);
            shownPanelDict[(int)type] = panel;

            if (panel.data.navigationMode == UINavigationMode.NormalNavigation)
            {
                lastNavigation = curNavigation;
                curNavigation = panel;
            }
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

            panel.Logic.OnInitData(panel);

            panel.SetRoot(GetRoot(panel.data.type));
            panel.Transform.SetAsLastSibling();

            panel.Logic.OnInitComponent(panel);
            panel.Logic.OnRegisterUIEvent(panel);

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
                    panel = AddChildren<Panel>();
                    panel.Type = type;
                    await InternalLoadPanelAsync(panel);
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

            panel.Logic.OnInitData(panel);

            panel.SetRoot(GetRoot(panel.data.type));
            panel.Transform.SetAsLastSibling();

            panel.Logic.OnInitComponent(panel);
            panel.Logic.OnRegisterUIEvent(panel);

            allPanelDict[(int)panel.Type] = panel;
        }


        public void HidePanel(PanelType type)
        {
            if (!IsPanelVisible(type))
            {
                return;
            }
            Panel panel = shownPanelDict[(int)type];
            if (panel == null || panel.IsDisposed)
            {
                return;
            }
            panel.IsShown = false;
            panel.Logic.OnHide(panel);
            shownPanelDict.Remove((int)type);
        }
        public void HidePanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            HidePanel(type);
        }
        public void HideAllShownPanel(bool includeFixed = false)
        {
            cachedList.Clear();
            foreach (KeyValuePair<int, Panel> item in shownPanelDict)
            {
                if (item.Value.data.type == UIType.Fixed && !includeFixed)
                {
                    continue;
                }
                if (item.Value.IsDisposed)
                {
                    continue;
                }

                cachedList.Add((PanelType)item.Key);
                item.Value.IsShown = false;
                item.Value.Logic.OnHide(item.Value);
            }
            if (cachedList.Count > 0)
            {
                for (int i = 0; i < cachedList.Count; i++)
                {
                    shownPanelDict.Remove((int)cachedList[i]);
                }
            }

        }



        /// <summary>
        ///当返回导航时检查当前窗口的返回逻辑
        ///如果为true，则执行返回逻辑
        ///如果为false，立即输入RealReturnWindow()逻辑
        /// </summary>
        /// <returns></returns>
        public bool PopupNavigationPanel()
        {
            if (curNavigation != null)
            {
                bool needReturn = curNavigation.Logic.IsReturn(curNavigation);
                if (needReturn) return false;
            }
            return RealPopupNavigationPanel();
        }
        private bool RealPopupNavigationPanel()
        {
            if (backSequence.Count == 0)
            {
                if (curNavigation == null) return false;
                if (curNavigation.Logic.IsReturn(curNavigation)) return true;


                PanelType prePanelType = curNavigation.PreType;
                if (prePanelType != PanelType.None)
                {
                    HidePanel(curNavigation.Type);
                    ShowPanelData showData = new ShowPanelData();
                    showData.executeNavigationLogic = false;
                    ShowPanel(prePanelType, showData);
                }
                return false;
            }
            NavigationData backData = backSequence.Peek();
            if (backData != null)
            {
                int cur = GetCurrentShownPanel();
                if (cur != (int)backData.hideTarget.Type)
                {

                    return false;
                }

                if (backData.hideTarget.Logic.IsReturn(backData.hideTarget))
                {
                    return true;
                }

                PanelType hideType = backData.hideTarget.Type;
                if (!IsPanelVisible(hideType))
                {
                    ExectuteBackSeqData(backData);
                }
                else
                {
                    HidePanel(hideType);
                    ExectuteBackSeqData(backData);
                }
            }
            return true;
        }
        private void ExectuteBackSeqData(NavigationData nd)
        {
            if (backSequence.Count > 0)
            {
                backSequence.Pop();
            }
            if (nd.backShowTargets == null)
            {
                return;
            }

            for (int i = 0; i < nd.backShowTargets.Count; i++)
            {
                PanelType backType = nd.backShowTargets[i];
                ShowPanelForNavigation(backType);
                if (i == nd.backShowTargets.Count - 1)
                {
                    Panel panel = GetPanel(backType);
                    if (panel.data.navigationMode == UINavigationMode.NormalNavigation)
                    {
                        lastNavigation = curNavigation;
                        curNavigation = panel;

                    }
                }
            }
        }
        private void ShowPanelForNavigation(PanelType type)
        {

            if (IsPanelVisible(type))
                return;

            var panel = GetPanel(type);
            panel.IsShown = true;
            panel.Logic.OnShow(panel);
            shownPanelDict[(int)panel.Type] = panel;




        }
        public void ClearBackSequence()
        {
            if (backSequence != null)
            {
                backSequence.Clear();
            }
        }
        private int GetCurrentShownPanel()
        {
            List<Panel> listWnds = shownPanelDict.Values.ToList();
            listWnds.Sort(compare);
            for (int i = listWnds.Count - 1; i >= 0; i--)
            {
                if (listWnds[i].data.type != UIType.Fixed)
                {
                    return (int)listWnds[i].Type;
                }
            }
            return (int)PanelType.None;
        }
        private void ExecuteNavigationLogic(Panel panel, ShowPanelData showData)
        {
            PanelData data = panel.data;
            if (panel.RefreshBackSeqData)
            {
                RefreshBackSequenceData(panel, showData);
            }
            else if (data.showMode == UIShowMode.HideOther)
            {
                HideAllShownPanel();
            }


            if (panel.data.forceClearNavigation || (showData != null && showData.forceClearBackSequenceData))
            {
                ClearBackSequence();
            }
            else
            {
                if (showData != null && showData.checkNavigation)
                {
                    CheckBackSequenceData(panel);
                }
            }
        }
        private void RefreshBackSequenceData(Panel panel, ShowPanelData showData)
        {
            PanelData data = panel.data;
            bool dealBackSequence = true;
            if (shownPanelDict.Count > 0 && dealBackSequence)
            {
                List<PanelType> removedKey = new List<PanelType>();
                List<Panel> sortedHiddenPanels = new List<Panel>();

                NavigationData backData = new NavigationData();
                foreach (KeyValuePair<int, Panel> item in shownPanelDict)
                {
                    if (data.showMode != UIShowMode.Normal)
                    {
                        if (item.Value.data.type == UIType.Fixed) continue;
                        removedKey.Add((PanelType)item.Key);
                        item.Value.IsShown = false;

                    }

                    if (item.Value.data.type != UIType.Fixed)
                    {
                        sortedHiddenPanels.Add(item.Value);
                    }
                }

                if (removedKey != null)
                {
                    for (int i = 0; i < removedKey.Count; i++)
                    {
                        shownPanelDict.Remove((int)removedKey[i]);
                    }
                }

                if (data.navigationMode == UINavigationMode.NormalNavigation && (showData == null || (!showData.ignoreAddNavigationData)))
                {
                    sortedHiddenPanels.Sort(this.compare);
                    List<PanelType> navHiddenPanels = new List<PanelType>();
                    for (int i = 0; i < sortedHiddenPanels.Count; i++)
                    {
                        PanelType pushPanelType = sortedHiddenPanels[i].Type;
                        navHiddenPanels.Add(pushPanelType);
                    }
                    backData.hideTarget = panel;
                    backData.backShowTargets = navHiddenPanels;
                    backSequence.Push(backData);
                }
            }
        }
        private void CheckBackSequenceData(Panel panel)
        {
            if (panel.RefreshBackSeqData)
            {
                if (backSequence.Count > 0)
                {
                    NavigationData backData = backSequence.Peek();
                    if (backData.hideTarget != null)
                    {
                        if (backData.hideTarget.Type != panel.Type)
                        {
                            ClearBackSequence();
                        }
                    }

                }
            }
        }


        public void ClosePanel(PanelType type)
        {
            if (!IsPanelVisible(type))
            {
                return;
            }
            Panel panel = shownPanelDict[(int)type];
            if (backSequence.Count > 0)
            {
                NavigationData seqData = backSequence.Peek();
                if (seqData != null && seqData.hideTarget == panel)
                {
                    PopupNavigationPanel();
                    return;
                }
            }
            HidePanel(type);
        }
        public void ClosePanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            ClosePanel(type);
        }
        public void ClearAllPanel()
        {
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

                UnPanel(panel.Type, false);
                panel.Dispose();
            }
            allPanelDict.Clear();
            shownPanelDict.Clear();
            backSequence.Clear();

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
            panel.Logic.OnBeforeUnload(panel);
            if (panel.IsLoad)
            {
                AssetManager.Instance.UnLoadAsset(panel.GameObject.name, _suff, _types);

                UnityEngine.Object.Destroy(panel.GameObject);
                panel.GameObject = null;
            }
            if (isDispose)
            {
                allPanelDict.Remove((int)type);
                shownPanelDict.Remove((int)type);
                panel.Dispose();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            typeToNameDict.Clear();
            nameToTypeDict.Clear();

            ClearAllPanel();
        }
    }
}