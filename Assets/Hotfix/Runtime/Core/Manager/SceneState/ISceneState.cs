using ET;

namespace LccHotfix
{
    public interface ISceneState
    {
        public ETTask OnEnter();
        public ETTask OnExit();
        public void Tick();
    }
}