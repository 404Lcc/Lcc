using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
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
                if (_pipelineList[i].CheckState())
                {
                    return _pipelineList[i].target;
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