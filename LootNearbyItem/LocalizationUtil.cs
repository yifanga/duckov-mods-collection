using System;
using System.Collections.Generic;
using System.Linq;
using SodaCraft.Localizations;
using UnityEngine;

namespace LootNearbyItem
{
    public static class LocalizationUtil
    {
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

        public static SystemLanguage CurrentLanguage => LocalizationManager.CurrentLanguage;

        public static string ScatteredObjectsText => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string TempTrashCanText => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string NoScatteredObjectsText => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string ItemOutOfRangeText => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string HotKeySetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string SearchHotKeySetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string SearchTimeKeepSetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string SearchContainersSetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string SearchContainersRadiusSetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string SearchPickupRadiusSetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string AutoUnplugSlotsSetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string SearchOtherContainersSetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string SearchOtherContainersRadiusSetting => GetTranslation(CurrentLanguage, new TranslationSet
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
        });

        public static string GenerateTempTrashCanSetting => GetTranslation(CurrentLanguage, new TranslationSet
        {
            ChineseSimplified = "是否在脚下生成临时垃圾堆盒子(物品大于指定个数触发)",
            ChineseTraditional = "是否在腳下生成臨時垃圾堆盒子(物品大於指定個數觸發)",
            Japanese = "足元に一時ゴミ箱を生成しますか?(アイテムが指定数以上で発動)",
            English = "Generate temp trash can at feet? (Triggers when items > threshold)",
            Korean = "발 아래에 임시 쓰레기통 생성? (아이템이 임계값 초과 시 발동)",
            French = "Générer poubelle temp aux pieds? (Déclenche si objets > seuil)",
            German = "Temp-Mülleimer am Fuß generieren? (Löst aus bei Gegenständen > Schwellenwert)",
            Russian = "Создать временную корзину у ног? (Срабатывает при предметах > порога)",
            Spanish = "¿Generar contenedor temp a los pies? (Se activa si objetos > umbral)"
        });

        public static string GenerateTempTrashCanThresholdSetting => GetTranslation(CurrentLanguage, new TranslationSet
        {
            ChineseSimplified = "生成垃圾堆的物品个数阈值(0-999,默认10)",
            ChineseTraditional = "生成垃圾堆的物品個數閾值(0-999,預設10)",
            Japanese = "ゴミ箱生成アイテム数閾値(0-999,デフォルト10)",
            English = "Trash can item threshold (0-999, default 10)",
            Korean = "쓰레기통 아이템 임계값 (0-999, 기본값 10)",
            French = "Seuil d'objets pour poubelle (0-999, défaut 10)",
            German = "Schwellenwert für Mülleimer (0-999, Standard 10)",
            Russian = "Порог предметов для корзины (0-999, по умолчанию 10)",
            Spanish = "Umbral de objetos para contenedor (0-999, predeterminado 10)"
        });

        public static string SearchHiddenContainersSetting => GetTranslation(CurrentLanguage, new TranslationSet
        {
            ChineseSimplified = "启用搜索土堆/藏匿点容器",
            ChineseTraditional = "啟用搜索土堆/藏匿點容器",
            Japanese = "土堆/隠し場所の検索を有効化",
            English = "Search hidden containers (dirt piles)",
            Korean = "숨겨진 용기(흙더미) 검색 활성화",
            French = "Activer la recherche de conteneurs cachés (tas de terre)",
            German = "Suche nach versteckten Behältern (Erdhaufen) aktivieren",
            Russian = "Включить поиск скрытых контейнеров (кучи земли)",
            Spanish = "Activar búsqueda de contenedores ocultos (montones de tierra)"
        });

        public static string SearchHiddenContainersRadiusSetting => GetTranslation(CurrentLanguage, new TranslationSet
        {
            ChineseSimplified = "土堆/藏匿点搜索半径",
            ChineseTraditional = "土堆/藏匿點搜索半徑",
            Japanese = "土堆/隠し場所の検索範囲",
            English = "Hidden containers search radius",
            Korean = "숨겨진 용기 검색 반경",
            French = "Rayon de recherche des conteneurs cachés",
            German = "Suchradius für versteckte Behälter",
            Russian = "Радиус поиска скрытых контейнеров",
            Spanish = "Radio de búsqueda de contenedores ocultos"
        });

        private static string GetTranslation(SystemLanguage language, TranslationSet translations)
        {
            switch (language)
            {
                case SystemLanguage.ChineseSimplified: return translations.ChineseSimplified;
                case SystemLanguage.ChineseTraditional: return translations.ChineseTraditional;
                case SystemLanguage.Japanese: return translations.Japanese;
                case SystemLanguage.Korean: return translations.Korean;
                case SystemLanguage.French: return translations.French;
                case SystemLanguage.German: return translations.German;
                case SystemLanguage.Russian: return translations.Russian;
                case SystemLanguage.Spanish: return translations.Spanish;
                default: return GetFallbackTranslation(translations);
            }
        }

        private static string GetFallbackTranslation(TranslationSet translations)
        {
            if (!string.IsNullOrEmpty(translations.English)) return translations.English;
            if (!string.IsNullOrEmpty(translations.ChineseSimplified)) return translations.ChineseSimplified;
            if (!string.IsNullOrEmpty(translations.Japanese)) return translations.Japanese;
            if (!string.IsNullOrEmpty(translations.Korean)) return translations.Korean;
            if (!string.IsNullOrEmpty(translations.French)) return translations.French;
            if (!string.IsNullOrEmpty(translations.German)) return translations.German;
            if (!string.IsNullOrEmpty(translations.Russian)) return translations.Russian;
            if (!string.IsNullOrEmpty(translations.Spanish)) return translations.Spanish;
            if (!string.IsNullOrEmpty(translations.ChineseTraditional)) return translations.ChineseTraditional;
            return "TRANSLATION MISSING";
        }

        public static SortedDictionary<string, string> BuildChineseKeyMappingDictionary()
        {
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            dict.Add("中键", "Mouse2");
            dict.Add("侧键1", "Mouse3");
            dict.Add("侧键2", "Mouse4");
            dict.Add("空格", "Space");
            dict.Add("回车", "Return");
            dict.Add("左Shift", "LeftShift");
            dict.Add("右Shift", "RightShift");
            dict.Add("左Ctrl", "LeftControl");
            dict.Add("右Ctrl", "RightControl");
            dict.Add("左Alt", "LeftAlt");
            dict.Add("右Alt", "RightAlt");
            dict.Add("大写锁定", "CapsLock");
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (c != 'A' && c != 'W' && c != 'S' && c != 'D')
                    dict.Add(c.ToString(), c.ToString());
            }
            return dict;
        }

        public static SortedDictionary<string, string> BuildEnglishKeyMappingDictionary()
        {
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            dict.Add("Middle Button", "Mouse2");
            dict.Add("Side Button 1", "Mouse3");
            dict.Add("Side Button 2", "Mouse4");
            dict.Add("Space", "Space");
            dict.Add("Enter", "Return");
            dict.Add("Backspace", "Backspace");
            dict.Add("Delete", "Delete");
            dict.Add("Left Shift", "LeftShift");
            dict.Add("Right Shift", "RightShift");
            dict.Add("Left Ctrl", "LeftControl");
            dict.Add("Right Ctrl", "RightControl");
            dict.Add("Left Alt", "LeftAlt");
            dict.Add("Right Alt", "RightAlt");
            dict.Add("Caps Lock", "CapsLock");
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (c != 'A' && c != 'W' && c != 'S' && c != 'D')
                    dict.Add(c.ToString(), c.ToString());
            }
            return dict;
        }

        public static SortedDictionary<string, string> GetKeyMappingDictionary()
        {
            if (UseChinese()) return BuildChineseKeyMappingDictionary();
            return BuildEnglishKeyMappingDictionary();
        }

        private static bool UseChinese()
        {
            SystemLanguage lang = LocalizationManager.CurrentLanguage;
            return lang == SystemLanguage.ChineseSimplified || lang == SystemLanguage.ChineseTraditional;
        }
    }
}
