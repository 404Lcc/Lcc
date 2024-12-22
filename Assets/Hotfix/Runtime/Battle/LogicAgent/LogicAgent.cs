using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class LogicAgent
    {
        protected LogicScript _rootScript;

        protected long _entityId;
        protected int _logicId;
        protected KVContext _context;
        protected List<TimerTask> _timerList;

        public long EntityId => _entityId;
        public int LogicId => _logicId;
        public KVContext Context => _context;

        public void Init(string rootName, long entityId, int logicId, KVContext preContext = null)
        {
            _rootScript = LogicScriptManager.Instance.GetScript(rootName);

            _entityId = entityId;
            _logicId = logicId;

            _context = preContext;
            if (_context == null)
            {
                _context = new KVContext();
            }
            _timerList = new List<TimerTask>();

            _context.SetObject(KVType.RootScript, _rootScript);
            _rootScript.InitAgent(this);
        }

        public virtual void Update()
        {
            var entity = this.GetSelfEntity();
            _rootScript.Update(this, Time.deltaTime);
        }
        public virtual void LateUpdate()
        {
            var entity = this.GetSelfEntity();
            _rootScript.LateUpdate(this, Time.deltaTime);
        }


        public void Dispose()
        {
            foreach (var item in _timerList)
            {
                item.Dispose();
            }
            _timerList.Clear();
            _rootScript.DestroyAgent(this);
            _context.Clear();
        }

        public void Trigger(LogicAction trigger, params object[] args)
        {
            _rootScript.Trigger(trigger, this, args);
        }

        public void DelayTrigger(LogicAction trigger, float delay, params object[] args)
        {
            var action = _rootScript.GetTrigger(trigger);
            if (action != null)
            {
                AddDelay(delay, action, args);
            }
        }

        public void AddDelay(float delay, Action<LogicAgent, object[]> action, params object[] args)
        {
            if (delay == 0)
            {
                action(this, args);
                return;
            }

            var entity = this.GetSelfEntity();
            if (entity == null)
            {
                return;
            }


            var timer = TimerManager.Instance.Register(delay, TimerUnitType.Millisecond, 1, false, null, () =>
            {
                if (entity == null)
                    return;
                if (!entity.hasComTimer)
                    return;

                action(this, args);
            });

            entity.AddTimer(timer);
            _timerList.Add(timer);
        }
    }
}