using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 本地化语言
    /// </summary>
    public enum Language : byte
    {
        unknown,
        /// <summary>
        /// 中文（简体）
        /// </summary>
        zh_cn,
        /// <summary>
        /// 中文（繁体）
        /// </summary>
        zh_tw,
        /// <summary>
        /// 英语
        /// </summary>
        en,
        /// <summary>
        /// 越南语
        /// </summary>
        vn,
    }

    public static class LocalizationConfig
    {
        public const string AssetLanguageNameLocation = "language";
        public const string AssetTagFormatStrings = "{0}_strings";

        public static readonly Dictionary<SystemLanguage, Language> LanguageDict = new Dictionary<SystemLanguage, Language>
        {
            { SystemLanguage.Chinese, Language.zh_cn },
            { SystemLanguage.ChineseSimplified, Language.zh_cn },
            { SystemLanguage.ChineseTraditional, Language.zh_tw },
            { SystemLanguage.English, Language.en },
            { SystemLanguage.Vietnamese, Language.vn },
        };
    }
}
