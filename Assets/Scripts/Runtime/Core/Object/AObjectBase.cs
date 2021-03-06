﻿using System;
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
        public long id;
        [HideInInspector]
        public bool isAwake;
        [HideInInspector]
        public bool isOnEnable;
        [HideInInspector]
        public bool isStart;
        [HideInInspector]
        public bool isOnDisable;
        [HideInInspector]
        public LccView lccView;
        [HideInInspector]
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
        public void InitObject(GameObject gameObject, object data = null)
        {
            this.gameObject = gameObject;
            id = IdUtil.Generate();
            AutoReference();
            GameObject childGameObject = new GameObject(GetType().FullName);
            childGameObject.transform.SetParent(gameObject.transform);
            childGameObject.transform.localPosition = Vector3.zero;
            childGameObject.transform.localRotation = Quaternion.identity;
            childGameObject.transform.localScale = Vector3.one;
            lccView = childGameObject.AddComponent<LccView>();
            lccView.className = GetType().Name;
            lccView.type = this;
            InitData(data);
            ObjectBaseEventSystem.Instance.Register(this);
        }
        public void AutoReference()
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
        public virtual void InitData(object data)
        {
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
    }
}