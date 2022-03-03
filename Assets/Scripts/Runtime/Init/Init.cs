using System;
using UnityEngine;

namespace LccModel
{
    public class Init : MonoBehaviour
    {
        public static event Action OnEventSystemFixedUpdate;
        public static event Action OnEventSystemUpdate;
        public static event Action OnEventSystemLateUpdate;
        public static event Action OnEventSystemQuit;
        void Start()
        {
            Manager.Instance.InitManager();
            EventManager.Instance.InitManager();
            UIEventManager.Instance.InitManager();

            EventManager.Instance.Publish(new Start()).Coroutine();

            DontDestroyOnLoad(gameObject);
        }
        void FixedUpdate()
        {
            ObjectBaseEventSystem.Instance.EventSystemFixedUpdate();
            OnEventSystemFixedUpdate?.Invoke();
        }
        void Update()
        {
            ObjectBaseEventSystem.Instance.EventSystemUpdate();
            OnEventSystemUpdate?.Invoke();
        }
        void LateUpdate()
        {
            ObjectBaseEventSystem.Instance.EventSystemLateUpdate();
            OnEventSystemLateUpdate?.Invoke();
        }
        void OnApplicationQuit()
        {
            OnEventSystemQuit?.Invoke();
            GameEntity.Instance.Dispose();
        }
    }
}