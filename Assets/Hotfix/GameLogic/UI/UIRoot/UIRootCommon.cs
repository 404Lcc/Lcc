namespace LccHotfix
{
    public class UIRootCommon : UIRootBase
    {
        public override bool OnChildClosed(WNode child)
        {
            return false;
        }
        public override bool OnEscape(ref EscapeType escapeType)
        {
            escapeType = EscapeType.SKIP_OVER;
            return false;
        }

        public override bool OnChildRequireEscape(WNode child)
        {
            return true;
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}