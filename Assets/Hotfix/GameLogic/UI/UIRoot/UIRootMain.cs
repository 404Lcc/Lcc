namespace LccHotfix
{
    public class UIRootMain : UIRootBase
    {

        public override bool OnChildRequireEscape(WNode child)
        {
            return true;
        }
    }
}