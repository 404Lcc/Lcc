namespace LccHotfix
{
    [Model]
    public class MainModel : ModelTemplate
    {
        public bool isEnterGame;
        public override void Init()
        {
            base.Init();
        }

        public void EnterGame()
        {
            isEnterGame = true;
        }
    }
}