using System.Collections.Generic;
using System.Linq;
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

        public static string ItemOutOfRangeText => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "距离可拾取物品有点远，请再靠近些吧~",
                ChineseTraditional = "距離可拾取物品有點遠，請再靠近些吧~",
                Japanese = "拾えるアイテムが少し遠いです、もう少し近づいてください~",
                English = "A bit far from the item - move a little closer, please~",
                Korean = "아이템이 조금 멀리 있어요, 가까이 다가가 주세요~",
                French = "Un peu loin de l'objet - approchez-vous, s'il vous plaît~",
                German = "Etwas zu weit vom Gegenstand - kommen Sie bitte etwas näher~",
                Russian = "Немного далековато от предмета - подойдите ближе, пожалуйста~",
                Spanish = "Un poco lejos del objeto - acércate un poco más, por favor~"
            }
        );
        public static string HotKeySetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "快捷键设置",
                ChineseTraditional = "快捷鍵設置",
                Japanese = "ホットキー設定",
                English = "Hotkey Settings",
                Korean = "단축키 설정",
                French = "Paramètres de raccourci",
                German = "Tastenkombinationseinstellungen",
                Russian = "Настройки горячих клавиш",
                Spanish = "Configuración de teclas rápidas"
            }
        );

        public static string SearchHotKeySetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "一键搜索快捷键设置",
                ChineseTraditional = "一鍵搜索快捷鍵設置",
                Japanese = "ワンクリック検索ホットキー設定",
                English = "One-click Search Hotkey Settings",
                Korean = "원클릭 검색 단축키 설정",
                French = "Paramètres de raccourci de recherche en un clic",
                German = "Ein-Klick-Such-Hotkey-Einstellungen",
                Russian = "Настройки горячих клавиш для поиска в один клик",
                Spanish = "Configuración de tecla rápida de búsqueda con un clic"
            }
        );
        public static string SearchContainersSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "是否启用搜索击杀掉落的战利品盒子",
                ChineseTraditional = "是否啟用搜索擊殺掉落的戰利品盒子",
                Japanese = "撃破でドロップした戦利品ボックスの検索を有効にするか",
                English = "Enable search for loot boxes from kills",
                Korean = "처치 시 드롭된 전리품 상자 검색 활성화 여부",
                French = "Activer la recherche des coffres de butin des ennemis vaincus",
                German = "Suche nach Beutekisten von besiegten Gegnern aktivieren",
                Russian = "Включить поиск ящиков с лутом после убийств",
                Spanish = "Habilitar búsqueda de contenedores de botín de enemigos eliminados"
            }
        );
        public static string SearchContainersRadiusSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "搜索击杀掉落战利品盒子的半径（单位m）",
                ChineseTraditional = "搜索擊殺掉落戰利品盒子的半徑（單位m）",
                Japanese = "撃破でドロップした戦利品ボックスの検索半径（単位m）",
                English = "Search radius for loot boxes from kills (in meters)",
                Korean = "처치 시 드롭된 전리품 상자 검색 반경 (단위: m)",
                French = "Rayon de recherche pour les coffres de butin (en mètres)",
                German = "Suchradius für Beutekisten (in Metern)",
                Russian = "Радиус поиска ящиков с лутом (в метрах)",
                Spanish = "Radio de búsqueda para contenedores de botín (en metros)"
            }
        );
                
        public static string SearchPickupRadiusSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "搜索地上物品的半径（官方默认0.3m，不建议修改，影响平衡）",
                ChineseTraditional = "搜索地上物品的半徑（官方預設0.3m，不建議修改，影響平衡）",
                Japanese = "地面アイテムの検索半径（公式デフォルト0.3m、変更非推奨、バランスに影響）",
                English = "Ground item search radius (default 0.3m, not recommended to change, affects balance)",
                Korean = "지면 아이템 검색 반경 (기본값 0.3m, 변경 권장하지 않음, 밸런스 영향)",
                French = "Rayon de recherche des objets au sol (0.3m par défaut, déconseillé de modifier, affecte l'équilibre)",
                German = "Suchradius für Bodenobjekte (Standard 0.3m, Änderung nicht empfohlen, beeinflusst Balance)",
                Russian = "Радиус поиска предметов на земле (по умолчанию 0.3м, изменение не рекомендуется, влияет на баланс)",
                Spanish = "Radio de búsqueda de objetos en el suelo (predeterminado 0.3m, no se recomienda cambiar, afecta el equilibrio)"
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
                if(c == 'A' ||c == 'W' || c == 'S' || c == 'D')
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

        public static SortedDictionary<string, string> GetKeyMappingDictionary()
        {
            if (UseChinese())
            {
                return BuildChineseKeyMappingDictionary();
            }
            return BuildEnglishKeyMappingDictionary();
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