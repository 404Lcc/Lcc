using LccModel;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class SceneStateManager : AObjectBase, IUpdate
    {
        public static SceneStateManager Instance { get; set; }

        private bool _forceStop;
        private SceneState _current;
        private SceneState _last;

        private Dictionary<string, SceneState> _sceneStateDict = new Dictionary<string, SceneState>();
        public override void Awake()
        {
            base.Awake();


            Instance = this;

            _forceStop = false;
            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(SceneStateAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(SceneStateAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    SceneStateAttribute sceneStateAttribute = (SceneStateAttribute)atts[0];

                    SceneState sceneState = (SceneState)Activator.CreateInstance(item);
                    sceneState.sceneName = sceneStateAttribute.sceneName;

                    _sceneStateDict.Add(sceneStateAttribute.sceneName, sceneState);
                }
            }

            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(StatePipelineAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(StatePipelineAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    StatePipelineAttribute statePipelineAttribute = (StatePipelineAttribute)atts[0];

                    StatePipeline sceneState = (StatePipeline)Activator.CreateInstance(item, statePipelineAttribute.sceneName, statePipelineAttribute.target);

                    if (_sceneStateDict.ContainsKey(statePipelineAttribute.sceneName))
                    {
                        _sceneStateDict[statePipelineAttribute.sceneName].AddPipeline(sceneState);
                    }
                    else
                    {
                        LogUtil.LogError("增加事件失败 " + statePipelineAttribute.sceneName + "不存在");
                    }
                }
            }

            if (_sceneStateDict.ContainsKey(SceneStateName.Login))
            {
                _current = _sceneStateDict[SceneStateName.Login];
                _current.OnEnter();
            }

        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;

            _forceStop = true;
            _sceneStateDict.Clear();
        }



        public SceneState GetState(string name)
        {
            if (!_sceneStateDict.ContainsKey(name)) return null;
            return _sceneStateDict[name];
        }

        public SceneState GetCurrentState()
        {
            return _current;
        }


        public void Update()
        {
            if (_current == null) return;
            if (_forceStop) return;

            string result = _current.CheckTarget();

            if (result != SceneStateName.None)
            {
                SceneState target = _sceneStateDict[result];
                _last = _current;
                _current = target;



                _last.OnExit();
                _current.OnEnter();
            }
            else
            {
                _current.Tick();
            }
        }



    }
}