using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ObjectBaseEventSystem
    {
        private static ObjectBaseEventSystem _instance;
        public static ObjectBaseEventSystem Instance
        {
            get
            {
                return _instance ?? (_instance = new ObjectBaseEventSystem());
            }
        }
        public Hashtable aObjectBases = new Hashtable();
        public Queue<long> awake = new Queue<long>();
        public Queue<long> awakeCopy = new Queue<long>();
        public Queue<long> onEnable = new Queue<long>();
        public Queue<long> onEnableCopy = new Queue<long>();
        public Queue<long> start = new Queue<long>();
        public Queue<long> startCopy = new Queue<long>();
        public Queue<long> fixedUpdate = new Queue<long>();
        public Queue<long> fixedUpdateCopy = new Queue<long>();
        public Queue<long> update = new Queue<long>();
        public Queue<long> updateCopy = new Queue<long>();
        public Queue<long> lateUpdate = new Queue<long>();
        public Queue<long> lateUpdateCopy = new Queue<long>();
        public Queue<long> onDisable = new Queue<long>();
        public Queue<long> onDisableCopy = new Queue<long>();
        public Queue<long> destroy = new Queue<long>();
        public Queue<long> destroyCopy = new Queue<long>();
        public void EventSystemFixedUpdate()
        {
            Awake();
            OnEnable();
            Start();
            FixedUpdate();
            OnDisable();
            Destroy();
        }
        public void EventSystemUpdate()
        {
            Update();
        }
        public void EventSystemLateUpdate()
        {
            LateUpdate();
        }
        public void Register(AObjectBase aObjectBase)
        {
            aObjectBases.Add(aObjectBase.id, aObjectBase);
            awake.Enqueue(aObjectBase.id);
            onEnable.Enqueue(aObjectBase.id);
            start.Enqueue(aObjectBase.id);
            fixedUpdate.Enqueue(aObjectBase.id);
            update.Enqueue(aObjectBase.id);
            lateUpdate.Enqueue(aObjectBase.id);
            onDisable.Enqueue(aObjectBase.id);
            destroy.Enqueue(aObjectBase.id);
        }
        private void Awake()
        {
            while (awake.Count > 0)
            {
                long id = awake.Dequeue();
                if (aObjectBases.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBases[id];
                    if (aObjectBase.gameObject.activeSelf && !aObjectBase.isAwake)
                    {
                        aObjectBase.isAwake = true;
                        aObjectBase.Awake();
                    }
                    else
                    {
                        awakeCopy.Enqueue(id);
                    }
                }
            }
            ObjectUtil.Swap(ref awake, ref awakeCopy);
        }
        private void OnEnable()
        {
            while (onEnable.Count > 0)
            {
                long id = onEnable.Dequeue();
                if (aObjectBases.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBases[id];
                    if (aObjectBase.lccView == null) continue;
                    if (aObjectBase.gameObject.activeSelf && aObjectBase.lccView.enabled && !aObjectBase.isOnEnable)
                    {
                        aObjectBase.isOnEnable = true;
                        aObjectBase.isOnDisable = false;
                        aObjectBase.OnEnable();
                    }
                    onEnableCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref onEnable, ref onEnableCopy);
        }
        private void Start()
        {
            while (start.Count > 0)
            {
                long id = start.Dequeue();
                if (aObjectBases.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBases[id];
                    if (aObjectBase.lccView == null) continue;
                    if (aObjectBase.gameObject.activeSelf && aObjectBase.lccView.enabled && !aObjectBase.isStart)
                    {
                        aObjectBase.isStart = true;
                        aObjectBase.Start();
                    }
                    else
                    {
                        startCopy.Enqueue(id);
                    }
                }
            }
            ObjectUtil.Swap(ref start, ref startCopy);
        }
        private void FixedUpdate()
        {
            while (fixedUpdate.Count > 0)
            {
                long id = fixedUpdate.Dequeue();
                if (aObjectBases.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBases[id];
                    if (aObjectBase.lccView == null) continue;
                    if (aObjectBase.gameObject.activeSelf && aObjectBase.lccView.enabled)
                    {
                        aObjectBase.FixedUpdate();
                    }
                    fixedUpdateCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref fixedUpdate, ref fixedUpdateCopy);
        }
        private void Update()
        {
            while (update.Count > 0)
            {
                long id = update.Dequeue();
                if (aObjectBases.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBases[id];
                    if (aObjectBase.lccView == null) continue;
                    if (aObjectBase.gameObject.activeSelf && aObjectBase.lccView.enabled)
                    {
                        aObjectBase.Update();
                    }
                    updateCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref update, ref updateCopy);
        }
        private void LateUpdate()
        {
            while (lateUpdate.Count > 0)
            {
                long id = lateUpdate.Dequeue();
                if (aObjectBases.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBases[id];
                    if (aObjectBase.lccView == null) continue;
                    if (aObjectBase.gameObject.activeSelf && aObjectBase.lccView.enabled)
                    {
                        aObjectBase.LateUpdate();
                    }
                    lateUpdateCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref lateUpdate, ref lateUpdateCopy);
        }
        private void OnDisable()
        {
            while (onDisable.Count > 0)
            {
                long id = onDisable.Dequeue();
                if (aObjectBases.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBases[id];
                    if (aObjectBase.lccView == null) continue;
                    if (aObjectBase.gameObject.activeSelf && !aObjectBase.lccView.enabled && !aObjectBase.isOnDisable)
                    {
                        aObjectBase.isOnEnable = false;
                        aObjectBase.isOnDisable = true;
                        aObjectBase.OnDisable();
                    }
                    onDisableCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref onDisable, ref onDisableCopy);
        }
        private void Destroy()
        {
            while (destroy.Count > 0)
            {
                long id = destroy.Dequeue();
                if (aObjectBases.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBases[id];
                    if (aObjectBase.gameObject == null || aObjectBase.lccView == null)
                    {
                        aObjectBase.OnDestroy();
                        aObjectBases.Remove(aObjectBase.id);
                        aObjectBase = null;
                    }
                    else
                    {
                        destroyCopy.Enqueue(id);
                    }
                }
            }
            ObjectUtil.Swap(ref destroy, ref destroyCopy);
        }
    }
}