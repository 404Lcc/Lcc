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

        private Dictionary<SceneStateType, SceneState> _sceneStateDict = new Dictionary<SceneStateType, SceneState>();
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
                    sceneState.sceneType = sceneStateAttribute.sceneType;

                    _sceneStateDict.Add(sceneStateAttribute.sceneType, sceneState);
                }
            }

            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(StatePipelineAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(StatePipelineAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    StatePipelineAttribute statePipelineAttribute = (StatePipelineAttribute)atts[0];

                    StatePipeline sceneState = (StatePipeline)Activator.CreateInstance(item);
                    sceneState.sceneType = statePipelineAttribute.sceneType;
                    sceneState.target = statePipelineAttribute.target;

                    if (_sceneStateDict.ContainsKey(statePipelineAttribute.sceneType))
                    {
                        _sceneStateDict[statePipelineAttribute.sceneType].AddPipeline(sceneState);
                    }
                    else
                    {
                        LogUtil.Error("�����¼�ʧ�� " + statePipelineAttribute.sceneType + "������");
                    }
                }
            }
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;

            _forceStop = true;
            _sceneStateDict.Clear();
        }

        public void SetDefaultState(SceneStateType type)
        {
            if (_current != null)
            {
                throw new Exception("�Ѿ���״̬�������ˣ�");
            }
            if (_sceneStateDict.ContainsKey(type))
            {
                _current = _sceneStateDict[type];
                _current.OnEnter().Coroutine();
            }
        }

        public SceneState GetState(SceneStateType type)
        {
            if (!_sceneStateDict.ContainsKey(type)) return null;
            return _sceneStateDict[type];
        }

        public SceneState GetCurrentState()
        {
            return _current;
        }

        public void Update()
        {
            if (_current == null) return;
            if (_forceStop) return;

            SceneStateType result = _current.CheckTarget();

            if (result != SceneStateType.None)
            {
                SceneState target = _sceneStateDict[result];
                _last = _current;
                _current = target;

                _last.OnExit().Coroutine();
                _current.OnEnter().Coroutine();
            }
            else
            {
                _current.Tick();
            }
        }
    }
}