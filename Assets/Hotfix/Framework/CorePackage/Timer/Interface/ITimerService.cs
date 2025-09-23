using System;

namespace LccHotfix
{
    public interface ITimerService : IService
    {
        /// <summary>
        /// 注册计时器
        /// </summary>
        TimerTask Register(float duration, TimerUnitType unitType = TimerUnitType.Second, int loopCount = 1, bool ignoreTimeScale = false, Action onRegister = null, Action onComplete = null, Action<float> onUpdate = null);

        /// <summary>
        /// 销毁所有计时器
        /// </summary>
        void DisposeAll();

        /// <summary>
        /// 暂停所有计时器
        /// </summary>
        void PauseAll();

        /// <summary>
        /// 恢复所有计时器
        /// </summary>
        void ResumeAll();

        /// <summary>
        /// 更新所有计时器
        /// </summary>
        void UpdateAll();
    }
}