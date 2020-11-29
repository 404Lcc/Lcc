using System;
using System.Collections.Generic;

namespace LccModel
{
    public class GameEventManager : Singleton<GameEventManager>
    {
        public List<GameEventData> gameEventList = new List<GameEventData>();
        public int index;
        public override void Update()
        {
            AutomaticExcute();
        }
        public GameEventData AddGameEvent(Action callback, Func<bool> condition)
        {
            GameEventData gameEvent = new GameEventData(gameEventList.Count, callback, condition);
            gameEventList.Add(gameEvent);
            return gameEvent;
        }
        public void Excute(GameEventData gameEvent)
        {
            gameEvent.Excute();
            gameEvent.Reset();
        }
        public void AutomaticExcute()
        {
            if (index < gameEventList.Count)
            {
                Excute();
            }
        }
        public void Reset(GameEventData gameEvent)
        {
            gameEvent.Reset();
        }
        public void Excute()
        {
            GameEventData gameEvent = gameEventList[index];
            if (gameEvent.id == -1)
            {
                Next();
                return;
            }
            if (gameEvent.IsCondition())
            {
                gameEvent.Excute();
                Next();
            }
        }
        public void Next()
        {
            index += 1;
            if (index >= gameEventList.Count)
            {
                gameEventList.Clear();
                index = 0;
            }
        }
    }
}