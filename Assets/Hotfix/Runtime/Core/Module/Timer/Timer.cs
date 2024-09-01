using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class Timer : Singleton<Timer>, ISingletonUpdate
    {
        private readonly Dictionary<long, TimerData> timerDataDict = new Dictionary<long, TimerData>();
        private readonly List<TimerData> timerDataList = new List<TimerData>();

        private long _idGenerator;

        private long GetId()
        {
            return ++this._idGenerator;
        }

        protected override void Dispose()
        {
            base.Dispose();
            timerDataDict.Clear();
            timerDataList.Clear();
            _idGenerator = 0;
        }

        public void Update()
        {
            for (int i = timerDataList.Count - 1; i >= 0; i--)
            {
                var item = timerDataList[i];
                item.duration += UnityEngine.Time.deltaTime;
                if (item.duration >= item.time)
                {
                    Run(item);
                    item.duration = 0;
                }
            }

        }


        private void Run(TimerData timerData)
        {
            switch (timerData.timerType)
            {
                case TimerType.OnceTimer:
                    {
                        Action action = (Action)timerData.obj;
                        action?.Invoke();

                        RemoveTimer(timerData.id);
                        break;
                    }
                case TimerType.RepeatedTimer:
                    {
                        Action action = (Action)timerData.obj;
                        action?.Invoke();
                        break;
                    }
            }
        }

        public void ResetTimer(long id)
        {
            if (!timerDataDict.TryGetValue(id, out var timerData))
            {
                return;
            }
            timerData.Reset();
        }
        private void AddTimer(TimerData timerData)
        {
            if (timerData.id == 0)
            {
                return;
            }
            if (timerDataDict.ContainsKey(timerData.id))
            {
                return;
            }
            timerDataDict.Add(timerData.id, timerData);
            timerDataList.Add(timerData);
        }
        public bool RemoveTimer(long id)
        {
            if (id == 0)
            {
                return false;
            }

            if (!timerDataDict.Remove(id, out var timerData))
            {
                return false;
            }
            timerDataList.Remove(timerData);
            return true;
        }



        //wait时间短并且逻辑需要连贯的建议WaitAsync
        //wait时间长不需要逻辑连贯的建议用NewOnceTimer
        public long NewOnceTimer(long time, Action action)
        {
            TimerData timer = TimerData.Create(this.GetId(), TimerType.OnceTimer, 0, time / 1000f, 0, action);
            AddTimer(timer);
            return timer.id;
        }




        public long NewFrameTimer(Action action)
        {
            return NewOnceTimer((long)(1 / 60f * 1000), action);
        }


        public long NewRepeatedTimer(long time, Action action)
        {
            TimerData timer = TimerData.Create(GetId(), TimerType.RepeatedTimer, 0, time / 1000f, 0, action);
            AddTimer(timer);
            return timer.id;
        }

    }
}