using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Model
{
    public class ObjectBase
    {
        public LccView lccView;
        public GameObject gameObject;
        public Transform transform
        {
            get
            {
                if (gameObject == null)
                {
                    return null;
                }
                return gameObject.transform;
            }
        }
        public void InitObjectBase(GameObject gameObject)
        {
            this.gameObject = gameObject;
            AutoReference();
            lccView = GameUtil.AddComponent<LccView>(gameObject);
            lccView.className = GetType().FullName;
            lccView.type = this;
            lccView.awake += Awake;
            lccView.onEnable += OnEnable;
            lccView.start += Start;
            lccView.fixedUpdate += FixedUpdate;
            lccView.update += Update;
            lccView.lateUpdate += LateUpdate;
            lccView.onDisable += OnDisable;
            lccView.onDestroy += Destroy;
            if (gameObject.activeSelf)
            {
                Awake();
                OnEnable();
            }
        }
        public void AutoReference()
        {
            Dictionary<string, FieldInfo> dic = new Dictionary<string, FieldInfo>();
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type objectType = typeof(Object);
            foreach (FieldInfo item in fields)
            {
                if (item.FieldType.IsSubclassOf(objectType))
                {
                    dic[item.Name.ToLower()] = item;
                }
            }
            if (dic.Count > 0)
            {
                AutoReference(transform, dic);
            }
        }
        public void AutoReference(Transform transform, Dictionary<string, FieldInfo> dic)
        {
            string name = transform.name.ToLower();
            if (dic.ContainsKey(name))
            {
                if (dic[name].FieldType.Equals(typeof(GameObject)))
                {
                    dic[name].SetValue(this, transform.gameObject);
                }
                else if (dic[name].FieldType.Equals(typeof(Transform)))
                {
                    dic[name].SetValue(this, transform);
                }
                else
                {
                    dic[name].SetValue(this, transform.GetComponent(dic[name].FieldType));
                }
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                AutoReference(transform.GetChild(i), dic);
            }
        }
        public void Invoke(string methodName, float time)
        {
            lccView?.Invoke(methodName, time);
        }
        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return lccView?.StartCoroutine(enumerator);
        }
        public void StopCoroutine(IEnumerator enumerator)
        {
            lccView?.StopCoroutine(enumerator);
        }
        public void StopCoroutine(Coroutine coroutine)
        {
            lccView?.StopCoroutine(coroutine);
        }
        public void StopAllCoroutines()
        {
            lccView?.StopAllCoroutines();
        }
        public virtual void Awake()
        {
        }
        public virtual void OnEnable()
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
        public virtual void OnDisable()
        {
        }
        public virtual void OnDestroy()
        {
        }
        private void Destroy()
        {
            lccView = null;
            gameObject = null;
            OnDestroy();
        }
    }
}