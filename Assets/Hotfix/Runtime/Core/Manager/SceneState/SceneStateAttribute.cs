using System;

namespace LccHotfix
{
    public class StatePipelineAttribute : BaseAttribute
    {
        public string sceneName;

        public string target;

        public StatePipelineAttribute(string sceneName, string target)
        {
            this.sceneName = sceneName;
            this.target = target;

        }
    }
    public class SceneStateAttribute : BaseAttribute
    {
        public string sceneName;
        public SceneStateAttribute(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }
}