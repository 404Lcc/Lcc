namespace LccHotfix
{
    public interface ISceneState
    {
        public void OnEnter(object[] args);
        public void OnExit();
        public void Tick();
    }
}