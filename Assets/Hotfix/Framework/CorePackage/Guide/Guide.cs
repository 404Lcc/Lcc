using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 一个完整的新手引导(包含多个步骤)
    /// </summary>
    public class Guide
    {
        private GuideConfig _config;
        private IGuideMessage _guideMessage;

        //完成条件
        private GuideFinishCondBase _finishCond;

        private int _curIndex = -1;
        private GuideStep _curStep;
        private bool _isRunning;
        private bool _isFinish;

        public int Id => _config.id;
        public int Type => _config.type;
        public int Priority => _config.priority;
        public bool IsRunning => _isRunning;
        public bool IsFinish => _isFinish;

        public Guide(GuideConfig config, IGuideMessage guideMessage)
        {
            _config = config;
            _guideMessage = guideMessage;
        }

        /// <summary>
        /// 步骤Update
        /// </summary>
        public void Update()
        {
            if (_curStep == null)
            {
                if (_isRunning && !_isFinish && _curIndex >= _config.stepList.Count)
                {
                    if (_finishCond == null)
                    {
                        SetGuideFinish(false);
                    }
                    else if (_finishCond.IsFinish())
                    {
                        SetGuideFinish(false);
                    }
                }

                return;
            }

            _curStep.Update();

            switch (_curStep.StateType)
            {
                case GuideStateType.Interrupt:
                    Main.GuideService.ReAddGuideTrigger(_config.id);
                    Release();
                    break;
                case GuideStateType.ForceFinish:
                    _curStep.Release();
                    _curStep  = null;
                    SetGuideFinish(true);
                    break;
                case GuideStateType.Finish:
                    NextStep();
                    break;
            }
        }

        /// <summary>
        /// 没运行，则启动第0个，否则运行下一个
        /// </summary>
        public void NextStep()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                UnityEngine.Debug.Log("[新手引导] 开始引导步骤" + _config.id);
                if (_guideMessage != null)
                {
                    _guideMessage.GuideStart(_config.id);
                }
            }

            if (_curStep != null)
            {
                _curStep.Release();
                _curStep = null;
            }
            
            _curIndex++;

            if (_curIndex >= _config.stepList.Count)
            {
                _curIndex = _config.stepList.Count;

                if (_finishCond == null && !string.IsNullOrEmpty(_config.finishCond))
                {
                    if (_config.finishArgs == null)
                    {
                        _config.finishArgs = new List<string>();
                    }

                    _finishCond = GuideFinishCondFactory.CreateCond(this, _config.finishCond, _config.finishArgs);
                }
                return;
            }

            _curStep = new GuideStep(this, _config.stepList[_curIndex]);
            _curStep.Run();
        }

        /// <summary>
        /// 设置引导完成
        /// </summary>
        private void SetGuideFinish(bool isForceFinish)
        {
            ReleaseFinishCond();
            _isFinish = true;
            if (_guideMessage != null)
            {
                _guideMessage.GuideEnd(_config.id, isForceFinish);
            }
        }

        /// <summary>
        /// 设置跳过当前整个新手引导
        /// </summary>
        public void SetJumpGuide()
        {
            SetGuideFinish(false);
        }

        /// <summary>
        /// 释放引导
        /// </summary>
        public void Release()
        {
            _curIndex = -1;
            _isRunning = false;
            _isFinish = false;
            
            if (_curStep != null)
            {
                _curStep.Release();
                _curStep = null;
            }

            ReleaseFinishCond();
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

        /// <summary>
        /// 是否是最后一步
        /// </summary>
        /// <returns></returns>
        public bool IsLastStep()
        {
            return _curIndex >= _config.stepList.Count - 1;
        }
    }
}
