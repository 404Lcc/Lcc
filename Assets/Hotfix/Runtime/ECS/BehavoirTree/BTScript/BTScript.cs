using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface IBTScript
    {
        void Init();
        void Dispose();


        void InitAgent(BTAgent agent);
        void Update(BTAgent agent, float dt);
        void LateUpdate(BTAgent agent, float dt);
        void DestroyAgent(BTAgent agent);
    }


    public class BTScript : IBTScript, ICoroutine
    {
        private Dictionary<BTAction, Action<BTAgent, object[]>> _trigger;

        public virtual void Init()
        {
            _trigger = new Dictionary<BTAction, Action<BTAgent, object[]>>();
        }
        public virtual void Dispose()
        {
            _trigger.Clear();
        }


        public virtual void InitAgent(BTAgent agent)
        {
        }

        public virtual void Update(BTAgent agent, float dt)
        {
        }
        public virtual void LateUpdate(BTAgent agent, float dt)
        {
        }
        public virtual void DestroyAgent(BTAgent agent)
        {
        }



        public virtual void RegisterCallback(BTAction trigger, Action<BTAgent, object[]> callback)
        {
            if (_trigger.ContainsKey(trigger))
                return;
            _trigger.Add(trigger, callback);
        }
        public virtual void Trigger(BTAction trigger, BTAgent agent, params object[] args)
        {
            if (!_trigger.ContainsKey(trigger))
                return;

            var callback = _trigger[trigger];
            callback(agent, args);
        }

        public virtual Action<BTAgent, object[]> GetTrigger(BTAction trigger)
        {
            if (!_trigger.ContainsKey(trigger))
                return null;

            var callback = _trigger[trigger];
            return callback;
        }
    }
}