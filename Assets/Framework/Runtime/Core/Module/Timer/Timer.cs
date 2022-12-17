using ET;
using System;
using System.Collections.Generic;

namespace LccModel
{
    public class Timer : Singleton<Timer>, ISingletonUpdate
    {

        private SortedDictionary<long, List<long>> _timerDict = new SortedDictionary<long, List<long>>();

        private readonly Queue<long> _timeOutTime = new Queue<long>();
        private readonly Queue<long> _timeOutTimerIds = new Queue<long>();
        private readonly Dictionary<long, TimerData> timerDataDict = new Dictionary<long, TimerData>();

        private long _idGenerator;

        // 记录最小时间
        private long _minTime = long.MaxValue;

        private long GetId()
        {
            return ++this._idGenerator;
        }
        private long GetNow()
        {
            return Time.Instance.ClientFrameTime();
        }

        protected override void Dispose()
        {
            base.Dispose();

            _timerDict.Clear();
            _minTime = 0;
            _timeOutTime.Clear();
            _timeOutTimerIds.Clear();
        }

        public void Update()
        {
            if (_timerDict.Count == 0)
            {
                return;
            }
            long timeNow = GetNow();

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

                TimerDictRemove(time);
            }

            while (_timeOutTimerIds.Count > 0)
            {
                long timerId = _timeOutTimerIds.Dequeue();

                if (!timerDataDict.Remove(timerId, out TimerData timerData))
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
                        ETTask tcs = (ETTask)timerData.obj;
                        tcs.SetResult();
                        break;
                    }
                case TimerType.RepeatedTimer:
                    {
                        long timeNow = GetNow();
                        timerData.startTime = timeNow;

                        Action action = (Action)timerData.obj;
                        action?.Invoke();

                        AddTimer(timerData);
                        break;
                    }
            }
        }






        private void AddTimer(TimerData timerData)
        {
            long tillTime = timerData.startTime + timerData.time;
            TimerDictAdd(tillTime, timerData.id);
            this.timerDataDict.Add(timerData.id, timerData);
            if (tillTime < _minTime)
            {
                this._minTime = tillTime;
            }
        }
        /// <summary>
        /// TimerDict增加
        /// </summary>
        /// <param name="time"></param>
        /// <param name="id"></param>
        private void TimerDictAdd(long time, long id)
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
        /// <summary>
        /// TimerDict移除
        /// </summary>
        /// <param name="time"></param>
        private void TimerDictRemove(long time)
        {
            if (_timerDict.TryGetValue(time, out List<long> list))
            {
                list.Clear();
                _timerDict.Remove(time);
            }
        }



        /// <summary>
        /// 移除计时器
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveTimer(ref long id)
        {
            long i = id;
            id = 0;
            return this.RemoveTimer(i);
        }
        /// <summary>
        /// 移除计时器
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool RemoveTimer(long id)
        {
            if (id == 0)
            {
                return false;
            }

            if (!this.timerDataDict.Remove(id, out var timerData))
            {
                return false;
            }
            return true;
        }













        public async ETTask WaitTillAsync(long tillTime, ETCancellationToken cancellationToken = null)
        {
            long timeNow = GetNow();
            if (timeNow >= tillTime)
            {
                return;
            }

            ETTask tcs = ETTask.Create(true);

            TimerData timer = TimerData.Create(GetId(), TimerType.OnceWaitTimer, timeNow, tillTime - timeNow, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.id;

            void CancelAction()
            {
                if (RemoveTimer(timerId))
                {
                    tcs.SetResult();
                }
            }

            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }


        public async ETTask WaitAsync(long time, ETCancellationToken cancellationToken = null)
        {
            if (time == 0)
            {
                return;
            }

            long timeNow = GetNow();

            ETTask tcs = ETTask.Create(true);

            TimerData timer = TimerData.Create(GetId(), TimerType.OnceWaitTimer, timeNow, time, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.id;

            void CancelAction()
            {
                if (RemoveTimer(timerId))
                {
                    tcs.SetResult();
                }
            }

            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }







        public async ETTask WaitFrameAsync(ETCancellationToken cancellationToken = null)
        {
            await WaitAsync(1, cancellationToken);
        }



        // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
        // wait时间短并且逻辑需要连贯的建议WaitTillAsync
        // wait时间长不需要逻辑连贯的建议用NewOnceTimer
        public long NewOnceTimer(long tillTime, Action action)
        {
            long timeNow = GetNow();
            if (tillTime < timeNow)
            {
                LogUtil.Error($"new once time too small: {tillTime}");
            }

            TimerData timer = TimerData.Create(this.GetId(), TimerType.OnceTimer, timeNow, tillTime - timeNow, 0, action);
            AddTimer(timer);
            return timer.id;
        }




        public long NewFrameTimer(Action action)
        {
            return NewRepeatedTimerInner(0, action);
        }



        public long NewRepeatedTimer(long time, Action action)
        {
            if (time < 100)
            {
                LogUtil.Error($"time too small: {time}");
                return 0;
            }

            return NewRepeatedTimerInner(time, action);
        }


        /// <summary>
        /// 创建一个RepeatedTimer
        /// </summary>
        private long NewRepeatedTimerInner(long time, Action action)
        {
            long timeNow = GetNow();
            TimerData timer = TimerData.Create(GetId(), TimerType.RepeatedTimer, timeNow, time, 0, action);

            // 每帧执行的不用加到timerId中，防止遍历
            AddTimer(timer);
            return timer.id;
        }

    }
}