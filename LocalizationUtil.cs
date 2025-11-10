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

        public static string TempTrashCanText => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "临时垃圾堆",
                ChineseTraditional = "臨時垃圾堆",
                Japanese = "一時的なゴミ箱",
                English = "Temporary Trash Can",
                Korean = "임시 쓰레기통",
                French = "Poubelle temporaire",
                German = "Temporärer Mülleimer",
                Russian = "Временная корзина",
                Spanish = "Contenedor temporal"
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
                ChineseSimplified = "一键搜索快捷键",
                ChineseTraditional = "一鍵搜索快捷鍵",
                Japanese = "ワンクリック検索ホットキー",
                English = "Search Hotkey",
                Korean = "검색 단축키",
                French = "Racc. recherche",
                German = "Such-Hotkey",
                Russian = "Клавиша поиска",
                Spanish = "Tecla búsqueda"
            }
        );

        public static string SearchTimeKeepSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "是否保留搜索时间",
                ChineseTraditional = "是否保留搜索時間",
                Japanese = "搜索時間を保持しますか",
                English = "Keep search time?",
                Korean = "검색 시간을 유지합니까?",
                French = "Conserver le temps de recherche?",
                German = "Suchzeit behalten?",
                Russian = "Сохранять время поиска?",
                Spanish = "¿Mantener tiempo de búsqueda?"
            }
        );

        public static string SearchContainersSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "启用搜索敌人战利品盒子",
                ChineseTraditional = "啟用搜索敵人戰利品盒子",
                Japanese = "敵戦利品検索有効化",
                English = "Search Enemy Loot",
                Korean = "적 전리품 검색 활성화",
                French = "Rechercher butin",
                German = "Feindbeute suchen",
                Russian = "Поиск вражеского лута",
                Spanish = "Buscar botín enemigo"
            }
        );
        public static string SearchContainersRadiusSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "敌人盒子搜索半径(建议10m)",
                ChineseTraditional = "敵人盒子搜索半徑(建議10m)",
                Japanese = "敵戦利品検索半径(推奨10m)",
                English = "Loot Search Radius",
                Korean = "전리품 검색 반경(권장10m)",
                French = "Rayon butin",
                German = "Beutesuchradius",
                Russian = "Радиус поиска лута",
                Spanish = "Radio búsqueda botín"
            }
        );
        public static string SearchPickupRadiusSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "地面散落物搜索半径(建议0.3m,官方值)",
                ChineseTraditional = "地面散落物搜索半徑(建議0.3m,官方值)",
                Japanese = "地面アイテム検索半径(推奨0.3m)",
                English = "Item Search Radius",
                Korean = "아이템 검색 반경(권장0.3m)",
                French = "Rayon objets sol",
                German = "Objektsuchradius",
                Russian = "Радиус поиска предметов",
                Spanish = "Radio búsqueda objetos"
            }
        );

        public static string AutoUnplugSlotsSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "启用自动拆卸插槽(全局生效)",
                ChineseTraditional = "啟用自動拆卸插槽(全域生效)",
                Japanese = "自動スロット外し(グローバル)",
                English = "Auto Unplug Slots (Global)",
                Korean = "자동 슬롯 분리 (전역)",
                French = "Débranchement auto (Global)",
                German = "Auto-Ausstecken (Global)",
                Russian = "Автоизвлечение слотов (глоб.)",
                Spanish = "Extracción auto (Global)"
            }
        );

        public static string SearchOtherContainersSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "启用搜索除敌人盒子外容器",
                ChineseTraditional = "啟用搜索除敵人盒子外容器",
                Japanese = "非敵コンテナ検索",
                English = "Search non-enemy containers",
                Korean = "비적 상자 검색",
                French = "Recherche conteneurs non-ennemis",
                German = "Suche nicht-feindliche Behälter",
                Russian = "Поиск не вражеских контейнеров",
                Spanish = "Buscar contenedores no enemigos"
            }
        );
        public static string SearchOtherContainersRadiusSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "除敌人盒子外容器搜索半径(建议0.3m,官方值)",
                ChineseTraditional = "除敵人盒子外容器搜索半徑(建議0.3m,官方值)",
                Japanese = "非敵コンテナ半径(推奨0.3m)",
                English = "Non-enemy container radius (rec. 0.3m)",
                Korean = "비적 상자 반경 (권장 0.3m)",
                French = "Rayon conteneurs non-ennemis (rec. 0.3m)",
                German = "Radius nicht-feindliche Behälter (emp. 0.3m)",
                Russian = "Радиус не вражеских контейнеров (рец. 0.3м)",
                Spanish = "Radio contenedores no enemigos (rec. 0.3m)"
            }
        );

        public static string IgnoreOtherContainersRequiredSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = "启用搜索带条件的除敌人盒子外容器(钥匙/铲子等)",
                ChineseTraditional = "搜索帶條件的盒子外容器(鑰匙/鏟子)",
                Japanese = "条件付きコンテナ検索(鍵/シャベル等)",
                English = "Search containers with requirements (key/shovel etc.)",
                Korean = "조건부 컨테이너 검색 (열쇠/삽 등)",
                French = "Recherche conteneurs conditionnels (clé/pelle etc.)",
                German = "Suche Behälter mit Anforderungen (Schlüssel/Schaufel etc.)",
                Russian = "Поиск контейнеров с условиями (ключ/лопата и др.)",
                Spanish = "Buscar contenedores con requisitos (llave/pala etc.)"
            }
        );

        public static string GenerateTempTrashCanSetting => GetTranslation(
            CurrentLanguage,
            new TranslationSet
            {
                ChineseSimplified = $"是否在脚下生成临时垃圾堆盒子(实验性,大于{DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD}个触发)",
                ChineseTraditional = $"是否在腳下生成臨時垃圾堆盒子(實驗性,大於{DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD}個觸發)",
                Japanese = $"足元に一時ゴミ箱生成? (実験的, {DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD}個以上)",
                English = $"Generate temp trash can? (Exp, >{DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD} items)",
                Korean = $"임시 쓰레기통 생성? (실험, {DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD}개 이상)",
                French = $"Générer poubelle temp? (Exp, >{DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD} objets)",
                German = $"Temp-Mülleimer generieren? (Exp, >{DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD} Gegenstände)",
                Russian = $"Создать временную корзину? (Эксп, >{DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD} предметов)",
                Spanish = $"¿Generar contenedor temp? (Exp, >{DynamicLootBoxManager.GENERATOR_TEMP_TRASH_CAN_THRESHOLD} objetos)"
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