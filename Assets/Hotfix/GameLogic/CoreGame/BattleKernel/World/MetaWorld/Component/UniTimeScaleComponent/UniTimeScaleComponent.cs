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
    }
    
    public class UniTimeScaleComponent : MetaComponent
    {
        protected MultChangeFloat_MIN mTimeSlowRatio = new MultChangeFloat_MIN(1);
        public float GameSpeed { get; protected set; } = 1f;
        public float TimeScale => mTimeSlowRatio.Value * GameSpeed;

        /// <summary>
        /// 设置游戏速度，向上设置用这个 >1，比如游戏倍速等。
        /// </summary>
        /// <param name="timeSpeed">大于1</param>
        public void SetGameSpeed(float timeSpeed)
        {
            GameSpeed = timeSpeed;
        }
        
        /// <summary>
        /// 设置游戏速度减缓，向下设置用这个 <1，比如子弹时间、暂停等。
        /// </summary>
        /// <param name="slowFlag">减速标签，不够就扩充</param>
        /// <param name="scale">缩小系数</param>
        public void SetTimeSlowRatio(ETimeSlowFlag slowFlag, float scale)
        {
            mTimeSlowRatio.AddChange(scale, (int)slowFlag);
            if(BattleLog.IsDebugEnabled)
                BattleLog.Debug($"设置TimeScale，来源是{slowFlag}，现在的TimeScale是{TimeScale}");
        }

        public void ClearTimeSlowRatio(ETimeSlowFlag slowFlag)
        {
            mTimeSlowRatio.RemoveChange((int)slowFlag);
            if(BattleLog.IsDebugEnabled)
                BattleLog.Debug($"清除TimeScale，来源是{slowFlag}，现在的TimeScale是{TimeScale}");
        }
    }
    
    public partial class MetaWorld
    {
        public UniTimeScaleComponent comUniTimeScale
        {
            get { return GetUniqueComponent<UniTimeScaleComponent>(MetaComponentsLookup.ComUniTimeScale); }
        }

        public bool hasComUniTimeScale
        {
            get { return HasUniqueComponent(MetaComponentsLookup.ComUniTimeScale); }
        }

        public void SetGameSpeed(float speed)
        {
            var index = MetaComponentsLookup.ComUniTimeScale;
            UniTimeScaleComponent component;
            if (!hasComUniTimeScale)
                component = (UniTimeScaleComponent)UniqueEntity.CreateComponent(index, typeof(UniTimeScaleComponent));
            else
                component = comUniTimeScale;
            component.SetGameSpeed(speed);
            SetUniqueComponent(index, component);
        }

        public void SetTimeSlow(float timeScale, ETimeSlowFlag flag = ETimeSlowFlag.SF_Init)
        {
            var index = MetaComponentsLookup.ComUniTimeScale;
            UniTimeScaleComponent component;
            if (!hasComUniTimeScale)
                component = (UniTimeScaleComponent)UniqueEntity.CreateComponent(index, typeof(UniTimeScaleComponent));
            else
                component = comUniTimeScale;
            component.SetTimeSlowRatio(flag, timeScale);
            SetUniqueComponent(index, component);
        }
        
        public void ClearTimeSlow(ETimeSlowFlag flag = ETimeSlowFlag.SF_Init)
        {
            var index = MetaComponentsLookup.ComUniTimeScale;
            UniTimeScaleComponent component;
            if (!hasComUniTimeScale)
                component = (UniTimeScaleComponent)UniqueEntity.CreateComponent(index, typeof(UniTimeScaleComponent));
            else
                component = comUniTimeScale;

            component.ClearTimeSlowRatio(flag);
            SetUniqueComponent(index, component);
        }
    }

    public static partial class MetaComponentsLookup
    {
        private static ComponentTypeIndex _ComUniTimeScaleIndex = new(typeof(UniTimeScaleComponent));
        public static int ComUniTimeScale => _ComUniTimeScaleIndex.Index;
    }
}
