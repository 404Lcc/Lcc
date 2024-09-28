namespace LccHotfix
{
    public class Root : Singleton<Root>
    {
        public Scene Scene;
        public Root()
        {
            Scene = SceneFactory.CreateScene("Scene");
        }
        protected override void Dispose()
        {
            Scene.Dispose();
        }
    }
}