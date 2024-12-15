using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LccHotfix
{
    public enum ViewCategory
    {
        Actor,//主GameObject对象
        UI,//主要表现是UI
        Fx,//主要表现是特效

        //下面属于附属逻辑
        //生命周期不跟随Main
        //可以随时增加或者删除
    }
    public interface IViewUpdate
    {
        void Update(float dt);
    }
    public interface IViewWrapper : IDispose
    {
        void SyncTransform(Vector3 position, Quaternion rotation, Vector3 scale);
    }

    //快捷访问ActorView
    public class ViewCache : IDispose
    {
        public ActorView actorView;

        public void CheckViewCache(IViewWrapper newView, ViewCategory newViewCategory)
        {
            if (newViewCategory == ViewCategory.Actor)
            {
                actorView = newView as ActorView;
            }
        }

        public void Dispose()
        {
            actorView = null;
        }
    }

    public class ComView : LogicComponent
    {
        private Dictionary<ViewCategory, IViewWrapper> _viewDict = new Dictionary<ViewCategory, IViewWrapper>();
        private ViewCache _cache = new ViewCache();

        public Dictionary<ViewCategory, IViewWrapper> ViewDict => _viewDict;
        public List<IViewWrapper> ViewList => _viewDict.Values.ToList();
        public int ViewCount => _viewDict.Count;
        public ViewCache Cache => _cache;

        public bool HasActorView => ActorView != null;
        public ActorView ActorView => Cache.actorView;

        public override void Dispose()
        {
            base.Dispose();

            var viewList = _viewDict.Values.ToList();
            for (int i = 0; i < viewList.Count; i++)
            {
                viewList[i].Dispose();
            }

            _viewDict.Clear();
            _cache.Dispose();
        }

        public IViewWrapper GetView(ViewCategory newViewCategory)
        {
            if (_viewDict.TryGetValue(newViewCategory, out var view))
            {
                return view;
            }
            return null;
        }

        public T GetView<T>(ViewCategory newViewCategory) where T : class, IViewWrapper
        {
            var view = GetView(newViewCategory);
            if (view != null)
            {
                return view as T;
            }
            return null;
        }

        public void AddView(IViewWrapper newView, ViewCategory newViewCategory, bool needReplace = false)
        {
            if (_viewDict.ContainsKey(newViewCategory))
            {
                _viewDict.Remove(newViewCategory);
            }
            _viewDict.Add(newViewCategory, newView);
            //更新缓存
            _cache.CheckViewCache(newView, newViewCategory);

            if (!needReplace)
            {
                Owner.ReplaceComponent(LogicComponentsLookup.ComView, this);
            }
        }

        public bool RemoveView(ViewCategory newViewCategory)
        {
            if (_viewDict.TryGetValue(newViewCategory, out var view))
            {
                view.Dispose();

                _viewDict.Remove(newViewCategory);
                //清理缓存
                _cache.CheckViewCache(null, newViewCategory);

                if (ViewCount == 0)
                {
                    Owner.RemoveComponent(LogicComponentsLookup.ComView);
                }
                else
                {
                    Owner.ReplaceComponent(LogicComponentsLookup.ComView, this);
                }
                return true;
            }
            return false;
        }
    }

    public partial class LogicEntity
    {
        public ComView comView { get { return (ComView)GetComponent(LogicComponentsLookup.ComView); } }
        public bool hasComView { get { return HasComponent(LogicComponentsLookup.ComView); } }

        public void AddView(IViewWrapper newView, ViewCategory newViewCategory)
        {
            var index = LogicComponentsLookup.ComView;
            if (HasComponent(LogicComponentsLookup.ComView))
            {
                comView.AddView(newView, newViewCategory);
            }
            else
            {
                var component = (ComView)CreateComponent(index, typeof(ComView));
                AddComponent(index, component);
                component.AddView(newView, newViewCategory);
            }
        }

        public void RemoveView(ViewCategory newViewCategory)
        {
            if (!hasComView)
                return;
            comView.RemoveView(newViewCategory);
        }

        public T GetView<T>(ViewCategory newViewCategory) where T : class, IViewWrapper
        {
            if (!hasComView)
            {
                return null;
            }
            return comView.GetView<T>(newViewCategory);
        }

        public void RemoveComView()
        {
            RemoveComponent(LogicComponentsLookup.ComView);
        }
    }

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComView;

        public static Entitas.IMatcher<LogicEntity> ComView
        {
            get
            {
                if (_matcherComView == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComView);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComView = matcher;
                }

                return _matcherComView;
            }
        }
    }
    public static partial class LogicComponentsLookup
    {
        public static int ComView;
    }
}