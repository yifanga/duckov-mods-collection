using System.Collections.Generic;
using System.Linq;
using SodaCraft.Localizations;
using UnityEngine;

namespace DuckovBetterRealDog
{

    public static class LocalizationUtil
    {
        // 当前系统语言
        public static SystemLanguage CurrentLanguage => LocalizationManager.CurrentLanguage;

        // 获取"地上散落物"的翻译（优化为更简洁的标题）
        public static string PetSearchToggleKeySetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "宠物拾取盒子开关键（长按）",
                ChineseTraditional = "寵物拾取盒子開關鍵（長按）",
                Japanese = "ペット収集ボックス切替（長押）",
                English = "Pet loot toggle (long press)",
                Korean = "펫 수집 토글 (길게)",
                French = "Basculer butin animal (appui long)",
                German = "Haustier-Beuteumschalter (lang)",
                Russian = "Перекл. ящика питомца (долго)",
                Spanish = "Alternar botín mascota (larga)"
            }
        );

        public static string PetDropBoxKeySetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "宠物卸货快捷键(短按原地，长按散开)",
                ChineseTraditional = "寵物卸貨快捷鍵(短按原地，長按散開)",
                Japanese = "ペット荷下ろしホットキー（短: その場, 長: 分散）",
                English = "Pet unload key (short: spot, long: spread)",
                Korean = "펫 하역 키 (짧: 제자리, 길: 분산)",
                French = "Clé décharger animal (court: place, long: étaler)",
                German = "Haustier-Entladen (kurz: Ort, lang: verteilen)",
                Russian = "Клавиша разгрузки (кор: место, дл: разброс)",
                Spanish = "Tecla descarga mascota (corto: lugar, largo: dispersar)"
            }
        );

        public static string ToggleNormalPatternSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "宠物恢复正常盒子堆叠模式",
                ChineseTraditional = "寵物恢復正常盒子堆疊模式",
                Japanese = "ペット通常ボックス積みモード回復",
                English = "Restore normal box stacking for pet",
                Korean = "펫 정상 박스 적재 모드 복원",
                French = "Rétablir l'empilement normal des boîtes pour animal",
                German = "Normaler Kistenstapelmodus für Haustier wiederherstellen",
                Russian = "Восстановить обычный режим стопки ящиков для питомца",
                Spanish = "Restaurar modo apilamiento normal de cajas para mascota"
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


        public static SortedDictionary<string, string> BuildChineseKeyMappingDictionary()
        {
            var keyMapping = new SortedDictionary<string, string>();

            // 添加鼠标键的翻译
            // keyMapping.Add("左键", "Mouse0");
            // keyMapping.Add("右键", "Mouse1");
            keyMapping.Add("中键", "Mouse2");
            keyMapping.Add("侧键1", "Mouse3");
            keyMapping.Add("侧键2", "Mouse4");
            // keyMapping.Add("侧键3", "Mouse5");
            // keyMapping.Add("侧键4", "Mouse6");

            // 添加特殊键的翻译
            keyMapping.Add("空格", "Space");
            // keyMapping.Add("制表键", "Tab");
            keyMapping.Add("回车", "Return");
            // keyMapping.Add("退出", "Escape");
            // keyMapping.Add("退格", "Backspace");
            // keyMapping.Add("删除", "Delete");

            // keyMapping.Add("上箭头", "UpArrow");
            // keyMapping.Add("下箭头", "DownArrow");
            // keyMapping.Add("左箭头", "LeftArrow");
            // keyMapping.Add("右箭头", "RightArrow");

            keyMapping.Add("左Shift", "LeftShift");
            keyMapping.Add("右Shift", "RightShift");
            keyMapping.Add("左Ctrl", "LeftControl");
            keyMapping.Add("右Ctrl", "RightControl");
            keyMapping.Add("左Alt", "LeftAlt");
            keyMapping.Add("右Alt", "RightAlt");

            keyMapping.Add("大写锁定", "CapsLock");
            // keyMapping.Add("上翻页", "PageUp");
            // keyMapping.Add("下翻页", "PageDown");
            // keyMapping.Add("Home", "Home");
            // keyMapping.Add("End", "End");

            // 添加字母键 (A-Z)
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (c == 'A' || c == 'W' || c == 'S' || c == 'D')
                {
                    continue;
                }
                keyMapping.Add(c.ToString(), c.ToString());
            }

            // 添加数字键 (0-9)
            // for (char c = '0'; c <= '9'; c++)
            // {
            //     keyMapping.Add(c.ToString(), c.ToString());
            // }

            // 添加功能键 (F1-F12)
            // for (int i = 1; i <= 12; i++)
            // {
            //     keyMapping.Add($"F{i}", $"F{i}");
            // }

            return keyMapping;
        }

        public static SortedDictionary<string, string> BuildEnglishKeyMappingDictionary()
        {
            var keyMapping = new SortedDictionary<string, string>();

            // Add mouse button translations
            keyMapping.Add("Middle Button", "Mouse2");
            keyMapping.Add("Side Button 1", "Mouse3");
            keyMapping.Add("Side Button 2", "Mouse4");

            // Add special keys translations
            keyMapping.Add("Space", "Space");
            keyMapping.Add("Enter", "Return");
            keyMapping.Add("Backspace", "Backspace");
            keyMapping.Add("Delete", "Delete");

            keyMapping.Add("Left Shift", "LeftShift");
            keyMapping.Add("Right Shift", "RightShift");
            keyMapping.Add("Left Ctrl", "LeftControl");
            keyMapping.Add("Right Ctrl", "RightControl");
            keyMapping.Add("Left Alt", "LeftAlt");
            keyMapping.Add("Right Alt", "RightAlt");

            keyMapping.Add("Caps Lock", "CapsLock");

            // Add letter keys (A-Z)
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (c == 'A' || c == 'W' || c == 'S' || c == 'D')
                {
                    continue;
                }
                keyMapping.Add(c.ToString(), c.ToString());
            }

            // Add number keys (0-9)
            // for (char c = '0'; c <= '9'; c++)
            // {
            //     keyMapping.Add(c.ToString(), c.ToString());
            // }

            // Add function keys (F1-F12)
            // for (int i = 1; i <= 12; i++)
            // {
            //     keyMapping.Add($"F{i}", $"F{i}");
            // }

            return keyMapping;
        }

        public static SortedDictionary<string, object> GetKeyMappingDictionary()
        {
            if (UseChinese())
            {
                return ConvertToObjectDictionary(BuildChineseKeyMappingDictionary());
            }
            return ConvertToObjectDictionary(BuildEnglishKeyMappingDictionary());
        }

        public static SortedDictionary<string, object> ConvertToObjectDictionary<TValue>(SortedDictionary<string, TValue> sourceDict)
        {
            var objectDict = new SortedDictionary<string, object>();

            foreach (var kvp in sourceDict)
            {
                objectDict.Add(kvp.Key, kvp.Value);
            }

            return objectDict;
        }

        private static bool UseChinese()
        {
            // 根据当前语言设置描述文字
            SystemLanguage[] chineseLanguages = {
                SystemLanguage.Chinese,
                SystemLanguage.ChineseSimplified,
                SystemLanguage.ChineseTraditional
            };

            return chineseLanguages.Contains(LocalizationManager.CurrentLanguage);
        }

    }

}