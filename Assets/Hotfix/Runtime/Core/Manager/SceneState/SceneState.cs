using ET;
using System.Collections.Generic;

namespace LccHotfix
{
    public abstract class SceneState : ISceneState
    {
        public SceneStateType sceneType;
        private List<StatePipeline> _pipelineList = new List<StatePipeline>();

        public virtual async ETTask OnEnter()
        {
        }

        public virtual async ETTask OnExit()
        {
        }

        public virtual void Tick()
        {
        }

        public SceneStateType CheckTarget()
        {
            for (int i = 0; i < _pipelineList.Count; i++)
            {
                if (_pipelineList[i].CheckState())
                {
                    return _pipelineList[i].target;
                }
            }
            return SceneStateType.None;
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