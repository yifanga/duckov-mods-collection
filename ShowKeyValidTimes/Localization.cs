using System.Collections.Generic;
using SodaCraft.Localizations;
using UnityEngine;

namespace ShowKeyValidTimes
{
    public class MyLocalization
    {
        public const string ValidTimeStats = "可用:";


        private static readonly Dictionary<UnityEngine.SystemLanguage, Dictionary<string, string>> Translations = new Dictionary<UnityEngine.SystemLanguage, Dictionary<string, string>>()
        {
            {
                UnityEngine.SystemLanguage.ChineseSimplified, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "可用:" }
                }
            },
            {
                UnityEngine.SystemLanguage.ChineseTraditional, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "可用:" }
                }
            },
            {
                UnityEngine.SystemLanguage.English, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "Use:" }
                }
            },
            {
                UnityEngine.SystemLanguage.Japanese, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "使用可:" }
                }
            },
            {
                UnityEngine.SystemLanguage.Korean, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "사용가능:" }
                }
            },
            {
                UnityEngine.SystemLanguage.French, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "Dispo:" }
                }
            },
            {
                UnityEngine.SystemLanguage.Russian, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "Использовать:" }
                }
            },
            {
                UnityEngine.SystemLanguage.German, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "Verfügbar:" }
                }
            },
            {
                UnityEngine.SystemLanguage.Spanish, new Dictionary<string, string>()
                {
                    { ValidTimeStats, "Usos:" }
                }
            }
        }; 

        public static string GetTranslation(string key)
        {
            var defaultLang = UnityEngine.SystemLanguage.English;
            var currentLanguage = LocalizationManager.CurrentLanguage;
            if (Translations.TryGetValue(currentLanguage, out var langDict))
            {
                if (langDict.TryGetValue(key, out var translation))
                {
                    return translation;
                }
            }
            return Translations[defaultLang][key];
        }
    }
}