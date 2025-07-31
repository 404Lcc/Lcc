namespace LccHotfix
{
    internal class FrameworkMain : Main
    {
        public override void OnInstall()
        {
            base.OnInstall();
            
            Current = this;
        }
    }
}