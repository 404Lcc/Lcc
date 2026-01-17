using System.Collections.Generic;

namespace LccHotfix
{
    public struct ViewLoaderData
    {
        public int Category;
        public string ObjName;
        public string BindPointName;
        public bool ClearFx;
        public System.Type CategoryType;
        public List<ViewLoaderData> SubViewList;
    }

    /*  ViewLoaderComponent: View资源加载组件
    不关联渲染对象，但渲染对象构建，依赖于其中存放的信息。服务器Entity也可以挂

    客户端：用AssetDetail提供的信息，来创建View组件（模型等引擎渲染对象）
    服务器：用它存放关于渲染对象的配置描述、状态细节，用于同步
    */
    public class ViewLoaderComponent : LogicComponent
    {
        public List<ViewLoaderData> ViewDataList { get; protected set; } = new();

        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
            ViewDataList.Clear();
        }

        public void AddData(ViewLoaderData loaderData)
        {
            ViewDataList.Add(loaderData);
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

        public void AddComViewLoader(ViewLoaderData loaderData)
        {
            var index = LogicComponentsLookup.ComViewLoader;
            if (!hasComViewLoader)
            {
                var component = (ViewLoaderComponent)CreateComponent(index, typeof(ViewLoaderComponent));
                component.ViewDataList.Clear();
                component.AddData(loaderData);
                AddComponent(index, component);
            }
            else
            {
                var component = (ViewLoaderComponent)GetComponent(index);
                component.AddData(loaderData);
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

        public void ChangeViewLoad(ViewLoaderData loaderData)
        {
            var index = LogicComponentsLookup.ComViewLoader;
            if (!hasComViewLoader)
            {
                var component = (ViewLoaderComponent)CreateComponent(index, typeof(ViewLoaderComponent));
                component.ViewDataList.Clear();
                component.AddData(loaderData);
                AddComponent(index, component);
            }
            else
            {
                var component = (ViewLoaderComponent)GetComponent(index);
                bool finded = false;
                for (int i = 0; i < component.ViewDataList.Count; i++)
                {
                    if (component.ViewDataList[i].Category == loaderData.Category)
                    {
                        finded = true;
                        component.ViewDataList[i] = loaderData;
                        break;
                    }
                }

                if (!finded)
                {
                    component.AddData(loaderData);
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