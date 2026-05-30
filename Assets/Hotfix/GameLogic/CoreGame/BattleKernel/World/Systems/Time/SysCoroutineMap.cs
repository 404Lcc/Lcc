using Entitas;
using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    //SysCoroutine 可以分成多个世界： 比如 Meta、Logic
    public class SysCoroutine_Meta : IExecuteSystem
    {
        private readonly ECWorlds _worlds;
        private readonly CoroutineMapExecute<MetaEntity> _modeCoroutine;

        public SysCoroutine_Meta(ECWorlds ecWorlds)
        {
            _worlds = ecWorlds;
            _modeCoroutine = new CoroutineMapExecute<MetaEntity>(_worlds.MetaWorld.GetGroup(MetaMatcher.AllOf(MetaComponentsLookup.ComCoroutineMap)), MetaComponentsLookup.ComCoroutineMap);
        }

        void IExecuteSystem.Execute()
        {
            _modeCoroutine.Execute();
        }
    }

    public class SysCoroutine_Logic : IExecuteSystem
    {
        private readonly ECWorlds _worlds;
        private readonly CoroutineMapExecute<LogicEntity> _gameCoroutine;

        public SysCoroutine_Logic(ECWorlds ecWorlds)
        {
            _worlds = ecWorlds;
            _gameCoroutine = new CoroutineMapExecute<LogicEntity>(_worlds.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComCoroutineMap)), LogicComponentsLookup.ComCoroutineMap);
        }

        void IExecuteSystem.Execute()
        {
            _gameCoroutine.Execute();
        }
    }

    //MetaWorld 和 LogicWorld 复用的系统
    public class CoroutineMapExecute<TEntity> where TEntity : class, IEntity, new()
    {
        private readonly IGroup<TEntity> _group;
        private readonly int _cmptIndex;
        private List<int> _finishedList;

        public CoroutineMapExecute(IGroup<TEntity> group, int cmptIndex)
        {
            _group = group;
            _cmptIndex = cmptIndex;
            _finishedList = new List<int>(30);
        }

        public void Execute()
        {
            foreach (var e in _group.GetEntities())
            {
                var cmpt = (CoroutineMapComponent)e.GetComponent(_cmptIndex);
                if (cmpt == null || !cmpt.HasAnyCoroutine())
                    continue;

                foreach (var v in cmpt.CoroutineMap)
                {
                    Stack<IEnumerator> coroutine_stack = v.Value;
                    if (coroutine_stack == null || coroutine_stack.Count == 0)
                    {
                        _finishedList.Add(v.Key);
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
                foreach (var rKey in _finishedList)
                {
                    cmpt.RemoveCoroutine(rKey);
                }

                _finishedList.Clear();
            }
        }
    }
}