using System;
using cfg;

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
        
        public static int GetItemFrameIcon(QualityType qualityType)
        {
            switch (qualityType)
            {
                case QualityType.White:
                    return 0;
                case QualityType.Green:
                    return 0;
                case QualityType.Blue:
                    return 0;
                case QualityType.Purple:
                    return 0;
                case QualityType.Yellow:
                    return 0;
                case QualityType.Red:
                    return 0;
                case QualityType.Platinum:
                    return 0;
            }

            return 0;
        }
    }
}