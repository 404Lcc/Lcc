using System.Collections.Generic;

namespace LccHotfix
{
    public partial class LocalizationData
    {
        private Language _language = Language.unknown;

        private readonly Dictionary<string, string> _dictionary = new(5000);

        public string GetRawString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!_dictionary.TryGetValue(key, out var value))
            {
                return null;
            }

            return value;
        }

        public bool HasRawString(string key) => _dictionary.ContainsKey(key);

        public void AddRawString(Dictionary<string, string> pairs)
        {
            if (pairs == null)
                return;

            foreach (var pair in pairs)
            {
                AddRawString(pair.Key, pair.Value);
            }
        }

        public void AddRawString(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                KLogger.LogError("[Localization] AddRawString Failed: invalid key");
                return;
            }

            if (_dictionary.ContainsKey(key))
            {
                KLogger.LogError($"[Localization] AddRawString Failed: duplicated key: {key}");
                return;
            }

            _dictionary.Add(key, value ?? string.Empty);
        }

        public void RemoveRawString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            _dictionary.Remove(key);
        }
    }
}
