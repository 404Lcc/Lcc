namespace LccHotfix
{
    [StatePipeline(SceneStateName.Main, SceneStateName.Login)]
    public class EnterLogin : StatePipeline
    {
        public EnterLogin(string sceneName, string target) : base(sceneName, target)
        {
        }

        public override bool CheckState()
        {
            return false;
        }
    }
}