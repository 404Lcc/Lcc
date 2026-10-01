namespace LccHotfix
{
    public static class LocalizationExtension
    {
        public static string Localize(this string key)
            => LocalizationManager.Instance.GetString(key);

        public static string Localize<T>(this string key, T param)
            => LocalizationManager.Instance.GetString(key, param);

        public static string Localize<T1, T2>(this string key, T1 param1, T2 param2)
            => LocalizationManager.Instance.GetString(key, param1, param2);

        public static string Localize<T1, T2, T3>(this string key, T1 param1, T2 param2, T3 param3)
            => LocalizationManager.Instance.GetString(key, param1, param2, param3);

        public static string Localize<T1, T2, T3, T4>(this string key, T1 param1, T2 param2, T3 param3, T4 param4)
            => LocalizationManager.Instance.GetString(key, param1, param2, param3, param4);

        public static string Localize<T1, T2, T3, T4, T5>(this string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
            => LocalizationManager.Instance.GetString(key, param1, param2, param3, param4, param5);

        public static string Localize<T1, T2, T3, T4, T5, T6>(this string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
            => LocalizationManager.Instance.GetString(key, param1, param2, param3, param4, param5, param6);

        public static string Localize<T1, T2, T3, T4, T5, T6, T7>(this string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5,
            T6 param6, T7 param7)
            => LocalizationManager.Instance.GetString(key, param1, param2, param3, param4, param5, param6, param7);

        public static string Localize<T1, T2, T3, T4, T5, T6, T7, T8>(this string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5,
            T6 param6, T7 param7, T8 param8)
            => LocalizationManager.Instance.GetString(key, param1, param2, param3, param4, param5, param6, param7, param8);

        public static string Localize(this string key, params object[] param)
            => LocalizationManager.Instance.GetString(key, param);

        public static string DisplayName(this Language language) => $"language_name_{language.ToString().ToLower()}".Localize();

        public static uint ToMask(this Language language) => (uint)(1 << (int)(language - 1));
    }
}
