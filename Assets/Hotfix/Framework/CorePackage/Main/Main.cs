using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal abstract partial class Main : Module, IMainService, ICoroutine
    {
        private readonly LinkedList<Module> _modules = new LinkedList<Module>();
        private readonly object _lock = new object();

        public static Main Current { get; set; }

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (_lock) // 加锁
            {
                foreach (Module module in _modules)
                {
                    module.Update(elapseSeconds, realElapseSeconds);
                }
            }
        }

        internal override void LateUpdate()
        {
            lock (_lock) // 加锁
            {
                foreach (Module module in _modules)
                {
                    module.LateUpdate();
                }
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        internal override void Shutdown()
        {
            lock (_lock) // 加锁
            {
                this.StopAllCoroutines();
                for (LinkedListNode<Module> current = _modules.Last; current != null; current = current.Previous)
                {
                    current.Value.Shutdown();
                }

                _modules.Clear();
                ReferencePool.ClearAll();
                MarshalUtility.FreeCachedHGlobal();
                Log.SetLogHelper(null);
                Current = null;
            }
        }

        public abstract void OnInstall();

        public abstract IEnumerator OnInitialize();

        /// <summary>
        /// 增加游戏框架模块。
        /// </summary>
        public T AddModule<T>() where T : Module, IService
        {
            lock (_lock) // 加锁
            {
                string moduleName = typeof(T).FullName;
                Type moduleType = Type.GetType(moduleName);
                if (moduleType == null)
                {
                    throw new Exception(string.Format("Can not find Game Framework module type '{0}'.", moduleName));
                }

                return CreateModule(moduleType) as T;
            }
        }

        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        private Module CreateModule(Type moduleType)
        {
            lock (_lock) // 加锁
            {
                Module module = (Module)Activator.CreateInstance(moduleType);
                if (module == null)
                {
                    throw new Exception(string.Format("Can not create module '{0}'.", moduleType.FullName));
                }

                _modules.AddLast(module);

                return module;
            }
        }

        public static void SetMain(Main main)
        {
            if (Main.Current != null)
            {
                Debug.LogError("SetMain，已经存在Current");
                return;
            }

            Main.Current = main;
            main.OnInstall();
            main.StartCoroutine(main.OnInitialize());
        }
    }
}