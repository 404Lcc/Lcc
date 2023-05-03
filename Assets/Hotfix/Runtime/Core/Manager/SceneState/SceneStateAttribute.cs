namespace LccHotfix
{
    public class StatePipelineAttribute : AttributeBase
    {
        public SceneStateType sceneType;

        public SceneStateType target;

        public StatePipelineAttribute(SceneStateType sceneType, SceneStateType target)
        {
            this.sceneType = sceneType;
            this.target = target;
        }
    }
    public class SceneStateAttribute : AttributeBase
    {
        public SceneStateType sceneType;
        public SceneStateAttribute(SceneStateType sceneType)
        {
            this.sceneType = sceneType;
        }
    }
}