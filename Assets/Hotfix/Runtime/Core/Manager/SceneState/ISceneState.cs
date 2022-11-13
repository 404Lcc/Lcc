namespace LccHotfix
{
    public interface ISceneState
    {
        public void OnEnter();
        public void Tick();
        public void OnExit();
    }
}