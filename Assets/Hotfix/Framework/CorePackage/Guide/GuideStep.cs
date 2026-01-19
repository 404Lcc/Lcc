using UnityEngine;

namespace LccHotfix
{
    public class GuideFinishState : GuideState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            
            _data.IsFsmOver = true;
        }
    }
    
    public class GuideStep
    {
        private int _guideId;
        private GuideStateNode _stateConfig;
        private GuideCondBase _overCond;
        private GuideFSM _fsm;
        private GuideStateData _data;
        private bool _finish;
        public bool IsExceptionQuit => _data.IsExceptionQuit;
        public bool IsForceQuit => _data.IsForceQuit;
        public bool Finish => _finish;

        /// <summary>
        /// 初始化
        /// </summary>
        public GuideStep(Guide guide, GuideStateNode stateNode)
        {
            _guideId = guide.Id;
            _stateConfig = stateNode;

            if (!string.IsNullOrEmpty(_stateConfig.overCond))
            {
                GuideCondFactory factory = new GuideCondFactory();
                _overCond = factory.CreateCond(guide, _stateConfig.overCond, _stateConfig.overArgs);
            }

            InitFSM();
        }

        private void InitFSM()
        {
            _data = new GuideStateData(_guideId, _stateConfig);
            _fsm = new GuideFSM(_data);
            _fsm.SetBlackboardValue("data", _data);

            GuideStateFactory stateFactory = new GuideStateFactory();
            var stateName = _data.StateConfig.stateName;
            string stateType = stateFactory.CreateState(_fsm, stateName);

            if (string.IsNullOrEmpty(stateType))
            {
                UnityEngine.Debug.LogError("[新手引导] GuideStep为空 引导id = " + _guideId);
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
            if (_finish)
                return;
            
            if (_data.IsFsmOver)
            {
                if (_data.IsExceptionQuit || _data.IsForceQuit)
                {
                    _finish = true;

                    _data.IsExceptionQuit = false;
                    _data.IsForceQuit = false;
                    return;
                }

                if (_overCond == null)
                {
                    _finish = true;
                }
                else
                {
                    if (_overCond.Trigger())
                    {
                        _finish = true;
                        return;
                    }
                }
            }


            _fsm.Update();
        }

        public void Reset()
        {
            _fsm.Reset();
            _finish = false;
        }

        public void Release()
        {
            _fsm.Release();
            _data.Reset();
        }
    }
}