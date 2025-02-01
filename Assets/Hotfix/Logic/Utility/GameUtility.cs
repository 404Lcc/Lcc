using System;

namespace LccHotfix
{
    public static class GameUtility
    {
        public static void FireNow(object sender, GameEventArgs e)
        {
            Entry.GetModule<EventManager>().FireNow(sender, e);
        }
        public static void Subscribe(GameEventType type, EventHandler<GameEventArgs> handler)
        {
            Entry.GetModule<EventManager>().Subscribe((int)type, handler);
        }

        public static void Unsubscribe(GameEventType type, EventHandler<GameEventArgs> handler)
        {
            Entry.GetModule<EventManager>().Unsubscribe((int)type, handler);
        }

        public static string GetLanguageText(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                Log.Error("key不能等于空");
                return "";
            }
            return LanguageManager.Instance.GetValue(key, args);
        }
    }
}