using ET;
using System;
using System.Collections.Generic;

namespace LccModel
{
    public class TimerManager : Singleton<TimerManager>, IUpdate
    {
        private SortedDictionary<long, List<long>> _timerDict = new SortedDictionary<long, List<long>>();
        private long _minTime;

        private readonly Queue<long> _timeOutTime = new Queue<long>();
        private readonly Queue<long> _timeOutTimerIds = new Queue<long>();
        public override void Update()
        {
            if (_timerDict.Count == 0)
            {
                return;
            }
            long timeNow = TimeManager.Instance.ServerNow();

            if (timeNow < _minTime)
            {
                return;
            }
            foreach (long item in _timerDict.Keys)
            {
                if (item > timeNow)
                {
                    _minTime = item;
                    break;
                }
                _timeOutTime.Enqueue(item);
            }


            while (_timeOutTime.Count > 0)
            {
                long time = _timeOutTime.Dequeue();
                foreach (long item in _timerDict[time])
                {
                    _timeOutTimerIds.Enqueue(item);
                }

                Remove(time);
            }

            while (_timeOutTimerIds.Count > 0)
            {
                long timerId = _timeOutTimerIds.Dequeue();

                TimerData timerData = GetChildren<TimerData>(timerId);
                if (timerData == null)
                {
                    continue;
                }
                Run(timerData);
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
                        break;
                    }
                case TimerType.OnceWaitTimer:
                    {
                        ETTask<bool> tcs = (ETTask<bool>)timerData.obj;
                        RemoveTimer(timerData.id);
                        tcs.SetResult(true);
                        break;
                    }
                case TimerType.RepeatedTimer:
                    {
                        Action action = (Action)timerData.obj;
                        action?.Invoke();

                        AddTimer(TimeManager.Instance.ServerNow() + timerData.time, timerData);
                        break;
                    }
            }
        }




        public async ETTask<bool> WaitAsync(long time, ETCancellationToken cancellationToken = null)
        {
            if (time == 0)
            {
                return true;
            }
            time = TimeManager.Instance.ServerNow() + time;

            ETTask<bool> tcs = ETTask<bool>.Create(true);

            TimerData timerData = AddChildren<TimerData>(TimerType.OnceWaitTimer, time, tcs);
            AddTimer(time, timerData);
            long timerId = timerData.id;

            void CancelAction()
            {
                RemoveTimer(timerId);
                tcs.SetResult(false);
            }

            bool ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
            return ret;
        }
        public async ETTask<bool> WaitFrameAsync(int frame, ETCancellationToken cancellationToken = null)
        {
            if (frame == 0)
            {
                return true;
            }
            return await WaitAsync((long)(frame / 60f * 1000), cancellationToken);
        }

        public void RepeatedTimer(long time, Action action)
        {
            if (time == 0)
            {
                return;
            }

            TimerData timerData = AddChildren<TimerData>(TimerType.RepeatedTimer, time, action);
            AddTimer(TimeManager.Instance.ServerNow() + time, timerData);
        }



        public void OnceTimer(long time, Action action)
        {
            if (time == 0)
            {
                return;
            }
            time = TimeManager.Instance.ServerNow() + time;

            TimerData timer = AddChildren<TimerData>(TimerType.OnceTimer, time, action);
            AddTimer(time, timer);
        }



        private void AddTimer(long time, TimerData timerData)
        {
            Add(time, timerData.id);
            if (time < _minTime)
            {
                _minTime = time;
            }
        }
        private void RemoveTimer(long id)
        {
            TimerData timerData = GetChildren<TimerData>(id);
            if (timerData == null)
            {
                return;
            }
            timerData.Dispose();
        }











        private void Add(long time, long id)
        {
            if (_timerDict.TryGetValue(time, out List<long> list))
            {
                list.Add(id);
            }
            else
            {
                list = new List<long>();
                list.Add(id);
                _timerDict.Add(time, list);
            }
        }
        private void Remove(long time)
        {
            if (_timerDict.TryGetValue(time, out List<long> list))
            {
                list.Clear();
                _timerDict.Remove(time);
            }
        }
    }
}