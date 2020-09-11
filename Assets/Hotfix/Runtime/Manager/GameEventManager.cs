using System.Collections;
using System.Collections.Generic;

namespace Hotfix
{
    public delegate void GameEventDelegate();
    public delegate bool GameDispatcherConditionDelegate();
    public class GameEventManager : Singleton<GameEventManager>
    {
        public Hashtable gameEvents = new Hashtable();
        public Hashtable gameDispatcherConditions = new Hashtable();
        public override void Update()
        {
            AutoGameDispatcher();
        }
        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddGameEventDelegate(GameEventType type, GameEventDelegate value)
        {
            gameEvents.Add(type, value);
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="type"></param>
        public void RemoveGameEventDelegate(GameEventType type)
        {
            gameEvents.Remove(type);
        }
        /// <summary>
        /// 获取事件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameEventDelegate GetGameEventDelegate(GameEventType type)
        {
            return gameEvents[type] as GameEventDelegate;
        }
        /// <summary>
        /// 增加分发条件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddGameDispatcherCondition(GameEventType type, GameDispatcherConditionDelegate value)
        {
            gameDispatcherConditions.Add(type, value);
        }
        /// <summary>
        /// 移除分发条件
        /// </summary>
        /// <param name="type"></param>
        public void RemoveGameDispatcherCondition(GameEventType type)
        {
            gameDispatcherConditions.Remove(type);
        }
        /// <summary>
        /// 获取分发条件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameDispatcherConditionDelegate GetGameDispatcherCondition(GameEventType type)
        {
            return gameDispatcherConditions[type] as GameDispatcherConditionDelegate;
        }
        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="type"></param>
        public void DispatcherEvent(GameEventType type)
        {
            ((GameEventDelegate)gameEvents[type])?.Invoke();
        }
        /// <summary>
        /// 自动分发
        /// </summary>
        public void AutoGameDispatcher()
        {
            List<GameEventType> typeList = new List<GameEventType>();
            IDictionaryEnumerator enumerator = gameDispatcherConditions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                GameDispatcherConditionDelegate conditionDelegate = enumerator.Value as GameDispatcherConditionDelegate;
                if (conditionDelegate())
                {
                    typeList.Add((GameEventType)enumerator.Key);
                    DispatcherEvent((GameEventType)enumerator.Key);
                }
            }
            for (int i = 0; i < typeList.Count; i++)
            {
                RemoveGameEventDelegate(typeList[i]);
                RemoveGameDispatcherCondition(typeList[i]);
            }
        }
    }
}