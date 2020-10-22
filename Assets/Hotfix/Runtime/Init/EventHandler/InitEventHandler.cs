namespace LccHotfix
{
    public class InitEventHandler : AEvent<Start>
    {
        public override void Publish(Start data)
        {
            Manager.Instance.InitManager();
        }
    }
}