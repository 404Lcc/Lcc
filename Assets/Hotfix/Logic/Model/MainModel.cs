namespace LccHotfix
{
    [Model]
    public class MainModel : ModelTemplate
    {
        public override void Init()
        {
            base.Init();
        }

        public void EnterGame()
        {
            SceneStateManager.Instance.NextState(SceneStateType.Game);
        }
    }
}