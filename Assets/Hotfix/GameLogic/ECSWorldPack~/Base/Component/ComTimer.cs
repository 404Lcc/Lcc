using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class ComTimer : LogicComponent
    {
        public List<TimerTask> timerList;

        public void AddTask(TimerTask timerTask)
        {
            if (timerList == null)
                timerList = new List<TimerTask>();

            timerList.Add(timerTask);
        }

        public override void Dispose()
        {
            foreach (var item in timerList)
            {
                item.Dispose();
            }

            timerList.Clear();

            base.Dispose();
        }
    }

    public partial class LogicEntity
    {
        public ComTimer comTimer
        {
            get { return (ComTimer)GetComponent(LogicComponentsLookup.ComTimer); }
        }

        public bool hasComTimer
        {
            get { return HasComponent(LogicComponentsLookup.ComTimer); }
        }


        public void AddTimer(TimerTask timerTask)
        {
            var index = LogicComponentsLookup.ComTimer;
            ComTimer component;
            if (!hasComTimer)
            {
                component = (ComTimer)CreateComponent(index, typeof(ComTimer));
                component.AddTask(timerTask);
                AddComponent(index, component);
            }
            else
            {
                component = (ComTimer)GetComponent(index);
                component.AddTask(timerTask);
                ReplaceComponent(index, component);
            }

        }

        public void AddDelay(float delay, Action<LogicEntity, object[]> action, params object[] args)
        {
            if (delay == 0)
            {
                action(this, args);
                return;
            }

            bool ignoreTimeScale = false;
            TimerTask timerTask = Main.TimerService.Register(delay, TimerUnitType.Second, 1, ignoreTimeScale, null, () =>
            {
                if (!hasComTimer)
                    return;

                action(this, args);
            });

            AddTimer(timerTask);
        }

        public void RemoveComTimer()
        {
            RemoveComponent(LogicComponentsLookup.ComTimer);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComTimerIndex = new ComponentTypeIndex(typeof(ComTimer));
        public static int ComTimer => ComTimerIndex.index;
    }
}