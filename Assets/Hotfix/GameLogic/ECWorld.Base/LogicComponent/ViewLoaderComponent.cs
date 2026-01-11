using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public struct ViewData
    {
        public int Category;
        public string ObjName;
        public string BindPointName;
        public bool ClearFx;
    }

    //////////////////////////////////////////////////////////////////////////
    /*  ViewLoaderComponent: View资源加载组件
    不关联渲染对象，但渲染对象构建，依赖于其中存放的信息。服务器Entity也可以挂

    客户端：用AssetDetail提供的信息，来创建View组件（模型等引擎渲染对象）
    服务器：用它存放关于渲染对象的配置描述、状态细节，用于同步
    */
    //////////////////////////////////////////////////////////////////////////
    public class ViewLoaderComponent : LogicComponent
    {
        public List<ViewData> ViewDataList { get; protected set; } = new();

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
            ViewDataList.Clear();
        }
        
        public void AddData(ViewData data)
        {
            ViewDataList.Add(data);
        }
        
    }


    public partial class LogicEntity
    {
        public ViewLoaderComponent comViewLoader
        {
            get { return (ViewLoaderComponent)GetComponent(LogicComponentsLookup.ComViewLoader); }
        }

        public bool hasComViewLoader
        {
            get { return HasComponent(LogicComponentsLookup.ComViewLoader); }
        }

        public void AddComViewLoader(ViewData data)
        {
            var index = LogicComponentsLookup.ComViewLoader;
            if (!hasComViewLoader)
            {
                var component = (ViewLoaderComponent)CreateComponent(index, typeof(ViewLoaderComponent));
                component.ViewDataList.Clear();
                component.AddData(data);
                AddComponent(index, component);
            }
            else
            {
                var component = (ViewLoaderComponent)GetComponent(index);
                component.AddData(data);
                ReplaceComponent(index, component);
            }
        }

        public void ChangeViewLoad(ViewData data)
        {
            var index = LogicComponentsLookup.ComViewLoader;
            if (!hasComViewLoader)
            {
                var component = (ViewLoaderComponent)CreateComponent(index, typeof(ViewLoaderComponent));
                component.ViewDataList.Clear();
                component.AddData(data);
                AddComponent(index, component);
            }
            else
            {
                var component = (ViewLoaderComponent)GetComponent(index);
                bool finded = false;
                for (int i = 0; i < component.ViewDataList.Count; i++)
                {
                    if (component.ViewDataList[i].Category == data.Category)
                    {
                        finded = true;
                        component.ViewDataList[i] = data;
                        break;
                    }
                }

                if (!finded)
                {
                    component.AddData(data);
                }
                
                ReplaceComponent(index, component);
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComViewLoaderIndex = new(typeof(ViewLoaderComponent));
        public static int ComViewLoader => _ComViewLoaderIndex.Index;
    }
}