namespace LccHotfix
{
    public class UIMainPanel : UIElementBase
    {
        public override void OnInit()
        {
            base.OnInit();
            
            var e = Node as ElementNode;
            e.IsFullScreen = true;
            e.EscapeType = EscapeType.Hide;
        }
    }
}