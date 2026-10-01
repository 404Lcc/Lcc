using LccModel;

namespace LccHotfix
{
    public static class LocalizationEvent
    {
        public class LanguageChange : IEventMessage
        {
            public Language Language;

            public static void Broadcast(Language language)
            {
                Event.SendMessage(new LanguageChange
                {
                    Language = language
                });
            }
        }
    }
}
