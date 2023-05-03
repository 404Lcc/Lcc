namespace LccHotfix
{
    [StatePipeline(SceneStateType.Main, SceneStateType.Login)]
    public class EnterLogin : StatePipeline
    {
        public override bool CheckState()
        {
            return false;
        }
    }
}