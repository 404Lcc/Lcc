using System.Collections.Generic;

namespace LccHotfix
{
    public class GuideStep
    {
        private Guide _guide;
        private int _guideId;
        private GuideStepConfig _config;
        private GuideFinishCondBase _finishCond;
        private GuideFSM _fsm;
        private GuideStateData _data;
        private bool _isException;
        private bool _isFinish;
        public bool IsException => _isException;
        public bool IsFinish => _isFinish;

        public GuideStep(Guide guide, GuideStepConfig config)
        {
            _guide = guide;
            _guideId = guide.Id;
            _config = config;
        }

        /// <summary>
        /// 启动步骤
        /// </summary>
        public void Run()
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

            if (_data.Config.generalStateList == null)
            {
                _data.Config.generalStateList = new List<string>();
            }

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

        /// <summary>
        /// 释放步骤
        /// </summary>
        public void Release()
        {
            ReleaseFinishCond();
            _isException = false;
            _isFinish = false;
            _fsm.Release();
            _fsm = null;
            _data = null;
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
