namespace LccHotfix
{
    public class SceneStateAttribute : AttributeBase
    {
        public SceneType sceneType;
        public SceneStateAttribute(SceneType sceneType)
        {
            this.sceneType = sceneType;
        }
    }
}