using LccModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public abstract class AObjectBase
    {
        [HideInInspector]
        public long id;
        private AObjectBase _parent;
        private bool _isComponent;
        private Dictionary<long, AObjectBase> _childrenDict = new Dictionary<long, AObjectBase>();
        private Dictionary<Type, AObjectBase> _componentDict = new Dictionary<Type, AObjectBase>();

        public bool IsDisposed => id == 0;
        public AObjectBase Parent
        {
            get
            {
                return _parent;
            }
        }
        private AObjectBase EntityParent
        {
            set
            {
                if (value == null)
                {
                    return;
                }
                _parent = value;
                _isComponent = false;
                _parent.InternalAddChildren(this);
            }
        }
        private AObjectBase ComponentParent
        {
            set
            {
                if (value == null)
                {
                    return;
                }
                _parent = value;
                _isComponent = true;
                _parent.InternalAddComponent(this);
            }
        }
        public Dictionary<long, AObjectBase> Children
        {
            get
            {
                if (_childrenDict == null)
                {
                    _childrenDict = new Dictionary<long, AObjectBase>();
                }
                return _childrenDict;
            }
        }
        public Dictionary<Type, AObjectBase> Components
        {
            get
            {
                if (_componentDict == null)
                {
                    _componentDict = new Dictionary<Type, AObjectBase>();
                }
                return _componentDict;
            }
        }
        public T GetParent<T>() where T : AObjectBase
        {
            return (T)Parent;
        }

        #region 实体
        public T GetChildren<T>(long id) where T : AObjectBase
        {
            if (_childrenDict.ContainsKey(id))
            {
                AObjectBase aObjectBase = _childrenDict[id];
                return (T)aObjectBase;
            }
            return null;
        }
        public AObjectBase AddChildren(AObjectBase aObjectBase)
        {
            aObjectBase.EntityParent = this;
            return aObjectBase;
        }
        public AObjectBase AddChildren(Type type, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddChildren<P1>(Type type, P1 p1, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddChildren<P1, P2>(Type type, P1 p1, P2 p2, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddChildren<P1, P2, P3>(Type type, P1 p1, P2 p2, P3 p3, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddChildren<P1, P2, P3, P4>(Type type, P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T>(params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T, P1>(P1 p1, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T, P1, P2>(P1 p1, P2 p2, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.EntityParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        #endregion
        #region 组件
        public AObjectBase GetComponent(Type type)
        {
            if (_componentDict.ContainsKey(type))
            {
                AObjectBase aObjectBase = _componentDict[type];
                return aObjectBase;
            }
            return null;
        }
        public T GetComponent<T>() where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                AObjectBase aObjectBase = _componentDict[type];
                return (T)aObjectBase;
            }
            return null;
        }
        public AObjectBase AddComponent(AObjectBase aObjectBase)
        {
            aObjectBase.ComponentParent = this;
            return aObjectBase;
        }
        public AObjectBase AddComponent(Type type, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddComponent<P1>(Type type, P1 p1, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddComponent<P1, P2>(Type type, P1 p1, P2 p2, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddComponent<P1, P2, P3>(Type type, P1 p1, P2 p2, P3 p3, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddComponent<P1, P2, P3, P4>(Type type, P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T>(params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T, P1>(P1 p1, params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T, P1, P2>(P1 p1, P2 p2, params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3, params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.ComponentParent = this;
            ObjectBaseEventSystem.Instance.Register(aObjectBase);
            ObjectBaseEventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            ObjectBaseEventSystem.Instance.Start(aObjectBase);
            ObjectBaseEventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public void RemoveComponent(AObjectBase aObjectBase)
        {
            if (IsDisposed)
            {
                return;
            }
            Type type = aObjectBase.GetType();
            AObjectBase temp = GetComponent(type);
            if (temp == null)
            {
                return;
            }
            InternalRemoveComponent(temp);
            temp.Dispose();
        }
        public void RemoveComponent(Type type)
        {
            if (IsDisposed)
            {
                return;
            }
            AObjectBase temp = GetComponent(type);
            if (temp == null)
            {
                return;
            }
            InternalRemoveComponent(temp);
            temp.Dispose();
        }
        public void RemoveComponent<T>() where T : AObjectBase
        {
            if (IsDisposed)
            {
                return;
            }
            T temp = GetComponent<T>();
            if (temp == null)
            {
                return;
            }
            InternalRemoveComponent(temp);
            temp.Dispose();
        }
        #endregion
        #region 显示
        public void ShowView(GameObject gameObject, GameObject parent = null)
        {
#if View
            LccView view = gameObject.AddComponent<LccView>();
            view.className = GetType().Name;
            view.type = this;
#endif
            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }
        }
        #endregion
        #region 自动引用
        public void AutoReference(Transform transform)
        {
            Dictionary<string, FieldInfo> fieldInfoDict = new Dictionary<string, FieldInfo>();
            FieldInfo[] fieldInfos = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type objectType = typeof(Object);
            foreach (FieldInfo item in fieldInfos)
            {
                if (item.FieldType.IsSubclassOf(objectType))
                {
                    fieldInfoDict[item.Name.ToLower()] = item;
                }
            }
            if (fieldInfoDict.Count > 0)
            {
                AutoReference(transform, fieldInfoDict);
            }
        }
        public void AutoReference(Transform transform, Dictionary<string, FieldInfo> fieldInfoDict)
        {
            string name = transform.name.ToLower();
            if (fieldInfoDict.ContainsKey(name))
            {
                if (fieldInfoDict[name].FieldType.Equals(typeof(GameObject)))
                {
                    fieldInfoDict[name].SetValue(this, transform.gameObject);
                }
                else if (fieldInfoDict[name].FieldType.Equals(typeof(Transform)))
                {
                    fieldInfoDict[name].SetValue(this, transform);
                }
                else
                {
                    fieldInfoDict[name].SetValue(this, transform.GetComponent(fieldInfoDict[name].FieldType));
                }
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                AutoReference(transform.GetChild(i), fieldInfoDict);
            }
        }
        public void AutoReference(GameObject gameObject)
        {
            AutoReference(gameObject.transform);
        }
        #endregion
        #region 生命周期
        public virtual void Awake()
        {
        }
        public virtual void Awake<P1>(P1 p1)
        {
        }
        public virtual void Awake<P1, P2>(P1 p1, P2 p2)
        {
        }
        public virtual void Awake<P1, P2, P3>(P1 p1, P2 p2, P3 p3)
        {
        }
        public virtual void Awake<P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4)
        {
        }
        public virtual void Start()
        {
        }
        public virtual void InitData(object[] datas)
        {
        }
        public virtual void FixedUpdate()
        {
        }
        public virtual void Update()
        {
        }
        public virtual void LateUpdate()
        {
        }
        public virtual void OnDestroy()
        {
        }
        #endregion
        #region Invoke
        public void Invoke(string methodName, object[] objs)
        {
            MethodInfo method = GetType().GetMethod(methodName);
            method.Invoke(this, objs);
        }
        #endregion
        #region 协程
        private CoroutineObject _coroutineObject = new CoroutineObject();
        public CoroutineHandler StartCoroutine(IEnumerator enumerator)
        {
            return _coroutineObject.StartCoroutine(enumerator);
        }
        public void StopCoroutine(CoroutineHandler handler)
        {
            handler.Stop();
        }
        public void PauseCoroutine(CoroutineHandler handler)
        {
            handler.Pause();
        }
        public void ResumeCoroutine(CoroutineHandler handler)
        {
            handler.Resume();
        }
        public void StopAllCoroutines()
        {
            _coroutineObject.StopAllCoroutines();
        }
        #endregion
        #region 销毁
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            ObjectBaseEventSystem.Instance.Remove(this);
            id = 0;
            if (_childrenDict.Count > 0)
            {
                foreach (AObjectBase item in _childrenDict.Values)
                {
                    item.Dispose();
                }
                _childrenDict.Clear();
            }
            if (_componentDict.Count > 0)
            {
                foreach (AObjectBase item in _componentDict.Values)
                {
                    item.Dispose();
                }
                _componentDict.Clear();
            }
            OnDestroy();
            if (Parent != null && !Parent.IsDisposed)
            {
                if (_isComponent)
                {
                    Parent.InternalRemoveComponent(this);
                }
                else
                {
                    Parent.InternalRemoveChildren(this);
                }
            }
            StopAllCoroutines();
        }
        #endregion
        #region 内部方法
        private void InternalAddChildren(AObjectBase aObjectBase)
        {
            _childrenDict.Add(aObjectBase.id, aObjectBase);
        }
        private void InternalAddComponent(AObjectBase aObjectBase)
        {
            _componentDict.Add(aObjectBase.GetType(), aObjectBase);
        }
        private void InternalRemoveChildren(AObjectBase aObjectBase)
        {
            _childrenDict.Remove(aObjectBase.id);
        }
        private void InternalRemoveComponent(AObjectBase aObjectBase)
        {
            _componentDict.Remove(aObjectBase.GetType());
        }
        #endregion
        #region 创建
        public static AObjectBase Create(Type type)
        {
            AObjectBase aObjectBase = (AObjectBase)Activator.CreateInstance(type);
            aObjectBase.id = IdUtil.Generate();
            return aObjectBase;
        }
        public static T Create<T>() where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            aObjectBase.id = IdUtil.Generate();
            return aObjectBase;
        }
        #endregion
    }
}