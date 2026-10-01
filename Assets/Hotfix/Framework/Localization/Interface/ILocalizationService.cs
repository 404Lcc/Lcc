namespace LccHotfix
{
    public interface ILocalizationService : IService
    {
        void Init();

        Language Language { get; set; }

        void SetLanguage(Language language);

        void SaveLanguageSettings();

        Language LoadLanguageSettings();

        string GetString(string key);

        string GetString<T>(string key, T param);

        string GetString<T1, T2>(string key, T1 param1, T2 param2);

        string GetString<T1, T2, T3>(string key, T1 param1, T2 param2, T3 param3);

        string GetString<T1, T2, T3, T4>(string key, T1 param1, T2 param2, T3 param3, T4 param4);

        string GetString<T1, T2, T3, T4, T5>(string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);

        string GetString<T1, T2, T3, T4, T5, T6>(string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6);

        string GetString<T1, T2, T3, T4, T5, T6, T7>(string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7);

        string GetString<T1, T2, T3, T4, T5, T6, T7, T8>(string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8);

        string GetString(string key, params object[] param);

        string GetRawString(string key);

        bool HasRawString(string key);
    }
}