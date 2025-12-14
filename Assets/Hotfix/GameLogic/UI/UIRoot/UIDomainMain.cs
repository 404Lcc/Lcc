namespace LccHotfix
{
    public class UIDomainMain : UIDomainBase
    {
        public override bool OnRequireEscape(ElementNode child)
        {
            return true;
        }
    }
}