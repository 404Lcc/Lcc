using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccModel
{
    public abstract class AObjectBase : IDisposable
    {
        private Dictionary<Type, AObjectBase> _componentDict = new Dictionary<Type, AObjectBase>();
        private AObjectBase _parent;
        [HideInInspector]
        public long id;
        public bool IsDisposed => id == 0;
        public AObjectBase Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (value == null)
                {
                    _parent = this;
                    return;
                }
                _parent = value;
            }
        }
        public T GetParent<T>() where T : AObjectBase
        {
            return (T)Parent;
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
        public T AddComponent<T>(params object[] datas) where T : AObjectBase
        {
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                Debug.LogError("Component已存在" + type.FullName);
                return null;
            }
            T aObjectBase = ObjectBaseFactory.Create<T>(this, datas);
            _componentDict.Add(typeof(T), aObjectBase);
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
            T aObjectBase = ObjectBaseFactory.Create<T, P1>(this, p1, datas);
            _componentDict.Add(typeof(T), aObjectBase);
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
            T aObjectBase = ObjectBaseFactory.Create<T, P1, P2>(this, p1, p2, datas);
            _componentDict.Add(typeof(T), aObjectBase);
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
            T aObjectBase = ObjectBaseFactory.Create<T, P1, P2, P3>(this, p1, p2, p3, datas);
            _componentDict.Add(typeof(T), aObjectBase);
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
            T aObjectBase = ObjectBaseFactory.Create<T, P1, P2, P3, P4>(this, p1, p2, p3, p4, datas);
            _componentDict.Add(typeof(T), aObjectBase);
            return aObjectBase;
        }
        public void RemoveComponent(AObjectBase aObjectBase)
        {
            if (IsDisposed)
            {
                return;
            }
            if (_componentDict == null)
            {
                return;
            }
            Type type = aObjectBase.GetType();
            if (_componentDict.ContainsKey(type))
            {
                _componentDict[type].Dispose();
                _componentDict.Remove(type);
            }
        }
        public void RemoveComponent<T>() where T : AObjectBase
        {
            if (IsDisposed)
            {
                return;
            }
            if (_componentDict == null)
            {
                return;
            }
            Type type = typeof(T);
            if (_componentDict.ContainsKey(type))
            {
                _componentDict[type].Dispose();
                _componentDict.Remove(type);
            }
        }
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
        public virtual void InitData(params object[] datas)
        {
        }
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
        public void Invoke(string methodName, float time)
        {
            //测试写法 后面时间优化
        }
        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            //测试写法 后面时间优化
            return GameObject.FindObjectOfType<Init>().StartCoroutine(enumerator);
        }
        public void StopCoroutine(IEnumerator enumerator)
        {
            //测试写法 后面时间优化
            GameObject.FindObjectOfType<Init>().StartCoroutine(enumerator);
        }
        public void StopCoroutine(Coroutine coroutine)
        {
            //测试写法 后面时间优化
            GameObject.FindObjectOfType<Init>().StopCoroutine(coroutine);
        }
        public void StopAllCoroutines()
        {
            //测试写法 后面时间优化
            GameObject.FindObjectOfType<Init>().StopAllCoroutines();
        }
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            ObjectBaseEventSystem.Instance.Remove(this);
            id = 0;
            if (_componentDict.Count > 0)
            {
                foreach (AObjectBase item in _componentDict.Values)
                {
                    item.Dispose();
                }
                _componentDict.Clear();
                _componentDict = null;
            }
            OnDestroy();
            if (Parent != null && !Parent.IsDisposed)
            {
                Parent.RemoveComponent(this);
            }
        }
    }
}