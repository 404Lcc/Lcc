using Entitas;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public partial class LogicWorld
    {
        /// <summary>
        /// 预热 Entity 对象池：批量 Create 再 Destroy，填满 reusableEntities。
        /// 空实体不挂业务组件，避免 PostInitialize / Group / View 副作用。
        /// 必须先全部 Create 再 Destroy；若边建边毁，第二次 Create 会 Pop 刚入池的实例，池内永远只有 1 个。
        /// </summary>
        /// <param name="count">目标可复用数量；已有库存只补差额。</param>
        public void WarmupEntities(int count)
        {
            if (count <= 0)
            {
                return;
            }

            var need = count - reusableEntitiesCount;
            if (need <= 0)
            {
                return;
            }

            var created = new LogicEntity[need];
            for (int i = 0; i < need; i++)
            {
                created[i] = CreateEntity();
            }

            for (int i = 0; i < need; i++)
            {
                created[i].Destroy();
            }
        }

        /// <summary>
        /// 取得（必要时创建）指定 index 的 Component 池。与 Entity.GetComponentPool 共用同一数组。
        /// </summary>
        public Stack<IComponent> GetComponentPool(int index)
        {
            var pools = componentPools;
            var pool = pools[index];
            if (pool != null)
            {
                return pool;
            }

            pool = new Stack<IComponent>();
            pools[index] = pool;
            return pool;
        }

        /// <summary>
        /// 直接向 componentPools[index] Push 干净实例，不走 AddComponent。
        /// 首波 CreateComponent 即可 Pop，避开 Activator.CreateInstance。
        /// </summary>
        /// <param name="index">LogicComponentsLookup 下标。</param>
        /// <param name="count">目标库存；已有只补差额。</param>
        /// <param name="factory">与运行时 CreateComponent 等价的无参构造。</param>
        public void WarmupComponents(int index, int count, Func<IComponent> factory)
        {
            if (count <= 0 || factory == null || index < 0 || index >= totalComponents)
            {
                return;
            }

            var pool = GetComponentPool(index);
            var need = count - pool.Count;
            if (need <= 0)
            {
                return;
            }

            for (int i = 0; i < need; i++)
            {
                var component = factory();
                if (component == null)
                {
                    return;
                }

                pool.Push(component);
            }
        }

        /// <summary>
        /// 按类型预热 Component 池，使用 Activator.CreateInstance，与 AddCom* 的 CreateComponent(index, type) 一致。
        /// </summary>
        public void WarmupComponents(int index, Type componentType, int count)
        {
            if (componentType == null)
            {
                return;
            }

            WarmupComponents(index, count, () => (IComponent)Activator.CreateInstance(componentType));
        }

        /// <summary>
        /// 泛型预热 Component 池；new T() 与 Activator 均走默认构造，入池态等价。
        /// </summary>
        public void WarmupComponents<T>(int index, int count) where T : class, IComponent, new()
        {
            WarmupComponents(index, count, () => new T());
        }
    }
}
