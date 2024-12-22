using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface ILogicScript
    {
        void Init();
        void Dispose();


        void InitAgent(LogicAgent agent);
        void Update(LogicAgent agent, float dt);
        void LateUpdate(LogicAgent agent, float dt);
        void DestroyAgent(LogicAgent agent);
    }

    public enum LogicAction
    {
        OnReleaseSkill
    }

    public class LogicScript : ILogicScript, ICoroutine
    {
        private Dictionary<LogicAction, Action<LogicAgent, object[]>> _trigger;

        public virtual void Init()
        {
            _trigger = new Dictionary<LogicAction, Action<LogicAgent, object[]>>();
        }
        public virtual void Dispose()
        {
            _trigger.Clear();
        }


        public virtual void InitAgent(LogicAgent agent)
        {
        }

        public virtual void Update(LogicAgent agent, float dt)
        {
        }
        public virtual void LateUpdate(LogicAgent agent, float dt)
        {
        }
        public virtual void DestroyAgent(LogicAgent agent)
        {
        }



        public virtual void RegisterCallback(LogicAction trigger, Action<LogicAgent, object[]> callback)
        {
            if (_trigger.ContainsKey(trigger))
                return;
            _trigger.Add(trigger, callback);
        }
        public virtual void Trigger(LogicAction trigger, LogicAgent agent, params object[] args)
        {
            if (!_trigger.ContainsKey(trigger))
                return;

            var callback = _trigger[trigger];
            callback(agent, args);
        }

        public virtual Action<LogicAgent, object[]> GetTrigger(LogicAction trigger)
        {
            if (!_trigger.ContainsKey(trigger))
                return null;

            var callback = _trigger[trigger];
            return callback;
        }
    }
}