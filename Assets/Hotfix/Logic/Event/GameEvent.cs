namespace LccHotfix
{
    public class Test : GameEventArgs
    {
        public override int Id => (int)GameEventType.Test;

        public override void Clear()
        {
        }
    }
}