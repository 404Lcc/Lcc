using System.Collections.Generic;

namespace LccHotfix
{
    public class TimerComponent : LogicComponent
    {
        private List<TimerTask> mTimerList = new List<TimerTask>();
        public List<TimerTask> TimerList => mTimerList;

        public override void DisposeOnRemove()
        {
            foreach (var item in mTimerList)
            {
                item.Dispose();
            }

            mTimerList.Clear();

            base.DisposeOnRemove();
        }

        public void Init()
        {
            mTimerList.Clear();
        }
    }

    public partial class LogicEntity
    {
        public TimerComponent comTimer
        {
            get { return (TimerComponent)GetComponent(LogicComponentsLookup.ComTimer); }
        }

        public bool hasComTimer
        {
            get { return HasComponent(LogicComponentsLookup.ComTimer); }
        }


        public void AddComTimer(TimerTask timerID)
        {
            var index = LogicComponentsLookup.ComTimer;
            TimerComponent component;
            if (!hasComTimer)
            {
                component = (TimerComponent)CreateComponent(index, typeof(TimerComponent));
                component.Init();
                component.TimerList.Add(timerID);
                AddComponent(index, component);
            }
            else
            {
                component = (TimerComponent)GetComponent(index);
                component.TimerList.Add(timerID);
                ReplaceComponent(index, component);
            }
        }

        public void RemoveComTimer()
        {
            RemoveComponent(LogicComponentsLookup.ComTimer);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComTimerIndex = new(typeof(TimerComponent));
        public static int ComTimer => _ComTimerIndex.Index;
    }
}