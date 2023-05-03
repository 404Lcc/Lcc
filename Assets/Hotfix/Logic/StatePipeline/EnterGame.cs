namespace LccHotfix
{
    [StatePipeline(SceneStateType.Main, SceneStateType.Game)]
    public class EnterGame : StatePipeline
    {
        public override bool CheckState()
        {
            return ModelManager.Instance.GetModel<MainModel>().isEnterGame;
        }
    }
}