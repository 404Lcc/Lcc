namespace LccHotfix
{
    public class GuideStep
    {
        private int _guideId;
        private GuideStepConfig _config;
        private GuideFinishCondBase _finishCond;
        private GuideFSM _fsm;
        private GuideStateData _data;
        private bool _isException;
        private bool _isFinish;
        public bool IsException => _isException;
        public bool IsFinish => _isFinish;

        /// <summary>
        /// 初始化
        /// </summary>
        public GuideStep(Guide guide, GuideStepConfig config)
        {
            _guideId = guide.Id;
            _config = config;

            if (!string.IsNullOrEmpty(config.finishCond))
            {
                _finishCond = GuideFinishCondFactory.CreateCond(guide, config.finishCond, config.finishArgs);
            }

            InitFSM();
        }

        private void InitFSM()
        {
            _data = new GuideStateData(_guideId, _config);
            _fsm = new GuideFSM(_data);
            _fsm.SetBlackboardValue("data", _data);

            var stateName = _data.Config.defaultStateName;
            var node = GuideStateFactory.CreateState(stateName);

            if (node == null)
            {
                UnityEngine.Debug.LogError($"[新手引导] 引导步骤状态机初始化异常 引导id = {_guideId} 默认状态名 = {stateName}");
                return;
            }

            _fsm.AddNode(node);

            foreach (var item in _data.Config.generalStateList)
            {
                var generalNode = GuideStateFactory.CreateState(item);
                if (generalNode == null)
                {
                    UnityEngine.Debug.LogError($"[新手引导] 引导步骤状态机初始化异常 引导id = {_guideId} 通用状态名 = {item}");
                    continue;
                }

                _fsm.AddNode(generalNode);
            }

            _fsm.SetDefaultState(stateName);
        }

        public void Run()
        {
            _fsm.RunDefault();
        }

        public void Update()
        {
            if (_isFinish)
                return;
            
            if (_data.IsFsmFinish)
            {
                if (_data.IsFsmException)
                {
                    _isException = true;
                    _isFinish = true;
                    return;
                }

                if (_finishCond == null)
                {
                    _isFinish = true;
                    return;
                }
                else
                {
                    if (_finishCond.IsFinish())
                    {
                        _isFinish = true;
                        return;
                    }
                }
            }


            _fsm.Update();
        }

        public void Reset()
        {
            _fsm.Reset();
            _isException = false;
            _isFinish = false;
        }

        public void Release()
        {
            _isException = false;
            _isFinish = false;
            _fsm.Release();
            _data.Reset();
        }
    }
}