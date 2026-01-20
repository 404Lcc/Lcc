using UnityEngine;

namespace LccHotfix
{
    public class GuideFinishState : GuideState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            
            _data.IsFsmFinish = true;
        }
    }
    
    public class GuideStep
    {
        private int _guideId;
        private GuideStepConfig _config;
        private GuideFinishCondBase _finishCond;
        private GuideFSM _fsm;
        private GuideStateData _data;
        private bool _isFinish;
        public bool IsExceptionQuit => _data.IsExceptionQuit;
        public bool IsForceQuit => _data.IsForceQuit;
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

            var stateName = _data.Config.stateName;
            bool succ = GuideStateFactory.CreateState(_fsm, stateName);

            if (!succ)
            {
                UnityEngine.Debug.LogError($"[新手引导] 引导步骤状态机初始化异常 引导id = {_guideId} 状态名 = {stateName}");
                return;
            }

            _fsm.AddNode<GuideFinishState>();
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
                if (_data.IsExceptionQuit || _data.IsForceQuit)
                {
                    _isFinish = true;

                    _data.IsExceptionQuit = false;
                    _data.IsForceQuit = false;
                    return;
                }

                if (_finishCond == null)
                {
                    _isFinish = true;
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
            _isFinish = false;
        }

        public void Release()
        {
            _fsm.Release();
            _data.Reset();
        }
    }
}