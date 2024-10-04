namespace LccHotfix
{
    public class UIRootMain : UIRootBase
    {
        public override bool OnChildClosed(WNode child)
        {
            return false;
        }

        public override bool OnChildRequireEscape(WNode child)
        {
            return true;
        }
    }
}