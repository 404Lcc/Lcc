namespace LccHotfix
{
    [Model]
    public class LoginModel : ModelTemplate
    {
        public override void Init()
        {
            base.Init();
        }

        public void OnEnterMain()
        {
            SceneStateManager.Instance.ChangeScene(SceneStateType.Main);
        }
    }
}