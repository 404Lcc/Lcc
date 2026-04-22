using System.Collections.Generic;

namespace LccHotfix
{
    public enum GuideStateType
    {
        None,
        Interrupt,//中断
        ForceFinish,//强制完成
        Finish,//完成
    }

    public class GuideStepTempData
    {
        public bool IsInterrupt { get; set; } = false;
        public bool IsForceFinish { get; set; } = false;
        public bool IsFinish { get; set; } = false;
    }
    
    public class GuideStep
    {
        private Guide _guide;
        private int _guideId;
        private GuideStepConfig _config;
        private GuideFinishCondBase _finishCond;
        private GuideFSM _fsm;
        private GuideStepTempData _tempData;
        private GuideStateType _stateType;
        public GuideStateType StateType => _stateType;

        public GuideStep(Guide guide, GuideStepConfig config)
        {
            _guide = guide;
            _guideId = guide.Id;
            _config = config;
            if (_config.generalStateList == null)
            {
                _config.generalStateList = new List<string>();
            }
        }

        /// <summary>
        /// 启动步骤
        /// </summary>
        public void Run()
        {
            _tempData = new GuideStepTempData();
            _fsm = new GuideFSM(_guideId, _config, _tempData);

            var stateName = _config.defaultStateName;
            var node = GuideStateFactory.CreateState(stateName);

            if (node == null)
            {
                UnityEngine.Debug.LogError($"[新手引导] 引导步骤状态机初始化异常 引导id = {_guideId} 默认状态名 = {stateName}");
                return;
            }

            _fsm.AddNode(node);

            foreach (var item in _config.generalStateList)
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
            
            
            ReleaseFinishCond();

            if (!string.IsNullOrEmpty(_config.finishCond))
            {
                if (_config.finishArgs == null)
                {
                    _config.finishArgs = new List<string>();
                }

                _finishCond = GuideFinishCondFactory.CreateCond(_guide, _config.finishCond, _config.finishArgs);
            }

            _fsm.RunDefault();
        }

        /// <summary>
        /// 迭代
        /// </summary>
        public void Update()
        {
            if (_tempData.IsInterrupt)
            {
                _stateType = GuideStateType.Interrupt;
                return;
            }
            
            if (_tempData.IsForceFinish)
            {
                _stateType = GuideStateType.ForceFinish;
                return;
            }
            
            if (_tempData.IsFinish)
            {
                if (_finishCond == null)
                {
                    _stateType = GuideStateType.Finish;
                }
                else
                {
                    if (_finishCond.IsFinish())
                    {
                        _stateType = GuideStateType.Finish;
                    }
                }
                return;
            }

            _fsm.Update();
        }

        /// <summary>
        /// 释放步骤
        /// </summary>
        public void Release()
        {
            ReleaseFinishCond();
            _stateType = GuideStateType.None;
            _fsm.Release();
            _fsm = null;
            _tempData = null;
        }

        /// <summary>
        /// 释放完成条件
        /// </summary>
        private void ReleaseFinishCond()
        {
            if (_finishCond == null)
            {
                return;
            }

            _finishCond.Release();
            _finishCond = null;
        }
    }
}
