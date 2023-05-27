using ET;
using System.Collections.Generic;

namespace LccHotfix
{
    public abstract class SceneState : ISceneState
    {
        public SceneStateType sceneType;

        public virtual async ETTask OnEnter()
        {
        }

        public virtual async ETTask OnExit()
        {
        }

        public virtual void Tick()
        {
        }
    }
}