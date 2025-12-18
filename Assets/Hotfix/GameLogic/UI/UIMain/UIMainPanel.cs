namespace LccHotfix
{
    public class UIMainPanel : UIElementBase
    {
        public override void OnConstruct()
        {
            base.OnConstruct();
            
            LayerID = UILayerID.Main;
            IsFullScreen = true;
            EscapeType = EscapeType.Hide;
        }
    }
}