namespace LccHotfix
{
    public class GuideFinishState : GuideState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            _data.IsFsmFinish = true;
        }
    }
}