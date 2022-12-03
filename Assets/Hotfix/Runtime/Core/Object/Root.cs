namespace LccHotfix
{
    public class Root : Singleton<Root>
    {
        public Scene Scene;
        public Root()
        {
            Scene = AObjectBase.Create<Scene>();
        }
        protected override void Dispose()
        {
            Scene.Dispose();
        }
    }
}