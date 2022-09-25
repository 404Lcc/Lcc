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
        public Dictionary<long, AObjectBase> aObjectBaseDict = new Dictionary<long, AObjectBase>();
        public Queue<long> fixedUpdate = new Queue<long>();
        public Queue<long> fixedUpdateCopy = new Queue<long>();
        public Queue<long> update = new Queue<long>();
        public Queue<long> updateCopy = new Queue<long>();
        public Queue<long> lateUpdate = new Queue<long>();
        public Queue<long> lateUpdateCopy = new Queue<long>();
        public void EventSystemFixedUpdate()
        {
            FixedUpdate();
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
            aObjectBaseDict.Add(aObjectBase.InstanceId, aObjectBase);
            if (aObjectBase is IFixedUpdate)
            {
                fixedUpdate.Enqueue(aObjectBase.InstanceId);
            }
            if (aObjectBase is IUpdate)
            {
                update.Enqueue(aObjectBase.InstanceId);
            }
            if (aObjectBase is ILateUpdate)
            {
                lateUpdate.Enqueue(aObjectBase.InstanceId);
            }
        }
        public void Remove(AObjectBase aObjectBase)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBaseDict.Remove(aObjectBase.InstanceId);
            }
        }
        public void Awake(AObjectBase aObjectBase)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBase.Awake();
            }
        }
        public void Awake<P1>(AObjectBase aObjectBase, P1 p1)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBase.Awake(p1);
            }
        }
        public void Awake<P1, P2>(AObjectBase aObjectBase, P1 p1, P2 p2)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBase.Awake(p1, p2);
            }
        }
        public void Awake<P1, P2, P3>(AObjectBase aObjectBase, P1 p1, P2 p2, P3 p3)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBase.Awake(p1, p2, p3);
            }
        }
        public void Awake<P1, P2, P3, P4>(AObjectBase aObjectBase, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBase.Awake(p1, p2, p3, p4);
            }
        }
        public void Start(AObjectBase aObjectBase)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBase.Start();
            }
        }
        public void InitData(AObjectBase aObjectBase, object[] datas)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBase.InitData(datas);
            }
        }
        private void FixedUpdate()
        {
            while (fixedUpdate.Count > 0)
            {
                long id = fixedUpdate.Dequeue();
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBaseDict[id];
                    aObjectBase.FixedUpdate();
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
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBaseDict[id];
                    aObjectBase.Update();
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
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = (AObjectBase)aObjectBaseDict[id];
                    aObjectBase.LateUpdate();
                    lateUpdateCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref lateUpdate, ref lateUpdateCopy);
        }
    }
}