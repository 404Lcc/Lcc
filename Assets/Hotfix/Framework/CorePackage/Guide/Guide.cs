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
        private GuideTriggerBase _trigger;

        //逐步骤的新手引导
        private List<GuideStep> _guideStepList = new List<GuideStep>();

        //完成条件
        private GuideCondBase _finishCond;

        private int _curIndex = -1;
        private GuideStep _curStep;
        private bool _isRunning;
        private bool _isFinish;

        public int Id => _config.id;
        public int Type => _config.type;
        public int Priority => _config.priority;
        public GuideTriggerBase Trigger => _trigger;
        public bool IsRunning => _isRunning;
        public bool IsFinish => _isFinish;

        public Guide(GuideConfig guideCfg)
        {
            _config = guideCfg;
            var stateList = _config.stateList;
            _guideStepList.Clear();
            for (int i = 0; i < stateList.Count; i++)
            {
                GuideStep step = new GuideStep(this, stateList[i]);
                _guideStepList.Add(step);
            }

            GuideCondFactory factory = new GuideCondFactory();
            _finishCond = factory.CreateCond(this, _config.finishCond, _config.finishArgs);
        }


        /// <summary>
        /// 步骤Update
        /// </summary>
        public void Update()
        {
            if (_curStep == null)
                return;

            _curStep.Update();

            if (_curStep.Finish)
            {
                //出现异常了
                if (_curStep.IsExceptionQuit || _curStep.IsForceQuit)
                {
                    SetGuideFinish();
                    return;
                }

                NextStep();
            }
        }

        public void NextStep()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                UnityEngine.Debug.Log("[新手引导] 开始引导步骤" + _config.id);
            }

            _curIndex++;

            if (_curIndex >= _guideStepList.Count)
            {
                _curIndex = _guideStepList.Count;

                if (_finishCond.Trigger())
                {
                    SetGuideFinish();
                }

                return;
            }

            _curStep = _guideStepList[_curIndex];
            _curStep.Run();
        }

        /// <summary>
        /// 设置引导完成
        /// </summary>
        private void SetGuideFinish()
        {
            _isFinish = true;
        }

        /// <summary>
        /// 设置跳过当前整个新手引导
        /// </summary>
        public void SetJumpGuide()
        {
            SetGuideFinish();
        }

        public void Reset()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _curIndex = -1;
            _isFinish = false;
            for (int i = 0; i < _guideStepList.Count; i++)
            {
                _guideStepList[i].Reset();
            }
        }

        public void Release()
        {
            if (_curStep != null)
            {
                _curStep.Release();
            }

            if (_trigger != null)
            {
                _trigger.Release();
            }
        }

        /// <summary>
        /// 是否是最后一步
        /// </summary>
        /// <returns></returns>
        public bool IsLastStep()
        {
            return _curIndex >= _guideStepList.Count - 1;
        }
    }
}