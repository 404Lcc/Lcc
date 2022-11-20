using LccModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
    public class SceneStateName
    {
        public const string None = "";
        public const string Login = "Login";
        public const string Main = "Main";
    }
    public class SceneStateManager : Singleton<SceneStateManager>, IUpdate
    {
        private bool _forceStop;
        private SceneState _current;
        private SceneState _last;

        private Dictionary<string, SceneState> _sceneStateDict = new Dictionary<string, SceneState>();

        public void InitManager()
        {
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
                        LogUtil.LogError("�����¼�ʧ�� " + statePipelineAttribute.sceneName + "������");
                    }
                }
            }

            if (_sceneStateDict.ContainsKey(SceneStateName.Login))
            {
                _current = _sceneStateDict[SceneStateName.Login];
                _current.OnEnter();
            }
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


        public override void Update()
        {
            base.Update();

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