using System;

namespace LccModel
{
    public static class ObjectBaseFactory
    {
        public static AObjectBase Create(Type type, AObjectBase parent = null, params object[] datas)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            aObjectBase.InitData(datas);
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            return aObjectBase;
        }
        public static T Create<T>(AObjectBase parent = null, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            aObjectBase.InitData(datas);
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            return aObjectBase;
        }
        public static T Create<T, P1>(AObjectBase parent, P1 p1, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            aObjectBase.InitData(datas);
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1);
            return aObjectBase;
        }
        public static T Create<T, P1, P2>(AObjectBase parent, P1 p1, P2 p2, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            aObjectBase.InitData(datas);
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2);
            return aObjectBase;
        }
        public static T Create<T, P1, P2, P3>(AObjectBase parent, P1 p1, P2 p2, P3 p3, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            aObjectBase.InitData(datas);
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            return aObjectBase;
        }
        public static T Create<T, P1, P2, P3, P4>(AObjectBase parent, P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            aObjectBase.Parent = parent;
            aObjectBase.InitData(datas);
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            return aObjectBase;
        }
    }
}