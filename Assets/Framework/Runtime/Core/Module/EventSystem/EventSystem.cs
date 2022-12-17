using LccModel;
using System.Collections.Generic;

namespace LccModel
{
    public class EventSystem : Singleton<EventSystem>, ISingletonFixedUpdate, ISingletonUpdate, ISingletonLateUpdate
    {
        public Dictionary<long, AObjectBase> aObjectBaseDict = new Dictionary<long, AObjectBase>();
        public Queue<long> fixedUpdate = new Queue<long>();
        public Queue<long> fixedUpdateCopy = new Queue<long>();
        public Queue<long> update = new Queue<long>();
        public Queue<long> updateCopy = new Queue<long>();
        public Queue<long> lateUpdate = new Queue<long>();
        public Queue<long> lateUpdateCopy = new Queue<long>();
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
        public void FixedUpdate()
        {
            while (fixedUpdate.Count > 0)
            {
                long id = fixedUpdate.Dequeue();
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = aObjectBaseDict[id];
                    if (aObjectBase is IFixedUpdate fixedUpdate)
                    {
                        fixedUpdate.FixedUpdate();
                    }
                    fixedUpdateCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref fixedUpdate, ref fixedUpdateCopy);
        }
        public void Update()
        {
            while (update.Count > 0)
            {
                long id = update.Dequeue();
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = aObjectBaseDict[id];
                    if (aObjectBase is IUpdate update)
                    {
                        update.Update();
                    }
                    updateCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref update, ref updateCopy);
        }
        public void LateUpdate()
        {
            while (lateUpdate.Count > 0)
            {
                long id = lateUpdate.Dequeue();
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = aObjectBaseDict[id];
                    if (aObjectBase is ILateUpdate lateUpdate)
                    {
                        lateUpdate.LateUpdate();
                    }
                    lateUpdateCopy.Enqueue(id);
                }
            }
            ObjectUtil.Swap(ref lateUpdate, ref lateUpdateCopy);
        }
    }
}