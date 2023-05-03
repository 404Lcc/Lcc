namespace LccHotfix
{
    [Model]
    public class LoginModel : ModelTemplate
    {
        public bool isEnterMain;
        public override void Init()
        {
            base.Init();
        }

        public void OnEnterMain()
        {
            isEnterMain = true;
        }
    }
}