using System;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    internal class LocalizationManager : Module, ILocalizationService
    {
        private Language _language = Language.unknown;
        private readonly LocalizationData _localizationData = new();

        public Language Language
        {
            get => _language;
            set => SetLanguage(value);
        }

        public void Init()
        {
            LoadDefaultLanguage();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            _language = Language.unknown;
            _localizationData.Unload();
        }

        private void LoadDefaultLanguage()
        {
            var language = GetDefaultLanguage();
            _language = language;
            _localizationData.Load(language, true);
            if (KLogger.IsDev)
                KLogger.Log($"[Localization] Init as language {language}");
        }

        private Language GetDefaultLanguage()
        {
            var setLanguage = LoadLanguageSettings();
            if (setLanguage != Language.unknown)
            {
                return setLanguage;
            }

            var systemLanguage = Application.systemLanguage;
            if (LocalizationConfig.LanguageDict.TryGetValue(systemLanguage, out var language))
            {
                return language;
            }

            Enum.TryParse(AppConfig.AppLanguage, out language);
            return language;
        }

        public void SetLanguage(Language language)
        {
            if (_language == language)
                return;

            _language = language;
            _localizationData.Load(language);
            SaveLanguageSettings();
            if (KLogger.IsDev)
                KLogger.Log($"[Localization] Set language {language}");

            LocalizationEvent.LanguageChange.Broadcast(language);
        }

        public void SaveLanguageSettings()
        {
            FileHelper.SaveString(AppConfig.LocalizationSettingsNamePath, _language.ToString());
        }

        public Language LoadLanguageSettings()
        {
            var languageName = FileHelper.LoadString(AppConfig.LocalizationSettingsNamePath);
            if (Enum.TryParse(languageName, out Language language))
            {
                return language;
            }
            return Language.unknown;
        }

        #region GetString

        public string GetString(string key)
        {
            var value = GetRawString(key);
            if (value == null)
            {
                return key;
            }
            return value;
        }

        public string GetString<T>(string key, T param)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param);
        }

        public string GetString<T1, T2>(string key, T1 param1, T2 param2)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param1, param2);
        }

        public string GetString<T1, T2, T3>(string key, T1 param1, T2 param2, T3 param3)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param1, param2, param3);
        }

        public string GetString<T1, T2, T3, T4>(string key, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param1, param2, param3, param4);
        }

        public string GetString<T1, T2, T3, T4, T5>(string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param1, param2, param3, param4, param5);
        }

        public string GetString<T1, T2, T3, T4, T5, T6>(string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param1, param2, param3, param4, param5, param6);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7>(string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param1, param2, param3, param4, param5, param6, param7);
        }

        public string GetString<T1, T2, T3, T4, T5, T6, T7, T8>(string key, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param1, param2, param3, param4, param5, param6, param7, param8);
        }

        public string GetString(string key, params object[] param)
        {
            var value = GetRawString(key);
            if (value == null)
                return key;
            return KString.Format(value, param);
        }

        #endregion

        public string GetRawString(string key)
        {
            return _localizationData.GetRawString(key);
        }

        public bool HasRawString(string key) => _localizationData.HasRawString(key);
    }
}