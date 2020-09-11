using System;
using UnityEngine;

namespace Model
{
    public class LccView : MonoBehaviour
    {
        [HideInInspector]
        public string className;
        public object type;
        public Action awake;
        public Action onEnable;
        public Action start;
        public Action fixedUpdate;
        public Action update;
        public Action lateUpdate;
        public Action onDisable;
        public Action onDestroy;
        public T GetType<T>()
        {
            return (T)type;
        }
        void Awake()
        {
            awake?.Invoke();
        }
        void OnEnable()
        {
            onEnable?.Invoke();
        }
        void Start()
        {
            start?.Invoke();
        }
        void FixedUpdate()
        {
            fixedUpdate?.Invoke();
        }
        void Update()
        {
            update?.Invoke();
        }
        void LateUpdate()
        {
            lateUpdate?.Invoke();
        }
        void OnDisable()
        {
            onDisable?.Invoke();
        }
        void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}