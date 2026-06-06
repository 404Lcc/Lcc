namespace LccHotfix
{
    public partial class Init
    {
        public static void Start()
        {
            Log.SetLogHelper(new DefaultLogHelper());
            Main.SetMain(new GameMain());
        }
    }
}