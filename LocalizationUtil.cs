using SodaCraft.Localizations;
using UnityEngine;

namespace LootNearbyItem
{



    public static class LocalizationUtil
    {
        // 当前系统语言
        public static SystemLanguage CurrentLanguage => LocalizationManager.CurrentLanguage;

        // 获取"地上散落物"的翻译（优化为更简洁的标题）
        public static string ScatteredObjectsText => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "地上散落物",
                ChineseTraditional = "地上散落物",
                Japanese = "地上の散乱物",
                English = "Ground Items",
                Korean = "지상 아이템",
                French = "Objets au sol",
                German = "Bodenobjekte",
                Russian = "Предметы",
                Spanish = "Objetos suelo"
            }
        );

        // 获取"不要找啦，附近没有散落物！"的翻译
        public static string NoScatteredObjectsText => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "别找啦，这附近没有可拾取的物品，歇会儿吧！",
                ChineseTraditional = "別找啦，這附近沒有可拾取的物品，歇會兒吧！",
                Japanese = "探すのやめよう、拾えるものは何もない、少し休憩しよう！",
                English = "Stop looking, nothing to pick up here - take a break!",
                Korean = "그만 찾아! 이 근처엔 주울 게 없어, 좀 쉬자!",
                French = "Cherche pas, y'a rien à ramasser ici - repose-toi un peu !",
                German = "Hör auf zu suchen, hier gibt's nichts zu finden - mach eine Pause!",
                Russian = "Хватит искать, тут нечего подобрать - отдохни немного!",
                Spanish = "¡Deja de buscar, no hay nada para recoger aquí - tómate un descanso!"
            }
        );

        // 翻译数据集结构
        private struct TranslationSet
        {
            public string ChineseSimplified;
            public string ChineseTraditional;
            public string Japanese;
            public string English;
            public string Korean;
            public string French;
            public string German;
            public string Russian;
            public string Spanish;
        }

        private static string GetTranslation(SystemLanguage language, TranslationSet translations)
        {
            return language switch
            {
                // 简体中文
                SystemLanguage.ChineseSimplified => translations.ChineseSimplified,

                // 繁体中文
                SystemLanguage.ChineseTraditional => translations.ChineseTraditional,

                // 日语
                SystemLanguage.Japanese => translations.Japanese,

                // 韩语
                SystemLanguage.Korean => translations.Korean,

                // 法语
                SystemLanguage.French => translations.French,

                // 德语
                SystemLanguage.German => translations.German,

                // 俄语
                SystemLanguage.Russian => translations.Russian,

                // 西班牙语
                SystemLanguage.Spanish => translations.Spanish,

                // 默认回退策略
                _ => GetFallbackTranslation(translations)
            };
        }

        // 处理缺失翻译的回退策略
        private static string GetFallbackTranslation(TranslationSet translations)
        {
            // 1. 优先尝试英语
            if (!string.IsNullOrEmpty(translations.English))
                return translations.English;

            // 2. 尝试简体中文
            if (!string.IsNullOrEmpty(translations.ChineseSimplified))
                return translations.ChineseSimplified;

            // 3. 尝试第一个非空翻译
            if (!string.IsNullOrEmpty(translations.Japanese)) return translations.Japanese;
            if (!string.IsNullOrEmpty(translations.Korean)) return translations.Korean;
            if (!string.IsNullOrEmpty(translations.French)) return translations.French;
            if (!string.IsNullOrEmpty(translations.German)) return translations.German;
            if (!string.IsNullOrEmpty(translations.Russian)) return translations.Russian;
            if (!string.IsNullOrEmpty(translations.Spanish)) return translations.Spanish;
            if (!string.IsNullOrEmpty(translations.ChineseTraditional)) return translations.ChineseTraditional;

            // 4. 最终回退
            return "TRANSLATION MISSING";
        }
    }

}