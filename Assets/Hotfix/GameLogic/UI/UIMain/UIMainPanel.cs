namespace LccHotfix
{
    public class UIMainPanel : UIElementBase
    {
        public override void OnConstruct()
        {
            base.OnConstruct();
            
            var e = Node as ElementNode;
            e.IsFullScreen = true;
            e.EscapeType = EscapeType.Hide;
        }
    }
}