using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class BTAgent
    {
        protected BTScript _rootScript;

        protected long _id;
        protected int _logicId;
        protected bool _isFinish;
        protected KVContext _context;
        protected List<TimerTask> _timerList;

        public long Id => _id;
        public int LogicId => _logicId;
        public bool IsFinish => _isFinish;
        public KVContext Context => _context;

        public void Init(string rootName, long id, int logicId, KVContext preContext = null)
        {
            _rootScript = Main.BTScriptService.GetScript(rootName);

            _id = id;
            _logicId = logicId;

            _context = preContext;
            if (_context == null)
            {
                _context = new KVContext();
            }

            _timerList = new List<TimerTask>();

            _context.SetObject(KVType.RootScript.ToInt(), _rootScript);
            _rootScript.InitAgent(this);
        }

        public virtual void Update()
        {
            _rootScript.Update(this, Time.deltaTime);
        }

        public virtual void LateUpdate()
        {
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

        public void Trigger(BTAction trigger, params object[] args)
        {
            _rootScript.Trigger(trigger, this, args);
        }

        public void DelayTrigger(BTAction trigger, float delay, params object[] args)
        {
            var action = _rootScript.GetTrigger(trigger);
            if (action != null)
            {
                AddDelay(delay, action, args);
            }
        }

        public void AddDelay(float delay, Action<BTAgent, object[]> action, params object[] args)
        {
            if (delay == 0)
            {
                action(this, args);
                return;
            }

            var timer = Main.TimerService.Register(delay, TimerUnitType.Second, 1, false, null, () => { action(this, args); });

            _timerList.Add(timer);
        }

        public void SetFinish()
        {
            _isFinish = true;
        }
    }
}