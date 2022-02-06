using System;

namespace LccHotfix
{
    public static class ObjectBaseFactory
    {
        public static AObjectBase Create(Type type, params object[] datas)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = null;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static AObjectBase Create(Type type, AObjectBase parent, params object[] datas)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static AObjectBase Create<P1>(Type type, AObjectBase parent, P1 p1, params object[] datas)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static AObjectBase Create<P1, P2>(Type type, AObjectBase parent, P1 p1, P2 p2, params object[] datas)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static AObjectBase Create<P1, P2, P3>(Type type, AObjectBase parent, P1 p1, P2 p2, P3 p3, params object[] datas)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static AObjectBase Create<P1, P2, P3, P4>(Type type, AObjectBase parent, P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static T Create<T>(params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = null;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static T Create<T>(AObjectBase parent, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static T Create<T, P1>(AObjectBase parent, P1 p1, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static T Create<T, P1, P2>(AObjectBase parent, P1 p1, P2 p2, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static T Create<T, P1, P2, P3>(AObjectBase parent, P1 p1, P2 p2, P3 p3, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public static T Create<T, P1, P2, P3, P4>(AObjectBase parent, P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
    }
}