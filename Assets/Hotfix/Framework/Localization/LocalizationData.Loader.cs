using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace LccHotfix
{
    public partial class LocalizationData
    {
#if UNITY_EDITOR
        private const string LanguageNamePath = "Assets/Content/Config/Local/language.json";
        private const string LocalizationStringConfigRootPathFormat = "Assets/Content/Config/Localization/{0}/Strings";

        private bool InitDataInEditor()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return false;

            var languageNameTextAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(LanguageNamePath);
            if (languageNameTextAsset != null)
            {
                AddRawStringByJson(languageNameTextAsset.bytes, LanguageNamePath);
            }

            var stringConfigPath = string.Format(LocalizationStringConfigRootPathFormat, _language);
            if (!UnityEditor.AssetDatabase.IsValidFolder(stringConfigPath))
                return true;

            var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset", new[] { stringConfigPath });
            foreach (var guid in guids)
            {
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(assetPath);
                if (textAsset != null)
                {
                    AddRawStringByJson(textAsset.bytes, assetPath);
                }
            }

            return true;
        }
#endif

        public void Load(Language language, bool reload = false)
        {
            if (!reload && (_language == language || language == Language.unknown))
                return;

            _language = language;

            Unload();
            InitData();
        }

        private void InitData()
        {
#if UNITY_EDITOR
            if (InitDataInEditor())
                return;
#endif
            var assetService = Main.AssetService;
            if (assetService == null)
            {
                KLogger.LogError("[Localization] InitData Failed: AssetService is null");
                return;
            }

            var languageNameRawFile = assetService.LoadAssetRawFileSync(LocalizationConfig.AssetLanguageNameLocation);
            if (languageNameRawFile != null)
            {
                AddRawStringByJson(languageNameRawFile.GetRawFileData(), LocalizationConfig.AssetLanguageNameLocation);
                languageNameRawFile.Release();
            }

            var package = assetService.RawFilePackage;
            if (package == null)
            {
                KLogger.LogError("[Localization] InitData Failed: RawFilePackage is null");
                return;
            }

            var infos = package.GetAssetInfos(KString.Format(LocalizationConfig.AssetTagFormatStrings, _language));
            for (int i = 0; i < infos.Length; ++i)
            {
                var handle = package.LoadRawFileSync(infos[i]);
                if (handle == null)
                    continue;
                AddRawStringByJson(handle.GetRawFileData(), infos[i].AssetPath);
                handle.Release();
            }
        }

        public void Unload()
        {
            _dictionary.Clear();
        }

        private static JsonTextReader CreateJsonReader(TextReader reader)
        {
            return new JsonTextReader(reader)
            {
                DateParseHandling = DateParseHandling.None,
                FloatParseHandling = FloatParseHandling.Decimal,
            };
        }

        private void AddRawStringByJson(byte[] jsonBytes, string source = null)
        {
            if (jsonBytes == null || jsonBytes.Length == 0)
            {
                LogAddRawStringByJsonFailed("empty json", source);
                return;
            }

            using var stream = new MemoryStream(jsonBytes, writable: false);
            using var textReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            using var jsonReader = CreateJsonReader(textReader);
            AddRawStringByJson(jsonReader, source);
        }

        private void AddRawStringByJson(JsonTextReader jsonReader, string source)
        {
            try
            {
                if (!jsonReader.Read() || jsonReader.TokenType != JsonToken.StartObject)
                {
                    LogAddRawStringByJsonFailed("invalid json format", source);
                    return;
                }

                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.EndObject)
                    {
                        return;
                    }

                    if (jsonReader.TokenType != JsonToken.PropertyName)
                    {
                        LogAddRawStringByJsonFailed("invalid json format", source);
                        return;
                    }

                    var key = jsonReader.Value as string;
                    if (!jsonReader.Read())
                    {
                        LogAddRawStringByJsonFailed("invalid json format", source);
                        return;
                    }

                    if (jsonReader.TokenType != JsonToken.String && jsonReader.TokenType != JsonToken.Null)
                    {
                        LogAddRawStringByJsonFailed($"invalid value token: {jsonReader.TokenType}", source);
                        return;
                    }

                    var value = jsonReader.TokenType == JsonToken.String ? (jsonReader.Value as string) : string.Empty;
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    if (!_dictionary.TryAdd(key, value))
                    {
                        LogAddRawStringByJsonFailed($"duplicated key: {key}", source);
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                LogAddRawStringByJsonFailed($"json parse exception: {e.Message}", source);
            }
        }

        private static void LogAddRawStringByJsonFailed(string reason, string source)
        {
            KLogger.LogError(string.IsNullOrEmpty(source)
                ? $"[Localization] AddRawStringByJson Failed: {reason}"
                : $"[Localization] AddRawStringByJson Failed: {reason}, source: {source}");
        }
    }
}
