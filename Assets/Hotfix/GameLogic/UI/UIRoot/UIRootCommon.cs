namespace LccHotfix
{
    public class UIRootCommon : UIDomainBase
    {
        public override bool OnEscape(ref EscapeType escapeType)
        {
            escapeType = EscapeType.Skip;
            return false;
        }

        public override bool OnRequireEscape(ElementNode child)
        {
            return true;
        }
    }
}