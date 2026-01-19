using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public interface IGuideCheckFinish
    {
        bool CheckGuideFinish(int guideId);
    }

    internal class GuideManager : Module, IGuideService
    {
        //原生配置数据
        private GuideConfigList _forceGuideConfig;
        private GuideConfigList _noForceGuideConfig;
        private GuideSeqConfig _forceGuideSeqTriggerConfig;
        private GuideSeqConfig _noForceGuideSeqTriggerConfig;
        private List<GuideTriggerConfig> _triggerConfigList = new List<GuideTriggerConfig>();

        //根据GuideTrigger配置生成的触发列表
        private List<GuideTriggerBase> _guideTriggerList = new List<GuideTriggerBase>();

        //根据配置生成引导字典 key=引导id
        private Dictionary<int, Guide> _guideDict = new Dictionary<int, Guide>();

        //运行时强制引导数据
        private List<Guide> _runTriggerForceGuideList = new List<Guide>();

        //运行时弱引导数据
        private List<Guide> _runTriggerNoForceGuideList = new List<Guide>();

        //是否有强制引导
        private bool HasForceGuide => _runTriggerForceGuideList.Count > 0;
        private bool HasNoForceGuide => _runTriggerNoForceGuideList.Count > 0;

        private IGuideCheckFinish _guideCheckFinish;
        private AssetLoader _loader;

        public GuideManager()
        {
            _loader = new AssetLoader();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            for (int i = 0; i < _guideTriggerList.Count; i++)
            {
                if (_guideTriggerList[i].Trigger() && _runTriggerForceGuideList.Count <= 0)
                {
                    var guideId = _guideTriggerList[i].Config.guideId;
                    if (_guideDict.TryGetValue(guideId, out var guide))
                    {
                        switch (guide.Type)
                        {
                            case (int)EGuideType.Force:
                                _runTriggerForceGuideList.Add(guide);
                                break;
                            case (int)EGuideType.NoForce:
                                _runTriggerNoForceGuideList.Add(guide);
                                break;
                        }

                        _guideDict.Remove(guideId);
                    }
                }
            }

            //可以运行的多余强制引导先回去，等这个强制引导完成
            for (int i = 1; i < _runTriggerForceGuideList.Count; i++)
            {
                var guide = _runTriggerForceGuideList[i];
                if (guide.Trigger == null)
                    continue;
                if (guide.Trigger.Trigger())
                {
                    _guideDict.Add(guide.Id, guide);
                    _runTriggerForceGuideList.RemoveAt(i);
                }
            }

            //检查是否有可触发的弱引导
            if (_runTriggerForceGuideList.Count <= 0)
            {
                TriggerNoForceGuide();
            }
            else
            {
                TriggerForceGuide();
            }
        }

        internal override void Shutdown()
        {
            foreach (var guide in _guideDict)
            {
                guide.Value.Release();
            }

            _guideDict.Clear();

            for (int i = 0; i < _guideTriggerList.Count; i++)
            {
                _guideTriggerList[i].Release();
            }

            _guideTriggerList.Clear();

            for (int i = 0; i < _runTriggerForceGuideList.Count; i++)
            {
                _runTriggerForceGuideList[i].Release();
            }

            _runTriggerForceGuideList.Clear();
        }

        public void SetGuideCheckFinish(IGuideCheckFinish guideCheckFinish)
        {
            _guideCheckFinish = guideCheckFinish;
        }

        public void LoadForceGuideConfig(GuideConfigList config)
        {
            _forceGuideConfig = config;
        }

        public void LoadForceGuideSeqTriggerConfig(GuideSeqConfig seqConfig)
        {
            _forceGuideSeqTriggerConfig = seqConfig;
            _triggerConfigList.AddRange(seqConfig.triggerList);
        }

        public void LoadNoForceGuideConfig(GuideConfigList config)
        {
            _noForceGuideConfig = config;
        }

        public void LoadNoForceGuideSeqTriggerConfig(GuideSeqConfig seqConfig)
        {
            _noForceGuideSeqTriggerConfig = seqConfig;
            _triggerConfigList.AddRange(seqConfig.triggerList);
        }

        public void InitGuide()
        {
            _guideDict.Clear();

            if (_forceGuideConfig != null)
            {
                for (int i = 0; i < _forceGuideConfig.configList.Count; i++)
                {
                    Guide newGuide = new Guide(_forceGuideConfig.configList[i]);
                    _guideDict.Add(_forceGuideConfig.configList[i].id, newGuide);
                }
            }

            if (_noForceGuideConfig != null)
            {
                for (int i = 0; i < _noForceGuideConfig.configList.Count; i++)
                {
                    Guide newGuide = new Guide(_noForceGuideConfig.configList[i]);
                    _guideDict.Add(_noForceGuideConfig.configList[i].id, newGuide);
                }
            }
        }

        public void InitGuideTrigger()
        {
            _guideTriggerList.Clear();
            GuideTriggerFactory triggerFactory = new GuideTriggerFactory();

            for (int i = 0; i < _triggerConfigList.Count; i++)
            {
                var finished = false;

                if (_guideCheckFinish != null)
                {
                    finished = _guideCheckFinish.CheckGuideFinish(_triggerConfigList[i].guideId);
                }

                if (finished)
                {
                    continue;
                }

                var trigger = triggerFactory.CreateTriggerByType(_triggerConfigList[i]);
                if (trigger != null)
                {
                    _guideTriggerList.Add(trigger);
                }
            }

            _guideTriggerList = _guideTriggerList.OrderByDescending(gt =>
            {
                if (_guideDict.TryGetValue(gt.Config.guideId, out var guide))
                {
                    return guide.Priority;
                }

                return int.MinValue;
            }).ToList();
        }


        /// <summary>
        /// 触发弱引导
        /// </summary>
        private void TriggerNoForceGuide()
        {
            if (_runTriggerNoForceGuideList.Count <= 0)
                return;

            _runTriggerNoForceGuideList = _runTriggerNoForceGuideList.OrderByDescending(g => g.Priority).ThenBy(g => g.Id).ToList();

            //没运行启动一下
            if (!_runTriggerNoForceGuideList[0].IsRunning)
            {
                _runTriggerNoForceGuideList[0].NextStep();
            }

            //开始迭代
            _runTriggerNoForceGuideList[0].Update();

            //清理多余的数据
            for (int i = 1; i < _runTriggerNoForceGuideList.Count; i++)
            {
                _runTriggerNoForceGuideList[i].Reset();
            }

            //引导完成
            if (_runTriggerNoForceGuideList[0].IsFinish)
            {
                _runTriggerNoForceGuideList[0].Release();
                _runTriggerNoForceGuideList.RemoveAt(0);
            }
        }

        /// <summary>
        /// 触发强制引导
        /// </summary>
        private void TriggerForceGuide()
        {
            if (_runTriggerForceGuideList.Count <= 0)
                return;

            //没运行启动一下
            if (!_runTriggerForceGuideList[0].IsRunning)
            {
                _runTriggerForceGuideList[0].NextStep();
            }

            //开始迭代
            _runTriggerForceGuideList[0].Update();

            //清理多余的数据
            for (int i = 1; i < _runTriggerForceGuideList.Count; i++)
            {
                _runTriggerForceGuideList[i].Reset();
            }

            //引导完成
            if (_runTriggerForceGuideList[0].IsFinish)
            {
                _runTriggerForceGuideList[0].Release();
                _runTriggerForceGuideList.RemoveAt(0);
            }
        }
    }
}