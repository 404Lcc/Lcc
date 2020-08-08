using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public delegate void GameEventDelegate();
    public delegate bool GameDispatcherConditionDelegate();
    public class GameEventManager : MonoBehaviour
    {
        public Hashtable gameevents;
        public Hashtable gamedispatcherconditions;
        void Awake()
        {
            InitManager();
        }
        void Start()
        {
        }
        void Update()
        {
            AutoGameDispatcher();
        }
        public void InitManager()
        {
            gameevents = new Hashtable();
            gamedispatcherconditions = new Hashtable();
        }
        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddGameEventDelegate(GameEventType type, GameEventDelegate value)
        {
            gameevents.Add(type, value);
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="type"></param>
        public void RemoveGameEventDelegate(GameEventType type)
        {
            gameevents.Remove(type);
        }
        /// <summary>
        /// 获取事件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameEventDelegate GetGameEventDelegate(GameEventType type)
        {
            return gameevents[type] as GameEventDelegate;
        }
        /// <summary>
        /// 增加分发条件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddGameDispatcherCondition(GameEventType type, GameDispatcherConditionDelegate value)
        {
            gamedispatcherconditions.Add(type, value);
        }
        /// <summary>
        /// 移除分发条件
        /// </summary>
        /// <param name="type"></param>
        public void RemoveGameDispatcherCondition(GameEventType type)
        {
            gamedispatcherconditions.Remove(type);
        }
        /// <summary>
        /// 获取分发条件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameDispatcherConditionDelegate GetGameDispatcherCondition(GameEventType type)
        {
            return gamedispatcherconditions[type] as GameDispatcherConditionDelegate;
        }
        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="type"></param>
        public void DispatcherEvent(GameEventType type)
        {
            GameEventDelegate eventdelegate = gameevents[type] as GameEventDelegate;
            if (eventdelegate != null)
            {
                eventdelegate();
            }
        }
        /// <summary>
        /// 自动分发
        /// </summary>
        public void AutoGameDispatcher()
        {
            List<GameEventType> list = new List<GameEventType>();
            IDictionaryEnumerator enumerator = gamedispatcherconditions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                GameDispatcherConditionDelegate conditiondelegate = enumerator.Value as GameDispatcherConditionDelegate;
                if (conditiondelegate())
                {
                    list.Add((GameEventType)enumerator.Key);
                    DispatcherEvent((GameEventType)enumerator.Key);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                RemoveGameEventDelegate(list[i]);
                RemoveGameDispatcherCondition(list[i]);
            }
        }
    }
}