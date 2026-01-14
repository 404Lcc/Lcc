using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    //用栈支持嵌套 yield return Function()
    public class CoroutineStack : Stack<IEnumerator>, IReference
    {
        public void OnRecycle()
        {
            Clear();
        }
    }

    public class CoroutineStackDic : Dictionary<int, CoroutineStack>
    {
    }

    /*  CoroutineMapComponent:
        Entity 生命周期安全的协程， 有关于Entity的异步过程可以挂这个组件
    */
    public class CoroutineMapComponent : LogicComponent
    {
        //外部不可直接操作map
        public CoroutineStackDic CoroutineMap { get; private set; } = new();

        public void Init()
        {
            CoroutineMap.Clear();
        }

        public void Dispose()
        {
            CoroutineMap.Clear();
        }

        public bool HasAnyCoroutine()
        {
            if (CoroutineMap == null)
                return false;
            return CoroutineMap.Count > 0;
        }

        public bool HasCoroutine(int flagID)
        {
            if (CoroutineMap == null)
                return false;
            return CoroutineMap.ContainsKey(flagID);
        }

        public bool AddCoroutine(int flagID, IEnumerator coroutine)
        {
            if (CoroutineMap == null)
                return false;
            if (CoroutineMap.ContainsKey(flagID))
                return false;
            var stack = ReferencePool.Acquire<CoroutineStack>();
            stack.Push(coroutine);
            CoroutineMap[flagID] = stack;
            return true;
        }

        public bool RemoveCoroutine(int flagID)
        {
            if (CoroutineMap == null)
                return false;
            if (!CoroutineMap.ContainsKey(flagID))
                return false;
            var stack = CoroutineMap[flagID];
            stack.Clear();
            ReferencePool.Release(stack);

            CoroutineMap.Remove(flagID);
            return true;
        }
    }


    public partial class LogicEntity
    {
        public CoroutineMapComponent comCoroutineMap
        {
            get { return (CoroutineMapComponent)GetComponent(LogicComponentsLookup.ComCoroutineMap); }
        }

        public bool hasComCoroutineMap
        {
            get { return HasComponent(LogicComponentsLookup.ComCoroutineMap); }
        }

        public void AddComCoroutineMap()
        {
            var index = LogicComponentsLookup.ComCoroutineMap;
            var component = CreateComponent<CoroutineMapComponent>(index);
            component.Init();
            AddComponent(index, component);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComCoroutineMapIndex = new(typeof(CoroutineMapComponent));
        public static int ComCoroutineMap => _ComCoroutineMapIndex.Index;
    }

    public partial class MetaEntity
    {
        public CoroutineMapComponent comCoroutineMap
        {
            get { return (CoroutineMapComponent)GetComponent(MetaComponentsLookup.ComCoroutineMap); }
        }

        public bool hasComCoroutineMap
        {
            get { return HasComponent(MetaComponentsLookup.ComCoroutineMap); }
        }

        private void AddComCoroutineMap()
        {
            var index = MetaComponentsLookup.ComCoroutineMap;
            var component = CreateComponent<CoroutineMapComponent>(index);
            component.Init();
            AddComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex _ComCoroutineMapIndex = new(typeof(CoroutineMapComponent));
        public static int ComCoroutineMap => _ComCoroutineMapIndex.Index;
    }
}