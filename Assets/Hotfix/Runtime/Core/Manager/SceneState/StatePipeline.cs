using UnityEngine;

namespace LccHotfix
{
    public abstract class StatePipeline
    {
        public SceneStateType sceneType;
        public SceneStateType target;

        public StatePipeline()
        {
        }
        public abstract bool CheckState();
    }
}