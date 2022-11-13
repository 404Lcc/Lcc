using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateName.Login, SceneStateName.Main, "Test")]
    public class LoginSceneState : SceneState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("Login" + "����");
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Login" + "�˳�");
        }
        public bool Test()
        {
            return true;
        }
    }
    [SceneState(SceneStateName.Main, SceneStateName.Login, "Test")]
    public class MainSceneState : SceneState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("Main" + "����");
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Main" + "�˳�");
        }
        public bool Test()
        {
            return false;
        }
    }
    public abstract class SceneState : ISceneState
    {
        public string sceneName;
        private List<StatePipeline> _pipelineList = new List<StatePipeline>();


        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void Tick()
        {
        }

        public string CheckTarget()
        {
            for (int i = 0; i < _pipelineList.Count; i++)
            {
                if (_pipelineList[i].condition != null)
                {
                    if (_pipelineList[i].condition())
                    {
                        return _pipelineList[i].target;
                    }
                }
            }
            return SceneStateName.None;
        }
        public void AddPipeline(StatePipeline statePipeline)
        {
            if (_pipelineList.Contains(statePipeline))
            {
                return;
            }
            else
            {
                _pipelineList.Add(statePipeline);
            }
        }
        public void RemovePipeline(StatePipeline statePipeline)
        {
            if (_pipelineList.Contains(statePipeline))
            {
                _pipelineList.Remove(statePipeline);
            }
        }
    }
}