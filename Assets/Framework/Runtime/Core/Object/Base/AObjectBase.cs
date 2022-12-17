using LccModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccModel
{
    public abstract class AObjectBase
    {
        [HideInInspector]
        public long Id { get; set; }
        [HideInInspector]
        public long InstanceId { get; protected set; }
        protected AObjectBase _parent;
        protected AObjectBase _domain;
        private bool _isComponent;
        private Dictionary<long, AObjectBase> _childrenDict = new Dictionary<long, AObjectBase>();
        private Dictionary<Type, AObjectBase> _componentDict = new Dictionary<Type, AObjectBase>();

        public bool IsDisposed => InstanceId == 0;
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
                if (value == this)
                {
                    return;
                }
                //严格限制parent必须要有domain,也就是说parent必须在数据树上面
                if (value.Domain == null)
                {
                    return;
                }
                if (this._parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this._parent == value)
                    {
                        return;
                    }
                    this._parent.InternalRemoveChildren(this);
                }
                _parent = value;
                _isComponent = false;
                _parent.InternalAddChildren(this);
                this.Domain = this._parent._domain;
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
                if (value == this)
                {
                    return;
                }
                //严格限制parent必须要有domain,也就是说parent必须在数据树上面
                if (value.Domain == null)
                {
                    return;
                }
                if (this._parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this._parent == value)
                    {
                        return;
                    }
                    this._parent.InternalRemoveComponent(this);
                }
                _parent = value;
                _isComponent = true;
                _parent.InternalAddComponent(this);
                this.Domain = this._parent._domain;
            }
        }
        public AObjectBase Domain
        {
            get
            {
                return _domain;
            }
            private set
            {
                if (value == null)
                {
                    return;
                }

                if (this._domain == value)
                {
                    return;
                }

                AObjectBase preDomain = this._domain;
                this._domain = value;

                if (preDomain == null)
                {
                    this.InstanceId = IdUtil.GenerateInstanceId();

                    EventSystem.Instance.Register(this);
                }

                // 递归设置孩子的Domain
                if (this._childrenDict != null)
                {

                    foreach (var item in _childrenDict.Values)
                    {
                        item.Domain = this._domain;
                    }

                }

                if (this._componentDict != null)
                {
                    foreach (var item in _componentDict.Values)
                    {
                        item.Domain = this._domain;
                    }


                }

            }
        }
        public Dictionary<long, AObjectBase> Children
        {
            get
            {
                if (this._childrenDict == null)
                {
                    this._childrenDict = new Dictionary<long, AObjectBase>();
                }
                return this._childrenDict;
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
        public void RemoveChildren<T>(long id) where T : AObjectBase
        {
            if (_childrenDict.TryGetValue(id, out AObjectBase aObjectBase))
            {
                InternalRemoveChildren(aObjectBase);
                aObjectBase.Dispose();
            }
        }
        public AObjectBase AddChildren(AObjectBase aObjectBase)
        {
            aObjectBase.EntityParent = this;
            return aObjectBase;
        }
        public AObjectBase AddChildren(Type type, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddChildren<P1>(Type type, P1 p1, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase, p1);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddChildren<P1, P2>(Type type, P1 p1, P2 p2, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase, p1, p2);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddChildren<P1, P2, P3>(Type type, P1 p1, P2 p2, P3 p3, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddChildren<P1, P2, P3, P4>(Type type, P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas)
        {
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T>(params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T, P1>(P1 p1, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase, p1);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T, P1, P2>(P1 p1, P2 p2, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase, p1, p2);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddChildren<T, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas) where T : AObjectBase
        {
            T aObjectBase = Create<T>();
            aObjectBase.Id = IdUtil.GenerateId();
            aObjectBase.EntityParent = this;
            EventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
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
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddComponent<P1>(Type type, P1 p1, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase, p1);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddComponent<P1, P2>(Type type, P1 p1, P2 p2, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase, p1, p2);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddComponent<P1, P2, P3>(Type type, P1 p1, P2 p2, P3 p3, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public AObjectBase AddComponent<P1, P2, P3, P4>(Type type, P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas)
        {
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            AObjectBase aObjectBase = Create(type);
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T>(params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T, P1>(P1 p1, params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase, p1);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T, P1, P2>(P1 p1, P2 p2, params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase, p1, p2);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3, params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase, p1, p2, p3);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
            return aObjectBase;
        }
        public T AddComponent<T, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4, params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                LogUtil.Error("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = Create<T>();
            aObjectBase.Id = Id;
            aObjectBase.ComponentParent = this;
            #region 自动索引
            if (this is GameObjectEntity entity)
            {
                if (entity.gameObject != null)
                {
                    aObjectBase.AutoReference(entity.gameObject);
                    aObjectBase.ShowView(entity.gameObject);
                }
            }
            #endregion

            EventSystem.Instance.Awake(aObjectBase, p1, p2, p3, p4);
            EventSystem.Instance.Start(aObjectBase);
            EventSystem.Instance.InitData(aObjectBase, datas);
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
            if (temp.InstanceId != aObjectBase.InstanceId)
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
        private void ShowView(GameObject gameObject, GameObject parent = null)
        {
            LccView view = gameObject.AddComponent<LccView>();
            view.className = GetType().Name;
            view.type = this;

            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }
        }
        #endregion
        #region 自动索引
        private void AutoReference(Transform transform)
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
        private void AutoReference(Transform transform, Dictionary<string, FieldInfo> fieldInfoDict)
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


            Transform[] childrens = transform.GetComponentsInChildren<Transform>(true);

            foreach (Transform item in childrens)
            {
                string itemName = item.name.ToLower();
                if (fieldInfoDict.ContainsKey(itemName))
                {
                    if (fieldInfoDict[itemName].FieldType.Equals(typeof(GameObject)))
                    {
                        fieldInfoDict[itemName].SetValue(this, item.gameObject);
                    }
                    else if (fieldInfoDict[itemName].FieldType.Equals(typeof(Transform)))
                    {
                        fieldInfoDict[itemName].SetValue(this, item);
                    }
                    else
                    {
                        fieldInfoDict[itemName].SetValue(this, item.GetComponent(fieldInfoDict[itemName].FieldType));
                    }
                }
            }
        }
        private void AutoReference(GameObject gameObject)
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
        public virtual void OnDestroy()
        {
        }
        #endregion
        #region Invoke
        public void Invoke(string methodName, object[] objs = null)
        {
            //TODO
            var m = GetType().GetMethod(methodName);
            m.Invoke(this, objs);
        }
        #endregion
        #region 协程

        private CoroutineObject _coroutineObject;
        public CoroutineObject CoroutineObject
        {
            get
            {
                if (_coroutineObject == null)
                {
                    _coroutineObject = new CoroutineObject();
                }
                return _coroutineObject;
            }
        }

        /// <summary>
        /// 开启协程
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public CoroutineHandler StartCoroutine(IEnumerator enumerator)
        {
            return CoroutineObject.StartCoroutine(enumerator);
        }

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="handler"></param>
        public void StopCoroutine(CoroutineHandler handler)
        {
            CoroutineObject.StopCoroutine(handler);
        }
        /// <summary>
        /// 恢复协程
        /// </summary>
        /// <param name="handler"></param>
        public void PauseCoroutine(CoroutineHandler handler)
        {
            CoroutineObject.PauseCoroutine(handler);
        }
        /// <summary>
        /// 暂停协程
        /// </summary>
        /// <param name="handler"></param>
        public void ResumeCoroutine(CoroutineHandler handler)
        {
            CoroutineObject.ResumeCoroutine(handler);
        }
        public void StopAllCoroutines()
        {
            CoroutineObject.StopAllCoroutines();
        }

        #endregion
        #region 销毁
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }


            EventSystem.Instance.Remove(this);
            InstanceId = 0;





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

            _domain = null;


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

            _parent = null;

            StopAllCoroutines();


        }
        #endregion
        #region 内部方法
        private void InternalAddChildren(AObjectBase aObjectBase)
        {
            _childrenDict.Add(aObjectBase.Id, aObjectBase);
        }
        private void InternalAddComponent(AObjectBase aObjectBase)
        {
            _componentDict.Add(aObjectBase.GetType(), aObjectBase);
        }
        private void InternalRemoveChildren(AObjectBase aObjectBase)
        {
            _childrenDict.Remove(aObjectBase.Id);
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
            return aObjectBase;
        }
        public static T Create<T>() where T : AObjectBase
        {
            T aObjectBase = Activator.CreateInstance<T>();
            return aObjectBase;
        }
        #endregion
    }
}