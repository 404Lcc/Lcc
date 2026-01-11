using Entitas;
using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    //////////////////////////////////////////////////////////////////////////
    /*
        SysCoroutine 可以分成多个世界： 比如 Meta、Logic
    */

    public class SysCoroutine_Meta : IExecuteSystem
    {
        private readonly MetaWorld _metaWorld;
        private readonly CoroutineMapExecute<MetaEntity> _coroutine;

        public SysCoroutine_Meta(MetaWorld metaWorld)
        {
            _metaWorld = metaWorld;
            _coroutine = new CoroutineMapExecute<MetaEntity>(_metaWorld, _metaWorld.GetGroup(MetaMatcher.AllOf(MetaComponentsLookup.ComCoroutineMap)), MetaComponentsLookup.ComCoroutineMap);
        }

        void IExecuteSystem.Execute()
        {
            _coroutine.Execute();
        }
    }

    public class SysCoroutine_Logic : IExecuteSystem
    {
        private readonly LogicWorld _logicWorld;
        private readonly CoroutineMapExecute<LogicEntity> m_gameCoroutine;

        public SysCoroutine_Logic(LogicWorld logicWorld)
        {
            _logicWorld = logicWorld;
            m_gameCoroutine = new CoroutineMapExecute<LogicEntity>(_logicWorld, _logicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComCoroutineMap)), LogicComponentsLookup.ComCoroutineMap);
        }

        void IExecuteSystem.Execute()
        {
            m_gameCoroutine.Execute();
        }
    }

    //MetaWorld 和 LogicWorld 复用的系统
    public class CoroutineMapExecute<TEntity> where TEntity : Entity, new()
    {
        private readonly Context<TEntity> m_context;
        private readonly IGroup<TEntity> m_group;
        private readonly int m_cmptIndex;
        private List<int> m_finishedList;

        public CoroutineMapExecute(Context<TEntity> context, IGroup<TEntity> group, int cmptIndex)
        {
            m_context = context;
            m_group = group;
            m_cmptIndex = cmptIndex;
            m_finishedList = new List<int>(30);
        }

        public void Execute()
        {
            foreach (var e in m_group.GetEntities())
            {
                var cmpt = (CoroutineMapComponent)e.GetComponent(m_cmptIndex);
                if (cmpt == null || !cmpt.HasAnyCoroutine())
                    continue;

                foreach (var v in cmpt.CoroutineMap)
                {
                    Stack<IEnumerator> coroutine_stack = v.Value;
                    if (coroutine_stack == null || coroutine_stack.Count == 0)
                    {
                        m_finishedList.Add(v.Key);
                        continue;
                    }

                    IEnumerator c = coroutine_stack.Peek();

                    bool move_next = c.MoveNext();
                    if (move_next)
                    {
                        if (c.Current is IEnumerator)
                        {
                            coroutine_stack.Push(c.Current as IEnumerator);
                            continue;
                        }
                    }
                    else
                    {
                        coroutine_stack.Pop();
                    }
                }

                //删除结束了
                foreach (var rKey in m_finishedList)
                {
                    cmpt.RemoveCoroutine(rKey);
                }

                m_finishedList.Clear();
            }
        }
    }
}