namespace LccHotfix
{
    public class SceneStateAttribute : AttributeBase
    {
        public SceneStateType sceneType;
        public SceneStateAttribute(SceneStateType sceneType)
        {
            this.sceneType = sceneType;
        }
    }
}