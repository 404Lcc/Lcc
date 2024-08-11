using LccModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LccHotfix
{
    public class PanelManager : AObjectBase
    {
        public static PanelManager Instance { get; set; }

        public const int DepthMultiply = 100;

        public Dictionary<int, Panel> allPanelDict = new Dictionary<int, Panel>();//已加载的界面
        public Dictionary<int, Panel> shownPanelDict = new Dictionary<int, Panel>();//打开的界面

        public Stack<NavigationData> backSequence = new Stack<NavigationData>();//导航堆栈

        public Dictionary<int, string> typeToNameDict = new Dictionary<int, string>();//type 名字
        public Dictionary<string, int> nameToTypeDict = new Dictionary<string, int>();//名字 type

        public Dictionary<int, IPanelHandler> typeToLogicDict = new Dictionary<int, IPanelHandler>();//逻辑

        public Panel curNavigation;//当前导航
        public Panel lastNavigation;//上一个导航

        public List<PanelType> cachedList = new List<PanelType>();//换成列表

        //public PanelCompare compare = new PanelCompare();

        public override void Awake()
        {
            base.Awake();


            Instance = this;

            foreach (PanelType item in Enum.GetValues(typeof(PanelType)))
            {
                if (item == PanelType.None) continue;
                string name = item.ToPanelString();
                typeToNameDict.Add((int)item, name);
                nameToTypeDict.Add(name, (int)item);


                Type type = Manager.Instance.GetTypeByName(item.GetType().Namespace + "." + name);
                if (type != null)
                {
                    object ui = Activator.CreateInstance(type);
                    if (ui is IPanelHandler logic)
                    {
                        typeToLogicDict[(int)item] = logic;
                    }
                    else
                    {
                        LogHelper.Error($"{name} 未继承IPanelHandler");
                    }
                }
                else
                {
                    LogHelper.Error($"UI逻辑未找到 {name}");
                }
            }

        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;

            typeToNameDict.Clear();
            nameToTypeDict.Clear();
            typeToLogicDict.Clear();

            UnAllPanel();
        }


        /// <summary>
        /// 根据类型获取根节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 判断界面是否打开
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsPanelVisible(PanelType type)
        {
            return shownPanelDict.ContainsKey((int)type);
        }
        /// <summary>
        /// 根据类型获取界面对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Panel GetPanel(PanelType type)
        {
            if (allPanelDict.ContainsKey((int)type))
            {
                return allPanelDict[(int)type];
            }
            return null;
        }
        /// <summary>
        /// 根据泛型获取界面类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private PanelType GetPanelByGeneric<T>() where T : IPanelHandler
        {
            if (nameToTypeDict.TryGetValue(typeof(T).Name, out int type))
            {
                return (PanelType)type;
            }

            return PanelType.None;
        }
        /// <summary>
        /// 根据类型获取逻辑对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IPanelHandler GetPanelLogic(PanelType type)
        {
            if (typeToLogicDict.ContainsKey((int)type))
            {
                return typeToLogicDict[(int)type];
            }
            return null;
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="showData"></param>
        public void ShowPanel(PanelType type, ShowPanelData showData = null)
        {
            Panel panel = LoadPanel(type);
            if (panel != null)
            {
                InternalShowPanel(panel, type, showData);
            }
        }
        /// <summary>
        /// 打开界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="showData"></param>
        public void ShowPanel<T>(ShowPanelData showData = null) where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            ShowPanel(type, showData);
        }
        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="showData"></param>
        /// <returns></returns>
        public void ShowPanelAsync(PanelType type, ShowPanelData showData = null)
        {
            //Panel panel = LoadPanelAsync(type);
            //if (panel != null)
            //{
            //    InternalShowPanel(panel, type, showData);
            //}
        }
        /// <summary>
        /// 打开界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="showData"></param>
        /// <returns></returns>
        public void ShowPanelAsync<T>(ShowPanelData showData = null) where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            ShowPanelAsync(type, showData);
        }
        private void InternalShowPanel(Panel panel, PanelType type, ShowPanelData showData = null)
        {
            if (showData != null && showData.forceReset)
            {
                //强制重置界面
                panel.Logic.OnReset(panel);
            }
            //默认执行导航逻辑
            if (showData == null || (showData != null && showData.executeNavigationLogic))
            {
                //执行导航逻辑
                ExecuteNavigationLogic(panel, showData);
            }


            object contextData = showData == null ? null : showData.contextData;
            panel.Depth = GetCurrentPanelMaxDepth(panel.data.type);
            panel.IsShown = true;
            panel.SetRoot(GetRoot(panel.data.type));
            panel.Logic.OnShow(panel, contextData);
            shownPanelDict[(int)type] = panel;

            if (panel.data.navigationMode == UINavigationMode.NormalNavigation)
            {
                //刷新导航
                lastNavigation = curNavigation;
                curNavigation = panel;
            }
        }


        /// <summary>
        /// 加载界面
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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
            var loader = new GameObject("loader");
            var asset = AssetManager.Instance.LoadGameObject(loader, name);

            GameObject go = UnityEngine.Object.Instantiate(asset);
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            panel.Loader = loader;
            panel.GameObject = CreateUIGameObject(go);
            panel.Canvas = panel.GameObject.GetComponent<Canvas>();
            panel.GameObject.name = go.name;
            loader.transform.SetParent(panel.GameObject.transform);

            panel.Logic.OnInitComponent(panel);
            panel.Logic.OnInitData(panel);
            panel.Logic.OnRegisterUIEvent(panel);

            allPanelDict[(int)panel.Type] = panel;
        }
        /// <summary>
        /// 异步加载界面
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private void LoadPanelAsync(PanelType type)
        {
            //CoroutineLock coroutineLock = null;
            //try
            //{
            //    coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.LoadUI, (int)type);
            //    Panel panel = GetPanel(type);
            //    if (panel == null)
            //    {
            //        panel = AddChildren<Panel>();
            //        panel.Type = type;
            //        await InternalLoadPanelAsync(panel);
            //    }

            //    if (!panel.IsLoad)
            //    {
            //        await InternalLoadPanelAsync(panel);
            //    }
            //    return panel;
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
            //finally
            //{
            //    coroutineLock?.Dispose();
            //}
        }
        private void InternalLoadPanelAsync(Panel panel)
        {
            //if (!typeToNameDict.TryGetValue((int)panel.Type, out string name))
            //{
            //    return;
            //}
            //var loader = new GameObject("loader");
            //var asset = await AssetManager.Instance.StartLoadGameObject(loader, name);

            //GameObject go = UnityEngine.Object.Instantiate(asset);
            //go.name = name;
            //go.transform.localPosition = Vector3.zero;
            //go.transform.localRotation = Quaternion.identity;
            //go.transform.localScale = Vector3.one;

            //panel.Loader = loader;
            //panel.GameObject = CreateUIGameObject(go);
            //panel.Canvas = panel.GameObject.GetComponent<Canvas>();
            //panel.GameObject.name = go.name;
            //loader.transform.SetParent(panel.GameObject.transform);

            //panel.Logic.OnInitComponent(panel);
            //panel.Logic.OnInitData(panel);
            //panel.Logic.OnRegisterUIEvent(panel);

            //allPanelDict[(int)panel.Type] = panel;
        }


        private GameObject CreateUIGameObject(GameObject gameObject)
        {
            gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<GraphicRaycaster>();
            ScreenAdaptationHelper.UIPanelAdaptation(gameObject);
            return gameObject;
        }




        /// <summary>
        /// 隐藏界面
        /// </summary>
        /// <param name="type"></param>
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
            panel.SetRoot(GlobalManager.Instance.RemoveRoot);
            panel.Logic.OnHide(panel);
            shownPanelDict.Remove((int)type);
        }
        /// <summary>
        /// 根据泛型隐藏界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void HidePanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            HidePanel(type);
        }
        /// <summary>
        /// 隐藏全部打开的界面
        /// </summary>
        /// <param name="includeFixed">忽略固定界面</param>
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
                item.Value.SetRoot(GlobalManager.Instance.RemoveRoot);
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
        /// 弹出导航面板
        /// </summary>
        /// <returns></returns>
        public bool PopupNavigationPanel()
        {
            if (curNavigation != null)
            {
                bool needReturn = curNavigation.Logic.IsReturn(curNavigation);
                if (needReturn)
                {
                    return false;
                }
            }
            return InternalPopupNavigationPanel();
        }
        /// <summary>
        /// 弹出导航面板
        /// </summary>
        /// <returns></returns>
        private bool InternalPopupNavigationPanel()
        {
            //没用导航数据
            if (backSequence.Count == 0)
            {
                if (curNavigation == null) return false;
                if (curNavigation.Logic.IsReturn(curNavigation)) return true;


                PanelType prePanelType = curNavigation.PreType;
                if (prePanelType != PanelType.None)
                {
                    //隐藏当前导航界面
                    HidePanel(curNavigation.Type);


                    //打开上一个导航界面
                    ShowPanelData showData = new ShowPanelData();
                    //没有导航数据所以不走导航
                    showData.executeNavigationLogic = false;
                    ShowPanel(prePanelType, showData);
                }
                return false;
            }
            //拿到导航数据栈顶
            NavigationData backData = backSequence.Peek();
            if (backData != null)
            {
                //获取当前界面
                int cur = GetCurrentShownPanel();
                //栈顶界面和当前层级最高的界面不相同直接return
                if (cur != (int)backData.hideTarget.Type)
                {
                    return false;
                }

                if (backData.hideTarget.Logic.IsReturn(backData.hideTarget))
                {
                    return true;
                }

                PanelType hideType = backData.hideTarget.Type;
                //隐藏目标界面
                if (IsPanelVisible(hideType))
                {
                    HidePanel(hideType);
                }
                //执行回退逻辑
                ExectuteFallback(backData);
            }
            return true;
        }
        /// <summary>
        /// 执行回退逻辑
        /// </summary>
        /// <param name="data"></param>
        private void ExectuteFallback(NavigationData data)
        {
            if (backSequence.Count > 0)
            {
                backSequence.Pop();
            }
            if (data.backShowTargets == null) return;

            //回退的时候把上一层打开的界面都打开
            for (int i = 0; i < data.backShowTargets.Count; i++)
            {
                PanelType backType = data.backShowTargets[i];

                //打开回退需要打开的界面
                ShowPanelForNavigation(backType);
                if (i == data.backShowTargets.Count - 1)
                {
                    Panel panel = GetPanel(backType);
                    if (panel.data.navigationMode == UINavigationMode.NormalNavigation)
                    {
                        //刷新导航
                        lastNavigation = curNavigation;
                        curNavigation = panel;

                    }
                }
            }
        }
        /// <summary>
        /// 根据导航数据打开界面
        /// </summary>
        /// <param name="type"></param>
        private void ShowPanelForNavigation(PanelType type)
        {
            if (IsPanelVisible(type)) return;
            Panel panel = GetPanel(type);
            panel.Depth = GetCurrentPanelMaxDepth(panel.data.type);
            panel.IsShown = true;
            panel.SetRoot(GetRoot(panel.data.type));
            panel.Logic.OnShow(panel);
            shownPanelDict[(int)panel.Type] = panel;
        }
        /// <summary>
        /// 获取当前层级最高的界面，一般来说就是当前打开的界面
        /// </summary>
        /// <returns></returns>
        private int GetCurrentShownPanel()
        {
            List<Panel> list = shownPanelDict.Values.ToList();
            //先进行个冒泡排序
            int num = list.Count - 1;
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num - i; j++)
                {
                    if (list[j].Depth < list[j + 1].Depth)
                    {
                        Panel temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                    }
                }
            }
            return (int)list[0].Type;
        }

        /// <summary>
        /// 获取当前类型最高层级
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int GetCurrentPanelMaxDepth(UIType type)
        {
            int temp = 0;
            List<Panel> list = shownPanelDict.Values.ToList();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].data.type == type)
                {
                    temp++;
                }
            }
            return temp * DepthMultiply;
        }
        /// <summary>
        /// 执行导航逻辑
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="showData"></param>
        private void ExecuteNavigationLogic(Panel panel, ShowPanelData showData)
        {
            //如果界面是导航类型的就刷新导航数据
            if (panel.data.navigationMode == UINavigationMode.NormalNavigation)
            {
                RefreshBackSequenceData(panel, showData);
            }
            else if (panel.data.showMode == UIShowMode.HideOther)
            {
                //关闭其他打开的界面
                HideAllShownPanel();
            }


            if (panel.data.forceClearNavigation || (showData != null && showData.forceClearBackSequenceData))
            {
                //强制清理导航数据
                ClearBackSequence();
            }
            else
            {
                if (panel.data.navigationMode == UINavigationMode.NormalNavigation)
                {
                    if (showData != null && showData.checkNavigation)
                    {
                        //强制检测导航数据
                        CheckBackSequenceData(panel);
                    }
                }
            }
        }
        /// <summary>
        /// 刷新导航数据
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="showData"></param>
        private void RefreshBackSequenceData(Panel panel, ShowPanelData showData)
        {
            if (shownPanelDict.Count > 0)
            {
                List<PanelType> removedList = new List<PanelType>();
                List<Panel> tempList = new List<Panel>();

                NavigationData navigationData = new NavigationData();


                foreach (KeyValuePair<int, Panel> item in shownPanelDict)
                {
                    //忽略打开的固定界面
                    if (item.Value.data.type == UIType.Fixed) continue;
                    //如果当前界面是 打开当前界面关闭其他界面 的类型
                    if (panel.data.showMode == UIShowMode.HideOther)
                    {
                        //隐藏
                        removedList.Add((PanelType)item.Key);
                        item.Value.IsShown = false;
                        item.Value.SetRoot(GlobalManager.Instance.RemoveRoot);
                        item.Value.Logic.OnHide(item.Value);
                    }
                    //记录当前打开的所有界面
                    tempList.Add(item.Value);
                }

                if (removedList != null)
                {
                    for (int i = 0; i < removedList.Count; i++)
                    {
                        shownPanelDict.Remove((int)removedList[i]);
                    }
                }

                //增加导航数据
                if (panel.data.navigationMode == UINavigationMode.NormalNavigation && (showData == null || (!showData.ignoreAddNavigationData)))
                {
                    //先进行个冒泡排序
                    int num = tempList.Count - 1;
                    for (int i = 0; i < num; i++)
                    {
                        for (int j = 0; j < num - i; j++)
                        {
                            if (tempList[j].Depth > tempList[j + 1].Depth)
                            {
                                Panel temp = tempList[j];
                                tempList[j] = tempList[j + 1];
                                tempList[j + 1] = temp;
                            }
                        }
                    }

                    List<PanelType> hiddenPanelList = new List<PanelType>();
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        PanelType item = tempList[i].Type;
                        hiddenPanelList.Add(item);
                    }
                    navigationData.hideTarget = panel;//当前界面
                    navigationData.backShowTargets = hiddenPanelList;//当前打开的界面
                    backSequence.Push(navigationData);
                }
            }
        }
        /// <summary>
        /// 检测导航数据
        /// </summary>
        /// <param name="panel"></param>
        private void CheckBackSequenceData(Panel panel)
        {
            if (backSequence.Count > 0)
            {
                NavigationData backData = backSequence.Peek();
                if (backData.hideTarget != null)
                {
                    //导航数据不匹配直接中断导航
                    if (backData.hideTarget.Type != panel.Type)
                    {
                        ClearBackSequence();
                    }
                }

            }
        }
        /// <summary>
        /// 清除导航数据
        /// </summary>
        public void ClearBackSequence()
        {
            if (backSequence != null)
            {
                backSequence.Clear();
            }
        }




        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="type"></param>
        public void ClosePanel(PanelType type)
        {
            if (!IsPanelVisible(type))
            {
                return;
            }
            Panel panel = shownPanelDict[(int)type];
            if (backSequence.Count > 0)
            {
                NavigationData navigationData = backSequence.Peek();
                //如果栈顶是要关闭的界面 执行弹出面板
                if (navigationData != null && navigationData.hideTarget == panel)
                {
                    PopupNavigationPanel();
                    return;
                }
            }
            HidePanel(type);
        }
        /// <summary>
        /// 根据泛型关闭界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ClosePanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            ClosePanel(type);
        }


        /// <summary>
        /// 卸载界面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isDispose"></param>
        private void UnPanel(PanelType type, bool isDispose = true)
        {
            Panel panel = GetPanel(type);
            if (panel == null)
            {
                return;
            }
            panel.Logic.OnBeforeUnload(panel);
            if (panel.IsLoad)
            {
                UnityEngine.Object.Destroy(panel.GameObject);
                panel.GameObject = null;
            }
            //todo
            if (curNavigation == panel)
            {
                curNavigation = null;
            }
            if (isDispose)
            {
                allPanelDict.Remove((int)type);
                shownPanelDict.Remove((int)type);
                panel.Dispose();
            }
        }
        /// <summary>
        /// 根据泛型卸载界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void UnPanel<T>() where T : IPanelHandler
        {
            PanelType type = GetPanelByGeneric<T>();
            UnPanel(type);
        }
        /// <summary>
        /// 卸载全部界面
        /// </summary>
        private void UnAllPanel()
        {
            if (allPanelDict == null) return;
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

            curNavigation = null;
            lastNavigation = null;
        }



    }
}