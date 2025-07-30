using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal partial class Main : Module, IMainService
    {
        private readonly LinkedList<Module> _modules = new LinkedList<Module>();
        private readonly object _lock = new object();
        
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
                for (LinkedListNode<Module> current = _modules.Last; current != null; current = current.Previous)
                {
                    current.Value.Shutdown();
                }

                _modules.Clear();
                ReferencePool.ClearAll();
                MarshalUtility.FreeCachedHGlobal();
                Log.SetLogHelper(null);
            }
        }

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

                LinkedListNode<Module> current = _modules.First;
                while (current != null)
                {
                    if (module.Priority > current.Value.Priority)
                    {
                        break;
                    }

                    current = current.Next;
                }

                if (current != null)
                {
                    _modules.AddBefore(current, module);
                }
                else
                {
                    _modules.AddLast(module);
                }

                return module;
            }
        }
    }
}