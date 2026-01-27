using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    /*  ViewLoaderComponent: View资源加载组件
    不关联渲染对象，但渲染对象构建，依赖于其中存放的信息。服务器Entity也可以挂

    客户端：用AssetDetail提供的信息，来创建View组件（模型等引擎渲染对象）
    服务器：用它存放关于渲染对象的配置描述、状态细节，用于同步
    */
    public class ViewLoaderComponent : LogicComponent
    {
        public IViewLoader MainViewData;
        private Dictionary<int, bool> mAllViewDone = new Dictionary<int, bool>();
        private Dictionary<int, IReceiveLoaded>  mAllDoneViewDict = new Dictionary<int, IReceiveLoaded>();

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
            mAllViewDone.Clear();
            mAllDoneViewDict.Clear();
            RecycleViewLoader();
            MainViewData = null;
        }

        private void RecycleViewLoader()
        {
            ReferencePool.Release(MainViewData);
        }

        public void AddLoader(IViewLoader loaderData)
        {
            if (!TryAddAllDoneDict(loaderData))
            {
                return;
            }

            if (MainViewData == null)
            {
                MainViewData = loaderData;
            }
            else
            {
                UnityEngine.Debug.LogError($"重复添加了View， 如果需要更换View请使用ChangeLoader");
            }
        }

        public void ChangeLoader(IViewLoader loaderData)
        {
            if (mAllViewDone.TryGetValue(loaderData.Category, out var result))
            {
                mAllViewDone[loaderData.Category] = false;
            }

            if (mAllDoneViewDict.TryGetValue(loaderData.Category, out var receiveLoaded))
            {
                receiveLoaded.Dispose();
                mAllDoneViewDict.Remove(loaderData.Category);
            }

            _changeLoaderInternal(MainViewData, loaderData);
        }

        private void _changeLoaderInternal(IViewLoader curLoaderData, IViewLoader loaderData)
        {
            if (curLoaderData.Category == loaderData.Category)
            {
                curLoaderData = loaderData;
                return;
            }
            
            var subLoaderList = loaderData.SubLoaderList;

            if (subLoaderList != null)
            {
                for (int i = 0; i < subLoaderList.Count; i++)
                {
                    _changeLoaderInternal(subLoaderList[i], loaderData);
                }
            }
        }

        private bool TryAddAllDoneDict(IViewLoader loader)
        {
            var category = loader.Category;
            if (mAllViewDone.TryGetValue(category, out var isDone))
            {
                UnityEngine.Debug.LogError($"Add view loader failed! Reason : Already exist category = {category}, pls check.");
                return false;
            }
            else
            {
                mAllViewDone.Add(category, false);
            }

            var subViewList = loader.SubLoaderList;
            if (subViewList != null)
            {
                for (int i = 0; i < subViewList.Count; i++)
                {
                    var subLoader = subViewList[i];
                    TryAddAllDoneDict(subLoader);
                }
            }
            
            return true;
        }
        
        public void AddSubLoader(IViewLoader loaderData)
        {
            if (!TryAddAllDoneDict(loaderData))
            {
                return;
            }
            else
            {
                if (MainViewData.SubLoaderList == null)
                    MainViewData.SubLoaderList = new List<IViewLoader>();
                MainViewData.SubLoaderList.Add(loaderData);
            }
        }

        public void AddSubLoader(IViewLoader curViewData, int parentCategory, IViewLoader loaderData)
        {
            if (!TryAddAllDoneDict(loaderData))
            {
                return;
            }
            if (curViewData.Category == parentCategory)
            {
                curViewData.SubLoaderList.Add(loaderData);
            }
            else
            {
                for (int i = 0; i < curViewData.SubLoaderList.Count; i++)
                {
                    var subLoader = curViewData.SubLoaderList[i];
                    AddSubLoader(subLoader, parentCategory, loaderData);
                }
            }
        }

        /// <summary>
        /// 每加载完一个物体，接收回来
        /// </summary>
        public void ReceiveLoaded(int category, IReceiveLoaded loaded, LogicEntity owner, ECWorlds world)
        {
            if (!mAllDoneViewDict.TryGetValue(category, out var existLoaded))
            {
                mAllDoneViewDict.Add(category, loaded);
            }
            else
            {
                UnityEngine.Debug.LogError($"已经存在了相同的category资源，请检查：category is {category}"); 
            }

            if (mAllViewDone.TryGetValue(category, out var value))
            {
                mAllViewDone[category] = true;
            }
            
            bool allDone = true;
            var allDoneList = mAllViewDone.Values.ToList();
            for (int i = 0; i < allDoneList.Count; i++)
            {
                allDone = allDone && allDoneList[i];
            }

            if (allDone)
            {
                mAllViewDone.Clear();
                Deploy(world);
            }
        }
        
        /// <summary>
        /// 准备部署
        /// </summary>
        public void Deploy(ECWorlds world)
        {
            _deploy(MainViewData, null, world);
        }

        private void _deploy(IViewLoader loader, IViewWrapper parentView, ECWorlds world)
        {
            if (!mAllDoneViewDict.TryGetValue(loader.Category, out var loaded))
            {
                return;
            }
            
            var curView = DeployView(loader, world, loaded, parentView);
            if (loader.SubLoaderList != null)
            {
                for (int i = 0; i < loader.SubLoaderList.Count; i++)
                {
                    _deploy(loader.SubLoaderList[i], curView, world);
                }
            }
        }

        private IViewWrapper DeployView(IViewLoader loader, ECWorlds world, IReceiveLoaded loaded, IViewWrapper parentView)
        {
            if (loader.IsDeploy)
            {
                var existView = _owner.GetView<IViewWrapper>(loader.Category);
                return existView;
            }
            
            var view = System.Activator.CreateInstance(loader.CategoryType, loaded, loader.Category, world) as IViewWrapper;
            if (view != null)
            {
                view.Init(_owner.ID, loader, parentView);
                Owner.AddView(view);
                loader.IsDeploy = true;
                return view;
            }

            return null;
        }

        private static void TryPlayParticelSystem(Transform trans)
        {
            var childParticles = trans.GetComponentsInChildren<ParticleSystem>();
            foreach (var childParticle in childParticles)
            {
                if(!childParticle.isPlaying)
                    childParticle.Play();
            }
        }
    }

    public partial class LogicEntity
    {
        public ViewLoaderComponent ComViewLoader
        {
            get { return (ViewLoaderComponent)GetComponent(LogicComponentsLookup.ComViewLoader); }
        }

        public bool hasComViewLoader
        {
            get { return HasComponent(LogicComponentsLookup.ComViewLoader); }
        }

        public void AddComViewLoader(IViewLoader loaderData)
        {
            var index = LogicComponentsLookup.ComViewLoader;
            if (!hasComViewLoader)
            {
                var component = (ViewLoaderComponent)CreateComponent(index, typeof(ViewLoaderComponent));
                component.AddLoader(loaderData);
                AddComponent(index, component);
            }
            else
            {
                UnityEngine.Debug.LogError("已经有了ViewLoader，如果想添加新的，请用接口：AddSubViewLoader");
            }
        }
        
        public void AddSubLoader(IViewLoader loaderData)
        {
            var index = LogicComponentsLookup.ComViewLoader;
            if (!hasComViewLoader)
            {
                var component = (ViewLoaderComponent)CreateComponent(index, typeof(ViewLoaderComponent));
                component.AddLoader(loaderData);
                AddComponent(index, component);
            }
            else
            {
                var component = (ViewLoaderComponent)GetComponent(index);
                component.AddSubLoader(loaderData);
                ReplaceComponent(index, component);
            }
        }
        
        public void AddSubLoader(int parentCategory, IViewLoader loaderData)
        {
            var index = LogicComponentsLookup.ComViewLoader;
            if (!hasComViewLoader)
            {
                var component = (ViewLoaderComponent)CreateComponent(index, typeof(ViewLoaderComponent));
                component.AddLoader(loaderData);
                AddComponent(index, component);
            }
            else
            {
                var component = (ViewLoaderComponent)GetComponent(index);
                var viewLoader = component.MainViewData;
                component.AddSubLoader(viewLoader, parentCategory, loaderData);
                ReplaceComponent(index, component);
            }
        }

        public void RemoveComViewLoader()
        {
            if (!hasComViewLoader)
            {
                RemoveComponent(LogicComponentsLookup.ComViewLoader);
            }
        }

        public void ChangeViewLoad(IViewLoader loaderData)
        {
            var index = LogicComponentsLookup.ComViewLoader;
            if (!hasComViewLoader)
            {
                var component = (ViewLoaderComponent)CreateComponent(index, typeof(ViewLoaderComponent));
                component.AddLoader(loaderData);
                AddComponent(index, component);
            }
            else
            {
                var component = (ViewLoaderComponent)GetComponent(index);
                var viewLoader = component.MainViewData;
                bool finded = TryFindLoader(viewLoader, loaderData);

                if (!finded)
                {
                    component.ChangeLoader(loaderData);
                }

                ReplaceComponent(index, component);
            }
        }

        private bool TryFindLoader(IViewLoader curData, IViewLoader loaderData)
        {
            if (curData.Category == loaderData.Category)
            {
                curData = loaderData;
                return true;
            }
            else
            {
                for (int i = 0; i < curData.SubLoaderList.Count; i++)
                {
                    var subLoader = curData.SubLoaderList[i];
                    if (TryFindLoader(subLoader, loaderData))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComNewViewLoaderIndex = new(typeof(ViewLoaderComponent));
        public static int ComViewLoader => _ComNewViewLoaderIndex.Index;
    }
}