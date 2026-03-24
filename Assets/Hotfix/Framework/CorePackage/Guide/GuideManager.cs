using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public interface IGuideCheckFinish
    {
        bool CheckGuideFinish(int guideId);
    }

    public interface IGuidePersistence
    {
        void Save(int guideId);
    }
    
    public interface IGuideMessage
    {
        void GuideStart(int id);
        void GuideEnd(int id, bool isException);
    }

    public class GuideManager : Module, IGuideService
    {
        //原生配置数据
        private GuideConfigList _forceGuideConfigList;
        private GuideConfigList _noForceGuideConfigList;
        private GuideTriggerConfigList _forceGuideTriggerConfigList;
        private GuideTriggerConfigList _noForceGuideTriggerConfigList;
        private List<GuideTriggerConfig> _triggerConfigList = new List<GuideTriggerConfig>();

        //根据GuideTrigger配置生成的触发列表
        private List<GuideTriggerCondBase> _guideTriggerList = new List<GuideTriggerCondBase>();

        //根据配置生成引导字典 key=引导id
        private Dictionary<int, Guide> _guideDict = new Dictionary<int, Guide>();

        //运行时强制引导数据
        private List<Guide> _runTriggerForceGuideList = new List<Guide>();

        //运行时弱引导数据
        private List<Guide> _runTriggerNoForceGuideList = new List<Guide>();

        //是否有强制引导
        private bool HasForceGuide => _runTriggerForceGuideList.Count > 0;
        private bool HasNoForceGuide => _runTriggerNoForceGuideList.Count > 0;

        //检测引导是否完成
        private IGuideCheckFinish _guideCheckFinish;

        //引导持久化
        private IGuidePersistence _guidePersistence;
        
        //引导消息
        private IGuideMessage _guideMessage;

        private AssetLoader _loader;

        public GuideManager()
        {
            _loader = new AssetLoader();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            for (int i = 0; i < _guideTriggerList.Count; i++)
            {
                if (_guideTriggerList[i].CheckTrigger() && _runTriggerForceGuideList.Count <= 0)
                {
                    var guideId = _guideTriggerList[i].Config.guideId;
                    if (_guideDict.TryGetValue(guideId, out var guide))
                    {
                        switch (guide.Type)
                        {
                            case (int)GuideType.Force:
                                _runTriggerForceGuideList.Add(guide);
                                break;
                            case (int)GuideType.NoForce:
                                _runTriggerNoForceGuideList.Add(guide);
                                break;
                        }

                        _guideDict.Remove(guideId);
                    }
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
            ResetGuide();
            _loader.Release();
        }

        public void SetGuideCheckFinish(IGuideCheckFinish guideCheckFinish)
        {
            _guideCheckFinish = guideCheckFinish;
        }

        public void SetGuidePersistence(IGuidePersistence guidePersistence)
        {
            _guidePersistence = guidePersistence;
        }
        
        public void SetGuideMessage(IGuideMessage guideMessage)
        {
            _guideMessage = guideMessage;
        }

        public void LoadForceGuideConfigList(GuideConfigList config)
        {
            _forceGuideConfigList = config;
        }

        public void LoadForceGuideTriggerConfigList(GuideTriggerConfigList config)
        {
            _forceGuideTriggerConfigList = config;
            _triggerConfigList.AddRange(config.triggerList);
        }

        public void LoadNoForceGuideConfigList(GuideConfigList config)
        {
            _noForceGuideConfigList = config;
        }

        public void LoadNoForceGuideTriggerConfigList(GuideTriggerConfigList config)
        {
            _noForceGuideTriggerConfigList = config;
            _triggerConfigList.AddRange(config.triggerList);
        }

        /// <summary>
        /// 初始化引导配置 账号登录完成时候初始化
        /// </summary>
        public void InitGuide()
        {
            _guideDict.Clear();

            if (_forceGuideConfigList != null)
            {
                for (int i = 0; i < _forceGuideConfigList.configList.Count; i++)
                {
                    Guide newGuide = new Guide(_forceGuideConfigList.configList[i], _guideMessage);
                    _guideDict.Add(_forceGuideConfigList.configList[i].id, newGuide);
                }
            }

            if (_noForceGuideConfigList != null)
            {
                for (int i = 0; i < _noForceGuideConfigList.configList.Count; i++)
                {
                    Guide newGuide = new Guide(_noForceGuideConfigList.configList[i], _guideMessage);
                    _guideDict.Add(_noForceGuideConfigList.configList[i].id, newGuide);
                }
            }
        }

        /// <summary>
        /// 初始化引导触发器 账号登录完成时候初始化
        /// </summary>
        public void InitGuideTrigger()
        {
            _guideTriggerList.Clear();

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

                var trigger = GuideTriggerCondFactory.CreateCond(_triggerConfigList[i]);
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
        /// 重置引导数据 账号退出时需要重置
        /// </summary>
        public void ResetGuide()
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

            for (int i = 0; i < _runTriggerNoForceGuideList.Count; i++)
            {
                _runTriggerNoForceGuideList[i].Release();
            }

            _runTriggerNoForceGuideList.Clear();
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
                _runTriggerNoForceGuideList[i].Release();
            }

            //引导完成
            if (_runTriggerNoForceGuideList[0].IsFinish)
            {
                if (_guidePersistence != null)
                {
                    _guidePersistence.Save(_runTriggerNoForceGuideList[0].Id);
                }

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
                _runTriggerForceGuideList[i].Release();
            }

            //引导完成
            if (_runTriggerForceGuideList[0].IsFinish)
            {
                if (_guidePersistence != null)
                {
                    _guidePersistence.Save(_runTriggerForceGuideList[0].Id);
                }

                _runTriggerForceGuideList[0].Release();
                _runTriggerForceGuideList.RemoveAt(0);
            }
        }
    }
}