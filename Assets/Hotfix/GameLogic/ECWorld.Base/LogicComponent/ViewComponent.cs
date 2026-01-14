using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IViewWrapper
    {
        int Category { get; }
        string ViewName { get; set; }
        void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale);
        void ModifyVisible(bool visible, int flag);
        void RemoveVisible(int flag);
        void DisposeView();
    }

    public class ViewComponent : LogicComponent
    {
        private ListDictionary<int, IViewWrapper> mViewDict = new();
        public List<IViewWrapper> ViewList => mViewDict.List;
        public int ViewCount => ViewList.Count;


        public override void DisposeOnRemove()
        {
            base.DisposeOnRemove();
            foreach (var view in ViewList)
            {
                view.DisposeView();
            }

            mViewDict.Clear();
        }

        public IViewWrapper GetView(int category)
        {
            foreach (var view in ViewList)
            {
                if (view.Category == category)
                {
                    return view;
                }
            }

            return null;
        }

        public T GetView<T>(int category) where T : class, IViewWrapper
        {
            var view = GetView(category);
            if (view == null)
            {
                return null;
            }

            var theView = view as T;
            if (theView == null)
            {
                UnityEngine.Debug.LogWarning($"GetComView theView == null, newCategory={category}");
            }

            return theView;
        }

        public void AddView(IViewWrapper newView, bool silently = false)
        {
            var newCategory = newView.Category;
            if (RemoveView(newCategory))
            {
                UnityEngine.Debug.LogWarning($"AddView existView, newCategory={newCategory}");
            }

            mViewDict.Add(newView.Category, newView);
            if (!silently)
            {
                _owner.ReplaceComponent(LogicComponentsLookup.ComView, this);
            }
        }

        public bool RemoveView(int category)
        {
            var count = mViewDict.Count;
            if (count == 0)
            {
                return false;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                var view = ViewList[i];
                if (view.Category == category)
                {
                    view.DisposeView();
                    mViewDict.Remove(category);
                    if (ViewCount == 0)
                    {
                        _owner.RemoveComponent(LogicComponentsLookup.ComView);
                    }
                    else
                    {
                        _owner.ReplaceComponent(LogicComponentsLookup.ComView, this);
                    }

                    return true;
                }
            }

            return false;
        }

        public void RemoveAllView()
        {
            foreach (var iViewWrapper in mViewDict)
            {
                RemoveView(iViewWrapper.Category);
            }
        }

        public bool HasView(int category)
        {
            if (mViewDict.TryGetValue(category, out var value))
                return true;

            return false;
        }
    }


    public partial class LogicEntity
    {
        public ViewComponent comView
        {
            get { return (ViewComponent)GetComponent(LogicComponentsLookup.ComView); }
        }

        public bool hasComView
        {
            get { return HasComponent(LogicComponentsLookup.ComView); }
        }

        public void AddView(IViewWrapper newView)
        {
            var index = LogicComponentsLookup.ComView;
            if (HasComponent(LogicComponentsLookup.ComView))
            {
                comView.AddView(newView);
                var component = GetComponent(index);
                ReplaceComponent(index, component);
            }
            else
            {
                var component = (ViewComponent)CreateComponent(index, typeof(ViewComponent));
                component.AddView(newView, true);
                AddComponent(index, component);
            }
        }

        public void RemoveView(int category)
        {
            if (!hasComView)
                return;
            comView.RemoveView(category);
        }

        public T GetView<T>(int category) where T : class, IViewWrapper
        {
            if (!hasComView)
            {
                return null;
            }

            return comView.GetView<T>(category);
        }

        public void ModifyVisible<T>(int category, bool visible, int flag) where T : class, IViewWrapper
        {
            if (!hasComView)
            {
                return;
            }

            var view = comView.GetView<T>(category);
            view.ModifyVisible(visible, flag);
        }

        public void RemoveComView()
        {
            comView.RemoveAllView();
            RemoveComponent(LogicComponentsLookup.ComView);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComViewIndex = new(typeof(ViewComponent));
        public static int ComView => _ComViewIndex.Index;
    }
}