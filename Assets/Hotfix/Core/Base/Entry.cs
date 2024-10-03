using System;
using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 游戏框架入口。
    /// </summary>
    public static class Entry
    {
        private static readonly LinkedList<Module> s_Modules = new LinkedList<Module>();

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (Module module in s_Modules)
            {
                module.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (LinkedListNode<Module> current = s_Modules.Last; current != null; current = current.Previous)
            {
                current.Value.Shutdown();
            }

            s_Modules.Clear();
            ReferencePool.ClearAll();
            MarshalUtility.FreeCachedHGlobal();
            Log.SetLogHelper(null);
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : class
        {
            //Type interfaceType = typeof(T);
            //if (!interfaceType.IsInterface)
            //{
            //    throw new Exception(string.Format("You must get module by interface, but '{0}' is not.", interfaceType.FullName));
            //}

            //if (!interfaceType.FullName.StartsWith("GameFramework.", StringComparison.Ordinal))
            //{
            //    throw new Exception(string.Format("You must get a Game Framework module, but '{0}' is not.", interfaceType.FullName));
            //}

            //string moduleName = string.Format("{0}.{1}", interfaceType.Namespace, interfaceType.Name.Substring(1));
            string moduleName = typeof(T).FullName;
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null)
            {
                throw new Exception(string.Format("Can not find Game Framework module type '{0}'.", moduleName));
            }

            return GetModule(moduleType) as T;
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要获取的游戏框架模块类型。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        private static Module GetModule(Type moduleType)
        {
            foreach (Module module in s_Modules)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }

        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        private static Module CreateModule(Type moduleType)
        {
            Module module = (Module)Activator.CreateInstance(moduleType);
            if (module == null)
            {
                throw new Exception(string.Format("Can not create module '{0}'.", moduleType.FullName));
            }

            LinkedListNode<Module> current = s_Modules.First;
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
                s_Modules.AddBefore(current, module);
            }
            else
            {
                s_Modules.AddLast(module);
            }

            return module;
        }
    }
}