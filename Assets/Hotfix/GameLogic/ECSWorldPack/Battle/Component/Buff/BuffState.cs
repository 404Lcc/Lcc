using cfg;
using System.Collections.Generic;

namespace LccHotfix
{
    public struct BuffInfo
    {
        public int buffId;
        public long fromEntityId;
        public long entityId;
        public int fromLogicId;
        public float during;
        public int level;
        public int maxLevel;
        public string btScript;
        public KVContext context;
    }

    public class BuffState
    {
        protected bool _isActive;
        protected BuffInfo _buffInfo;
        protected float _aliveTime;
        protected Buff _buffConfig;
        protected int _fxId;

        protected BTAgent _agent;
        protected List<BuffProperty> _propList = new List<BuffProperty>();

        public int BuffId => _buffInfo.buffId;
        public long FromEntityId => _buffInfo.fromEntityId;
        public long EntityId => _buffInfo.entityId;
        public int FromLogicId => _buffInfo.fromLogicId;
        public float During => _buffInfo.during;
        public int Level => _buffInfo.level;
        public int MaxLevel => _buffInfo.maxLevel;
        public string BTScript => _buffInfo.btScript;


        public Buff BuffConfig => _buffConfig;
        public BTAgent Agent => _agent;


        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public void Init(BuffInfo info)
        {
            _isActive = true;
            _buffInfo = info;
            _buffConfig = Main.ConfigService.Tables.TBBuff.Get(BuffId);

            var key = FromLogicId << 16 | BuffId;

            // 加bool属性
            for (int i = (int)BoolPropertyType.Invalid + 1; i <= (int)BoolPropertyType.IsStuning; i <<= 1)
            {
                if (((int)_buffConfig.BoolBuffType & i) == 0)
                    continue;

                var prop = new BoolBuffProperty();
                prop.Init(Level, i, EntityId, key, false);
                _propList.Add(prop);
            }

            // 加float属性
            foreach (var item in _buffConfig.ValueBuffType)
            {
                //todo
                var prop = new FloatBuffProperty();
                prop.Init(Level, (int)item.Type, EntityId, key, item.Value, false);
                _propList.Add(prop);
            }

            if (BTScript != "")
            {
                _agent = new BTAgent();
                _agent.Init(BTScript, EntityId, BuffId, info.context);
            }
        }

        public virtual void EnterState()
        {
            foreach (var item in _propList)
            {
                item.OnActive();
            }

            _agent?.Trigger(BTAction.OnActiveBuff);

            PlayFX();
        }

        public virtual void UpdateState(float dt)
        {
            if (!_isActive)
                return;

            _agent?.Update();

            _aliveTime += dt;
            if (During > 0 && _aliveTime >= During)
            {
                _isActive = false;
            }
        }

        public virtual void LeaveState()
        {
            foreach (var item in _propList)
            {
                item.OnDeactive();
            }

            _agent?.Trigger(BTAction.OnDeactiveBuff);
            _agent.Dispose();
            // Main.FXService.Release(_fxId);
        }

        /// <summary>
        /// 更新层数
        /// </summary>
        public void UpdateLevel(BuffInfo newInfo)
        {
            foreach (var item in _propList)
            {
                item.OnDeactive();
            }

            _agent?.Trigger(BTAction.OnDeactiveBuff);

            _aliveTime = 0;
            _propList.Clear();

            _isActive = true;
            _buffInfo = newInfo;

            var key = FromLogicId << 16 | BuffId;

            // 加bool属性
            for (int i = (int)BoolPropertyType.Invalid + 1; i <= (int)BoolPropertyType.IsStuning; i <<= 1)
            {
                if (((int)_buffConfig.BoolBuffType & i) == 0)
                    continue;

                var prop = new BoolBuffProperty();
                prop.Init(Level, i, EntityId, key, false);
                _propList.Add(prop);
            }

            // 加float属性
            foreach (var item in _buffConfig.ValueBuffType)
            {
                //todo
                var prop = new FloatBuffProperty();
                prop.Init(Level, (int)item.Type, EntityId, key, item.Value, false);
                _propList.Add(prop);
            }

            foreach (var item in _propList)
            {
                item.OnActive();
            }

            _agent?.Trigger(BTAction.OnActiveBuff);
        }

        public void TriggerAction(BTAction action, params object[] args)
        {
            _agent?.Trigger(action, args);
        }

        public float GetAliveTime()
        {
            return _aliveTime;
        }

        public void ResetAliveTime()
        {
            _aliveTime = 0;
        }

        // public FXObject GetFX()
        // {
        //     return Main.FXService.GetFX(_fxId);
        // }

        private void PlayFX()
        {
        }
    }
}