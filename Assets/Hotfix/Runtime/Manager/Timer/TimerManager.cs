using UnityEngine;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 计时器单位类型
    /// </summary>
    public enum TimerUnitType
    {
        Millisecond,        //毫秒
        Second,             //秒
        Minute,             //分钟
        Hour,               //小时
        Day,                //天
    }

    /// <summary>
    /// 计时器任务
    /// </summary>
    public class TimerTask
    {
        private TimerUnitType _unitType;//单位类型
        private float _duration;//持续时间
        private int _loopCount;//循环次数（-1代表无限循环）
        private bool _ignoreTimeScale;//是否忽略时间缩放
        private Action _onRegister;//注册时的回调
        private Action<float> _onUpdate;//更新时的回调(每帧)
        private Action _onComplete;//完成时的回调
        private float _targetTime;//目标时间             
        private float _lastUpdateTime;//上一次更新的时间
        private bool _isDone;//是否完成(到达目标时间)
        private bool _isPaused;//是否暂停
        private bool _isDisposed;//是否被销毁

        public TimerUnitType UnityType => _unitType;
        public float Duration => _duration;
        public int LoopCount => _loopCount;
        public bool IgnoreTimeScale => _ignoreTimeScale;
        public Action OnRegister => _onRegister;
        public Action<float> OnUpdate => _onUpdate;
        public Action OnComplete => _onComplete;
        public bool IsCompleted => _isDone || _isDisposed;//是否完成

        /// <summary>
        /// 注册计时器
        /// </summary>
        public void Register(float duration, TimerUnitType unitType, int loopCount, bool ignoreTimeScale, Action onRegister, Action onComplete, Action<float> onUpdate)
        {
            _duration = ConvertUnitToSecond(duration, unitType);
            _unitType = unitType;
            _loopCount = loopCount;
            _ignoreTimeScale = ignoreTimeScale;
            _onRegister = onRegister;
            _onComplete = onComplete;
            _onUpdate = onUpdate;

            _onRegister?.Invoke();

            float curTime = GetWorldTime();
            _lastUpdateTime = curTime;
            _targetTime = curTime + _duration;
        }

        /// <summary>
        /// 重新设置持续时间
        /// </summary>
        public void ResetDuration(float duration)
        {
            _duration = duration;
            float curTime = GetWorldTime();
            _lastUpdateTime = curTime;
            _targetTime = curTime + _duration;
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        public void Pause()
        {
            if (_isPaused || IsCompleted)
            {
                return;
            }

            _isPaused = true;
        }

        /// <summary>
        /// 恢复计时器
        /// </summary>
        public void Resume()
        {
            if (!_isPaused || IsCompleted)
            {
                return;
            }

            _isPaused = false;
        }

        /// <summary>
        /// 销毁计时器
        /// </summary>
        public void Dispose()
        {
            if (IsCompleted)
            {
                return;
            }

            _isDisposed = true;
        }

        /// <summary>
        /// 更新计时器
        /// </summary>
        public void Update()
        {
            if (IsCompleted)
            {
                return;
            }

            if (_isPaused)
            {
                _targetTime += GetDeltaTime();
                _lastUpdateTime = GetWorldTime();
                return;
            }

            _onUpdate?.Invoke(GetLeftTime());
            _lastUpdateTime = GetWorldTime();
            if (GetWorldTime() >= _targetTime)
            {
                try
                {
                    _onComplete?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }

                if (_loopCount == -1)
                {
                    _targetTime = GetWorldTime() + _duration;
                }
                else
                {
                    _loopCount--;
                    if (_loopCount <= 0)
                    {
                        _isDone = true;
                    }
                    else
                    {
                        _targetTime = GetWorldTime() + _duration;
                    }
                }
            }
        }

        /// <summary>
        /// 得到剩余时间（秒）
        /// </summary>
        public float GetLeftTime()
        {
            if (IsCompleted || GetWorldTime() >= _targetTime)
            {
                return 0;
            }

            float leftTime = _targetTime - GetWorldTime();
            return leftTime;
        }

        /// <summary>
        /// 得到剩余时间比例
        /// </summary>
        public float GetLeftTimeRatio()
        {
            if (IsCompleted || GetWorldTime() >= _targetTime)
            {
                return 1;
            }

            float timeLeftRatio = GetLeftTime() / _duration;
            return timeLeftRatio;
        }

        /// <summary>
        /// 得到增量时间
        /// </summary>
        private float GetDeltaTime()
        {
            float deltaTime = GetWorldTime() - _lastUpdateTime;
            return deltaTime;
        }

        /// <summary>
        /// 得到时间(距游戏开始运行的时间)
        /// </summary>
        private float GetWorldTime()
        {
            float worldTime = _ignoreTimeScale ? Time.realtimeSinceStartup : Time.time;
            return worldTime;
        }

        /// <summary>
        /// 转换时间单位为秒
        /// </summary>
        private float ConvertUnitToSecond(float duration, TimerUnitType unitType)
        {
            switch (unitType)
            {
                case TimerUnitType.Millisecond:
                    return duration / 1000;
                case TimerUnitType.Second:
                    return duration;
                case TimerUnitType.Minute:
                    return duration * 60;
                case TimerUnitType.Hour:
                    return duration * 60 * 60;
                case TimerUnitType.Day:
                    return duration * 60 * 60 * 24;
                default:
                    return duration;
            }
        }
    }

    /// <summary>
    /// 计时器管理器
    /// </summary>
    internal class TimerManager : Module
    {
        public static TimerManager Instance => Entry.GetModule<TimerManager>();

        private List<TimerTask> _taskList = new List<TimerTask>();//计时器任务列表 
        private List<TimerTask> _taskListToAdd = new List<TimerTask>();//计时器任务列表(先缓存所有计时器)

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            UpdateAll();
        }

        internal override void Shutdown()
        {
            _taskList.Clear();
            _taskListToAdd.Clear();
        }

        /// <summary>
        /// 注册计时器
        /// </summary>
        public TimerTask Register(float duration, TimerUnitType unitType = TimerUnitType.Second, int loopCount = 1, bool ignoreTimeScale = false, Action onRegister = null, Action onComplete = null, Action<float> onUpdate = null)
        {
            TimerTask task = new TimerTask();
            task.Register(duration, unitType, loopCount, ignoreTimeScale, onRegister, onComplete, onUpdate);
            _taskListToAdd.Add(task);
            return task;
        }

        /// <summary>
        /// 销毁所有计时器
        /// </summary>
        public void DisposeAll()
        {
            foreach (var task in _taskList)
            {
                task.Dispose();
            }
            _taskListToAdd.Clear();
            _taskList.Clear();
        }

        /// <summary>
        /// 暂停所有计时器
        /// </summary>
        public void PauseAll()
        {
            foreach (var task in _taskList)
            {
                task.Pause();
            }
        }

        /// <summary>
        /// 恢复所有计时器
        /// </summary>
        public void ResumeAll()
        {
            foreach (var task in _taskList)
            {
                task.Resume();
            }
        }

        /// <summary>
        /// 更新所有计时器
        /// </summary>
        public void UpdateAll()
        {
            //用_taskListToAdd缓存一下是防止foreach时添加task报错
            foreach (var timer in _taskListToAdd)
            {
                _taskList.Add(timer);
            }
            _taskListToAdd.Clear();
            foreach (var timer in _taskList)
            {
                timer.Update();
            }

            //移除计时器任务
            for (int i = _taskList.Count - 1; i >= 0; i--)
            {
                if (_taskList[i].IsCompleted)
                {
                    _taskList.RemoveAt(i);
                }
            }
        }
    }
}