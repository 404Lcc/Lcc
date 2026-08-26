using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IViewWrapper
    {
        int Category { get; }
        string ViewName { get; set; }
        /// <summary>
        /// 从池取出后绑定 loaded/category/world，替代原构造函数。
        /// </summary>
        void Bind(IReceiveLoaded loaded, int category, ECWorlds world);
        void Init(long entityId, IViewLoader loader, IViewWrapper parent);
        void SyncTransform(long entityId, Vector3 position, Quaternion rotation, Vector3 scale);
        void ModifyVisible(bool visible, int flag);
        void RemoveVisible(int flag);
        void DisposeView();
    }

    public class ViewComponent : LogicComponent
    {
        private ListDictionary<int, IViewWrapper> _viewDict = new();
        public List<IViewWrapper> ViewList => _viewDict.List;
        public int ViewCount => ViewList.Count;

        public void ReplaceMaterial(int index)
        {
            foreach (var view in ViewList)
            {
                if (view is MainGameObjectView simpleView)
                    simpleView.ReplaceMaterial(index);
            }
        }

        public void RestoreMaterial()
        {
            ReplaceMaterial(0);
        }

        public override void DisposeOnRemove()
        {
            // 须在 base.DisposeOnRemove 之前归还，基类会把 _owner 置空
            var pool = _owner?.OwnerWorld?.ViewWrapperPool;
            foreach (var view in ViewList)
            {
                view.DisposeView();
                pool?.Release(view);
            }

            _viewDict.Clear();
            base.DisposeOnRemove();
        }

        public T MainActorView<T>() where T : class, IViewWrapper
        {
            return GetView<T>(EViewCategory.MainGameObject);
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
                //UnityEngine.Debug.LogWarning($"GetComView theView == null, newCategory={category}");
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

            _viewDict.Add(newView.Category, newView);
            if (!silently)
            {
                _owner.ReplaceComponent(LogicComponentsLookup.ComView, this);
            }
        }

        public bool RemoveView(int category)
        {
            var count = _viewDict.Count;
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
                    _owner?.OwnerWorld?.ViewWrapperPool.Release(view);
                    _viewDict.Remove(category);
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
            RestoreMaterial();
            foreach (var iViewWrapper in _viewDict)
            {
                RemoveView(iViewWrapper.Category);
            }
        }

        public bool HasView(int category)
        {
            if (_viewDict.TryGetValue(category, out var value))
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