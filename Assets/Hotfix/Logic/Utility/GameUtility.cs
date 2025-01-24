namespace LccHotfix
{
    public static class GameUtility
    {
        public static void FireNow(object sender, GameEventArgs e)
        {
            Entry.GetModule<EventManager>().FireNow(sender, e);
        }
    }
}