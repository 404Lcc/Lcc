namespace LccHotfix
{
    [StatePipeline(SceneStateType.Login, SceneStateType.Main)]
    public class EnterMain : StatePipeline
    {
        public override bool CheckState()
        {
            return ModelManager.Instance.GetModel<LoginModel>().isEnterMain;
        }
    }
}