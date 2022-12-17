using ET;
using LccModel;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public static class Game
    {
        private static readonly Dictionary<Type, ISingleton> _singletonDict = new Dictionary<Type, ISingleton>();
        private static readonly Stack<ISingleton> _singletons = new Stack<ISingleton>();

        private static readonly Queue<ISingleton> fixedUpdates = new Queue<ISingleton>();
        private static readonly Queue<ISingleton> updates = new Queue<ISingleton>();
        private static readonly Queue<ISingleton> lateUpdates = new Queue<ISingleton>();
        private static readonly Queue<ETTask> frameFinishTask = new Queue<ETTask>();

        public static Scene Scene => Root.Instance.Scene;

        public static T AddSingleton<T>() where T : Singleton<T>, new()
        {
            T singleton = new T();
            AddSingleton(singleton);

            return singleton;
        }
        public static void AddSingleton(ISingleton singleton)
        {
            Type singletonType = singleton.GetType();
            if (_singletonDict.ContainsKey(singletonType))
            {
                throw new Exception($"µ¥ÀýÒÑ´æÔÚ {singletonType.Name}");
            }

            _singletonDict.Add(singletonType, singleton);
            _singletons.Push(singleton);

            if (singleton is ISingletonFixedUpdate)
            {
                fixedUpdates.Enqueue(singleton);
            }

            if (singleton is ISingletonUpdate)
            {
                updates.Enqueue(singleton);
            }

            if (singleton is ISingletonLateUpdate)
            {
                lateUpdates.Enqueue(singleton);
            }

            singleton.Register();
        }

        public static async ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            await task;
        }
        public static void FrameFinishUpdate()
        {
            while (frameFinishTask.Count > 0)
            {
                ETTask task = frameFinishTask.Dequeue();
                task.SetResult();
            }
        }

        public static void FixedUpdate()
        {
            int count = fixedUpdates.Count;
            while (count-- > 0)
            {
                ISingleton singleton = fixedUpdates.Dequeue();

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonFixedUpdate fixedUpdate)
                {
                    continue;
                }

                fixedUpdates.Enqueue(singleton);
                try
                {
                    fixedUpdate.FixedUpdate();
                }
                catch (Exception e)
                {
                    LogUtil.Error(e);
                }
            }
        }


        public static void Update()
        {
            int count = updates.Count;
            while (count-- > 0)
            {
                ISingleton singleton = updates.Dequeue();

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonUpdate update)
                {
                    continue;
                }

                updates.Enqueue(singleton);
                try
                {
                    update.Update();
                }
                catch (Exception e)
                {
                    LogUtil.Error(e);
                }
            }
        }

        public static void LateUpdate()
        {
            int count = lateUpdates.Count;
            while (count-- > 0)
            {
                ISingleton singleton = lateUpdates.Dequeue();

                if (singleton.IsDisposed())
                {
                    continue;
                }

                if (singleton is not ISingletonLateUpdate lateUpdate)
                {
                    continue;
                }

                lateUpdates.Enqueue(singleton);
                try
                {
                    lateUpdate.LateUpdate();
                }
                catch (Exception e)
                {
                    LogUtil.Error(e);
                }
            }
        }
        public static void Close()
        {
            while (_singletons.Count > 0)
            {
                ISingleton iSingleton = _singletons.Pop();
                iSingleton.Destroy();
            }
            _singletonDict.Clear();
        }
    }
}