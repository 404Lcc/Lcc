using System;
using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    public class GameEventManager : Singleton<GameEventManager>
    {
        public Hashtable gameEvents = new Hashtable();
        public Hashtable gameEventConditions = new Hashtable();
        public void AddGameEvent(GameEventType type, Action gameEvent, Func<bool> gameEventCondition)
        {
            gameEvents.Add(type, gameEvent);
            gameEventConditions.Add(type, gameEventCondition);
        }
        public void RemoveGameEvent(GameEventType type)
        {
            gameEvents.Remove(type);
            gameEventConditions.Remove(type);
        }
        public Action GetGameEvent(GameEventType type)
        {
            return (Action)gameEvents[type];
        }
        public Func<bool> GetGameEventCondition(GameEventType type)
        {
            return (Func<bool>)gameEventConditions[type];
        }
        public bool Publish(GameEventType type)
        {
            if ((bool)((Func<bool>)gameEventConditions[type])?.Invoke())
            {
                ((Action)gameEvents[type])?.Invoke();
                return true;
            }
            return false;
        }
        public void AutoPublish()
        {
            List<GameEventType> typeList = new List<GameEventType>();
            IDictionaryEnumerator enumerator = gameEventConditions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                GameEventType type = (GameEventType)enumerator.Key;
                if (Publish(type))
                {
                    typeList.Add(type);
                }
            }
            for (int i = 0; i < typeList.Count; i++)
            {
                RemoveGameEvent(typeList[i]);
            }
        }
    }
}