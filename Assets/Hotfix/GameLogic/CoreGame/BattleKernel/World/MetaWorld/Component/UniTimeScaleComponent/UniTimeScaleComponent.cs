namespace LccHotfix
{
    public enum ETimeSlowFlag
    {
        SF_Init = 1001,
        SF_Pause,
        SF_Choice,
        SF_Result,
        SF_ChoicePreview,
        SF_TutorialOverlay,
        SF_NarrativeOverlay,
        SF_Overlay,
        SF_BulletTime,
    }

    public class UniTimeScaleComponent : MetaComponent
    {
        protected MultChangeFloat_MIN mTimeSlowRatio = new MultChangeFloat_MIN(1);

        public float GameSpeed { get; protected set; } = 1f;
        public float TimeScale => mTimeSlowRatio.Value * GameSpeed;

        /// <summary>
        /// 设置游戏速度，向上设置用这个 &gt;1，比如游戏倍速等。
        /// </summary>
        public void SetGameSpeed(float timeSpeed)
        {
            GameSpeed = timeSpeed;
        }

        /// <summary>
        /// 设置游戏速度减缓，向下设置用这个 &lt;1，比如子弹时间、暂停等。
        /// </summary>
        public void SetTimeSlowRatio(ETimeSlowFlag slowFlag, float scale)
        {
            mTimeSlowRatio.AddChange(scale, (int)slowFlag);
            if (BattleLogger.IsDebugEnabled)
                BattleLogger.LogDebug($"设置TimeScale，来源是{slowFlag}，现在的TimeScale是{TimeScale}");
        }

        public void ClearTimeSlowRatio(ETimeSlowFlag slowFlag)
        {
            mTimeSlowRatio.RemoveChange((int)slowFlag);
            if (BattleLogger.IsDebugEnabled)
                BattleLogger.LogDebug($"清除TimeScale，来源是{slowFlag}，现在的TimeScale是{TimeScale}");
        }
    }

    public partial class MetaWorld
    {
        private BulletTimeController mBulletTimeController;

        private BulletTimeController BulletTime => mBulletTimeController ??= new BulletTimeController();

        public UniTimeScaleComponent comUniTimeScale
        {
            get { return GetUniqueComponent<UniTimeScaleComponent>(MetaComponentsLookup.ComUniTimeScale); }
        }

        public bool hasComUniTimeScale
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniTimeScale); }
        }

        private UniTimeScaleComponent GetOrCreateUniTimeScale(out int index)
        {
            index = MetaComponentsLookup.ComUniTimeScale;
            if (!hasComUniTimeScale)
            {
                return (UniTimeScaleComponent)UniqueEntity.CreateComponent(index, typeof(UniTimeScaleComponent));
            }

            return comUniTimeScale;
        }

        public void SetGameSpeed(float speed)
        {
            var component = GetOrCreateUniTimeScale(out var index);
            component.SetGameSpeed(speed);
            SetUniqueComponent(index, component);
        }

        public void SetTimeSlow(float timeScale, ETimeSlowFlag flag = ETimeSlowFlag.SF_Init)
        {
            var component = GetOrCreateUniTimeScale(out var index);
            component.SetTimeSlowRatio(flag, timeScale);
            SetUniqueComponent(index, component);
        }

        public void ClearTimeSlow(ETimeSlowFlag flag = ETimeSlowFlag.SF_Init)
        {
            var component = GetOrCreateUniTimeScale(out var index);
            component.ClearTimeSlowRatio(flag);
            SetUniqueComponent(index, component);
        }

        public void EnterBulletTime(float timeScale, float transitionDuration = -1f)
        {
            var component = GetOrCreateUniTimeScale(out var index);
            BulletTime.Enter(component, timeScale, transitionDuration);
            SetUniqueComponent(index, component);
        }

        public void LeaveBulletTime(float transitionDuration = -1f)
        {
            if (!hasComUniTimeScale)
            {
                return;
            }

            var index = MetaComponentsLookup.ComUniTimeScale;
            var component = comUniTimeScale;
            BulletTime.Leave(component, transitionDuration);
            SetUniqueComponent(index, component);
        }

        public void TickBulletTime(float unscaledDt)
        {
            if (!hasComUniTimeScale || mBulletTimeController == null)
            {
                return;
            }

            var index = MetaComponentsLookup.ComUniTimeScale;
            var component = comUniTimeScale;
            if (BulletTime.Tick(component, unscaledDt))
            {
                SetUniqueComponent(index, component);
            }
        }

        public void SetBulletTimeCompensateEestBoss(bool enabled)
        {
            BattleBulletTimeUtility.CompensateEestBossOnly = enabled;
        }

        public bool IsBulletTimeCompensateEestBoss()
        {
            return BattleBulletTimeUtility.CompensateEestBossOnly;
        }

        public void ForceStopBulletTime()
        {
            BattleBulletTimeUtility.CompensateEestBossOnly = false;

            if (mBulletTimeController == null && !hasComUniTimeScale)
            {
                return;
            }

            var index = MetaComponentsLookup.ComUniTimeScale;
            UniTimeScaleComponent component = null;
            if (hasComUniTimeScale)
            {
                component = comUniTimeScale;
            }

            BulletTime.ForceStop(component);
            if (component != null)
            {
                SetUniqueComponent(index, component);
            }
        }

        public bool IsInBulletTime()
        {
            return mBulletTimeController != null && mBulletTimeController.InBulletTime;
        }

        public float GetBulletTimeCompensateRatio()
        {
            if (mBulletTimeController == null || !hasComUniTimeScale)
            {
                return 1f;
            }

            return mBulletTimeController.GetCompensateRatio(comUniTimeScale.TimeScale);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex _ComUniTimeScaleIndex = new(typeof(UniTimeScaleComponent));
        public static int ComUniTimeScale => _ComUniTimeScaleIndex.Index;
    }
}
