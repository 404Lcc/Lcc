namespace LccHotfix
{
    public class UIRootMain : UIDomainBase
    {
        public override bool OnRequireEscape(ElementNode child)
        {
            return true;
        }
    }
}