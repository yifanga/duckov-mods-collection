using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Duckov.Modding;
using UnityEngine;

namespace LootNearbyItem
{
    public static class ModConfigManager
    {
        [Serializable]
        public class ConfigData
        {
            public string searchKey = "H";
            public bool searchTimeKeep = false;
            public bool searchContainers = false;
            public float searchContainersRadius = 10f;
            public float searchPickupRadius = 0.3f;
            public bool autoUnplugSlots = false;
            public bool searchOtherContainers = false;
            public float searchOtherContainersRadius = 0.3f;
            public bool generatorTempTrashCan = false;
            public int generatorTempTrashCanThreshold = 10;
            public bool searchHiddenContainers = false;
            public float searchHiddenContainersRadius = 3f;

            [NonSerialized]
            public KeyCode searchKeyCode = KeyCode.H;
        }

        private const string MOD_NAME = "LootNearbyItem";
        private const string DEFAULT_SEARCH_KEY = "H";

        private static string ModFolderPath = Path.Combine(ModManager.DefaultModFolderPath, MOD_NAME);
        private static string ConfigFilePath = Path.Combine(ModManager.DefaultModFolderPath, MOD_NAME, "config.txt");

        private static readonly string[] ValidKeys = new string[]
        {
            "Mouse0", "Mouse1", "Mouse2", "Mouse3", "Mouse4", "Mouse5", "Mouse6",
            "Space", "Tab", "Return", "Escape", "Backspace", "Delete",
            "UpArrow", "DownArrow", "LeftArrow", "RightArrow",
            "LeftShift", "RightShift", "LeftControl", "RightControl",
            "LeftAlt", "RightAlt", "CapsLock", "PageUp", "PageDown", "Home", "End"
        };

        private static ConfigData _current = CreateDefaultConfig();

        private static ConfigData CreateDefaultConfig()
        {
            return new ConfigData
            {
                searchKey = "H",
                searchTimeKeep = false,
                searchContainers = false,
                searchContainersRadius = 10f,
                searchPickupRadius = 0.3f,
                searchKeyCode = KeyCode.H,
                autoUnplugSlots = false,
                searchOtherContainers = false,
                searchOtherContainersRadius = 0.3f,
                generatorTempTrashCan = false,
                generatorTempTrashCanThreshold = 10,
                searchHiddenContainers = false,
                searchHiddenContainersRadius = 3f
            };
        }

        public static void Init(string modRootFolder)
        {
            _current = CreateDefaultConfig();
            ModFolderPath = Path.Combine(modRootFolder, MOD_NAME);
            ConfigFilePath = Path.Combine(ModFolderPath, "config.txt");
            try
            {
                if (!Directory.Exists(ModFolderPath))
                    Directory.CreateDirectory(ModFolderPath);
                LoadOrCreateConfig();
            }
            catch (Exception ex)
            {
                Debug.LogError("ModConfig initialization failed: " + ex.Message);
            }
        }

        private static void LoadOrCreateConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    json = RemoveLineComments(json);
                    Debug.Log($"[LootNearbyItem] 配置文件内容: {json}");
                    ConfigData config = UnityEngine.JsonUtility.FromJson<ConfigData>(json);
                    if (config == null)
                    {
                        Debug.LogWarning("[LootNearbyItem] 配置解析返回null，使用默认配置（不覆盖用户文件）");
                        return;
                    }
                    if (ValidateConfig(config))
                    {
                        if (Enum.TryParse<KeyCode>(config.searchKey, true, out KeyCode result))
                            config.searchKeyCode = result;
                        else
                            Debug.LogWarning($"[LootNearbyItem] 快捷键 '{config.searchKey}' 无法解析为KeyCode，使用默认H");
                        Debug.Log($"[LootNearbyItem] 配置加载成功: key={config.searchKey}, pickupRadius={config.searchPickupRadius}, searchContainers={config.searchContainers}");
                        _current = config;
                        return;
                    }
                    else
                    {
                        Debug.LogWarning("[LootNearbyItem] 配置验证失败，使用默认配置（不覆盖用户文件）");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[LootNearbyItem] 加载配置失败: " + ex.Message + ". 使用默认配置（不覆盖用户文件）");
                    return;
                }
            }
            // 文件不存在时才创建默认配置
            SaveFile(ConfigFilePath, _current);
        }

        public static void SaveConfig()
        {
            if (_current == null) return;
            try
            {
                if (!Directory.Exists(ModFolderPath))
                    Directory.CreateDirectory(ModFolderPath);
                SaveFile(ConfigFilePath, _current);
            }
            catch (Exception ex)
            {
                Debug.LogError("ModConfig save config failed: " + ex.Message);
            }
        }

        private static bool ValidateConfig(ConfigData tmpConfig)
        {
            if (tmpConfig == null)
            {
                Debug.LogWarning("[LootNearbyItem] 配置对象为null");
                return false;
            }
            if (string.IsNullOrEmpty(tmpConfig.searchKey))
            {
                Debug.LogWarning("[LootNearbyItem] searchKey为空");
                return false;
            }
            if (!IsValidKey(tmpConfig.searchKey))
            {
                Debug.LogWarning("[LootNearbyItem] 无效的快捷键: " + tmpConfig.searchKey);
                return false;
            }
            return true;
        }

        private static bool IsValidKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            if (key.Length == 1 && char.IsLetterOrDigit(key[0])) return true;
            if ((key.Length == 2 || key.Length == 3) && key[0] == 'F' && int.TryParse(key.Substring(1), out int fNum))
                return fNum >= 1 && fNum <= 12;
            return Array.Exists(ValidKeys, k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        private static void SaveFile(string configFilePath, ConfigData configData)
        {
            if (configData == null) return;
            try
            {
                string header = "// Loot Nearby Item Mod Configuration\n" +
                    "// ---------------------------------\n" +
                    "// searchKey: 搜索快捷键 (字母A-Z, 数字0-9, F1-F12, Mouse0-6, Space, Tab, Return, Escape等)\n" +
                    "// searchTimeKeep: true/false - 是否保留搜索时间\n" +
                    "// searchContainers: true/false - 是否搜索附近战利品容器\n" +
                    "// searchContainersRadius: 搜索战利品容器半径(0.3-999)\n" +
                    "// searchPickupRadius: 搜索地面物品半径(0.3-999)\n" +
                    "// autoUnplugSlots: true/false - 自动拆出配件槽物品\n" +
                    "// searchOtherContainers: true/false - 搜索非击杀掉落容器\n" +
                    "// searchOtherContainersRadius: 非击杀掉落容器半径(0.3-999)\n" +
                    "// generatorTempTrashCan: true/false - 生成临时垃圾堆\n" +
                    "// generatorTempTrashCanThreshold: 垃圾堆触发阈值(0-999)\n" +
                    "// searchHiddenContainers: true/false - 搜索土堆/藏匿点容器\n" +
                    "// searchHiddenContainersRadius: 土堆/藏匿点搜索半径(0.3-999)\n\n";
                File.WriteAllText(configFilePath, header + UnityEngine.JsonUtility.ToJson(configData, true));
                Debug.Log("Mod configuration saved");
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to save config: " + ex.Message);
            }
        }

        private static string RemoveLineComments(string json)
        {
            // 统一换行符，处理Windows \r\n
            json = json.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] lines = json.Split('\n');
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                string trimmed = line.TrimStart();
                if (!trimmed.StartsWith("//"))
                    sb.AppendLine(line.TrimEnd());
            }
            return sb.ToString().Trim();
        }

        public static KeyCode GetSearchKeyCode()
        {
            if (_current == null) throw new InvalidOperationException("ModConfig not initialized.");
            return _current.searchKeyCode;
        }

        public static void SetSearchKeyCode(string keyCodeStr)
        {
            if (_current == null) throw new InvalidOperationException("ModConfig not initialized.");
            if (!IsValidKey(keyCodeStr) || !Enum.TryParse<KeyCode>(keyCodeStr, true, out KeyCode result))
            {
                Debug.LogWarning("Invalid key: " + keyCodeStr + ". Keep key unchanged: " + _current.searchKey);
            }
            else
            {
                _current.searchKey = keyCodeStr;
                _current.searchKeyCode = result;
            }
        }

        public static bool GetSearchTimeKeep() => _current?.searchTimeKeep ?? false;
        public static void SetSearchTimeKeep(bool enable) { if (_current != null) _current.searchTimeKeep = enable; }

        public static bool GetAutoUnplugSlots() => _current?.autoUnplugSlots ?? false;
        public static void SetAutoUnplugSlots(bool enable) { if (_current != null) _current.autoUnplugSlots = enable; }

        public static float GetSearchContainersRadius()
        {
            if (_current == null) return 10f;
            float v = Mathf.Clamp(_current.searchContainersRadius, 0.3f, 999f);
            _current.searchContainersRadius = v;
            return v;
        }
        public static void SetSearchContainersRadius(float radius) { if (_current != null) _current.searchContainersRadius = Mathf.Clamp(radius, 0.3f, 999f); }

        public static float GetSearchPickupRadius()
        {
            if (_current == null) return 0.3f;
            float v = Mathf.Clamp(_current.searchPickupRadius, 0.3f, 999f);
            _current.searchPickupRadius = v;
            return v;
        }
        public static void SetSearchPickupRadius(float radius) { if (_current != null) _current.searchPickupRadius = Mathf.Clamp(radius, 0.3f, 999f); }

        public static bool GetSearchContainers() => _current?.searchContainers ?? false;
        public static void SetSearchContainers(bool enable) { if (_current != null) _current.searchContainers = enable; }

        public static bool GetSearchOtherContainers() => _current?.searchOtherContainers ?? false;
        public static void SetSearchOtherContainers(bool enable) { if (_current != null) _current.searchOtherContainers = enable; }

        public static float GetSearchOtherContainersRadius()
        {
            if (_current == null) return 0.3f;
            float v = Mathf.Clamp(_current.searchOtherContainersRadius, 0.3f, 999f);
            _current.searchOtherContainersRadius = v;
            return v;
        }
        public static void SetSearchOtherContainersRadius(float radius) { if (_current != null) _current.searchOtherContainersRadius = Mathf.Clamp(radius, 0.3f, 999f); }

        public static void SetGeneratorTempTrashCan(bool enable) { if (_current != null) _current.generatorTempTrashCan = enable; }
        public static bool GetGeneratorTempTrashCan() => _current?.generatorTempTrashCan ?? false;

        public static void SetGeneratorTempTrashCanThreshold(int threshold) { if (_current != null) _current.generatorTempTrashCanThreshold = Mathf.Clamp(threshold, 0, 999); }
        public static int GetGeneratorTempTrashCanThreshold()
        {
            if (_current == null) return 10;
            int v = Mathf.Clamp(_current.generatorTempTrashCanThreshold, 0, 999);
            _current.generatorTempTrashCanThreshold = v;
            return v;
        }

        public static bool GetSearchHiddenContainers() => _current?.searchHiddenContainers ?? false;
        public static void SetSearchHiddenContainers(bool enable) { if (_current != null) _current.searchHiddenContainers = enable; }
        public static float GetSearchHiddenContainersRadius() => _current?.searchHiddenContainersRadius ?? 3f;
        public static void SetSearchHiddenContainersRadius(float radius) { if (_current != null) _current.searchHiddenContainersRadius = Mathf.Clamp(radius, 0.3f, 999f); }

        public static void OnModConfigMenuActivated(Duckov.Modding.ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                SetupModConfig();
                // 不再调用LoadConfigFromModConfig()，本地config.txt优先
            }
        }

        public static void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                Debug.LogWarning("ModConfig not available");
                return;
            }
            Debug.Log("准备添加ModConfig配置项");
            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);

            ModConfigAPI.SafeAddInputWithSlider(MOD_NAME, "SearchPickupRadiusSetting",
                LocalizationUtil.SearchPickupRadiusSetting, typeof(float), GetSearchPickupRadius(), new Vector2(0.3f, 999f));
            ModConfigAPI.SafeAddInputWithSlider(MOD_NAME, "GenerateTempTrashCanThresholdSetting",
                LocalizationUtil.GenerateTempTrashCanThresholdSetting, typeof(int), GetGeneratorTempTrashCanThreshold(), new Vector2(0f, 999f));
            ModConfigAPI.SafeAddBoolDropdownList(MOD_NAME, "GenerateTempTrashCanSetting",
                LocalizationUtil.GenerateTempTrashCanSetting, GetGeneratorTempTrashCan());
            ModConfigAPI.SafeAddInputWithSlider(MOD_NAME, "SearchOtherContainersRadiusSetting",
                LocalizationUtil.SearchOtherContainersRadiusSetting, typeof(float), GetSearchOtherContainersRadius(), new Vector2(0.3f, 999f));
            ModConfigAPI.SafeAddBoolDropdownList(MOD_NAME, "SearchOtherContainersSetting",
                LocalizationUtil.SearchOtherContainersSetting, GetSearchOtherContainers());
            ModConfigAPI.SafeAddInputWithSlider(MOD_NAME, "SearchContainersRadiusSetting",
                LocalizationUtil.SearchContainersRadiusSetting, typeof(float), GetSearchContainersRadius(), new Vector2(0.3f, 999f));
            ModConfigAPI.SafeAddBoolDropdownList(MOD_NAME, "SearchContainersSetting",
                LocalizationUtil.SearchContainersSetting, GetSearchContainers());
            ModConfigAPI.SafeAddBoolDropdownList(MOD_NAME, "AutoUnplugSlots",
                LocalizationUtil.AutoUnplugSlotsSetting, GetAutoUnplugSlots());
            ModConfigAPI.SafeAddBoolDropdownList(MOD_NAME, "SearchTimeKeepSetting",
                LocalizationUtil.SearchTimeKeepSetting, GetSearchTimeKeep());
            ModConfigAPI.SafeAddBoolDropdownList(MOD_NAME, "SearchHiddenContainersSetting",
                LocalizationUtil.SearchHiddenContainersSetting, GetSearchHiddenContainers());

            SortedDictionary<string, object> options = ConvertToObjectDictionary(LocalizationUtil.GetKeyMappingDictionary());
            ModConfigAPI.SafeAddDropdownList(MOD_NAME, "SearchHotKeySetting",
                LocalizationUtil.SearchHotKeySetting, options, typeof(string), GetSearchKeyCode().ToString());

            Debug.Log("ModConfig setup completed");
        }

        public static void OnModConfigOptionsChanged(string key)
        {
            if (key.StartsWith(MOD_NAME + "_"))
            {
                LoadConfigFromModConfig();
                SaveConfig();
                Debug.Log("ModConfig updated - " + key);
            }
        }

        public static void LoadConfigFromModConfig()
        {
            SetSearchKeyCode(ModConfigAPI.SafeLoad(MOD_NAME, "SearchHotKeySetting", "H"));
            SetSearchTimeKeep(ModConfigAPI.SafeLoad(MOD_NAME, "SearchTimeKeepSetting", false));
            SetAutoUnplugSlots(ModConfigAPI.SafeLoad(MOD_NAME, "AutoUnplugSlots", false));
            SetSearchContainers(ModConfigAPI.SafeLoad(MOD_NAME, "SearchContainersSetting", false));
            SetSearchContainersRadius(ModConfigAPI.SafeLoad(MOD_NAME, "SearchContainersRadiusSetting", 10f));
            SetSearchPickupRadius(ModConfigAPI.SafeLoad(MOD_NAME, "SearchPickupRadiusSetting", 0.3f));
            SetSearchOtherContainers(ModConfigAPI.SafeLoad(MOD_NAME, "SearchOtherContainersSetting", false));
            SetSearchOtherContainersRadius(ModConfigAPI.SafeLoad(MOD_NAME, "SearchOtherContainersRadiusSetting", 0.3f));
            SetGeneratorTempTrashCan(ModConfigAPI.SafeLoad(MOD_NAME, "GenerateTempTrashCanSetting", false));
            SetGeneratorTempTrashCanThreshold(ModConfigAPI.SafeLoad(MOD_NAME, "GenerateTempTrashCanThresholdSetting", 10));
            SetSearchHiddenContainers(ModConfigAPI.SafeLoad(MOD_NAME, "SearchHiddenContainersSetting", false));
        }

        public static SortedDictionary<string, object> ConvertToObjectDictionary<TValue>(SortedDictionary<string, TValue> sourceDict)
        {
            SortedDictionary<string, object> result = new SortedDictionary<string, object>();
            foreach (KeyValuePair<string, TValue> item in sourceDict)
                result.Add(item.Key, item.Value);
            return result;
        }

        public static void SetupModSetting()
        {
            if (!ModSettingAPI.IsInit)
            {
                Debug.Log("[LootNearbyItem] ModSetting未初始化，跳过注册");
                return;
            }
            Debug.Log("[LootNearbyItem] 注册ModSetting配置项");

            // 快捷键 - 使用按键绑定
            ModSettingAPI.AddKeybinding("SearchHotKey",
                LocalizationUtil.SearchHotKeySetting,
                GetSearchKeyCode(), KeyCode.H,
                (key) => { SetSearchKeyCode(key.ToString()); SaveConfig(); });

            // 搜索地面物品半径
            ModSettingAPI.AddSlider("SearchPickupRadius",
                LocalizationUtil.SearchPickupRadiusSetting,
                GetSearchPickupRadius(), new Vector2(0.3f, 999f),
                (val) => { SetSearchPickupRadius(val); SaveConfig(); }, 1, 6);

            // 搜索敌人战利品盒
            ModSettingAPI.AddToggle("SearchContainers",
                LocalizationUtil.SearchContainersSetting,
                GetSearchContainers(),
                (val) => { SetSearchContainers(val); SaveConfig(); });

            // 敌人战利品盒半径
            ModSettingAPI.AddSlider("SearchContainersRadius",
                LocalizationUtil.SearchContainersRadiusSetting,
                GetSearchContainersRadius(), new Vector2(0.3f, 999f),
                (val) => { SetSearchContainersRadius(val); SaveConfig(); }, 1, 6);

            // 搜索其他容器
            ModSettingAPI.AddToggle("SearchOtherContainers",
                LocalizationUtil.SearchOtherContainersSetting,
                GetSearchOtherContainers(),
                (val) => { SetSearchOtherContainers(val); SaveConfig(); });

            // 其他容器半径
            ModSettingAPI.AddSlider("SearchOtherContainersRadius",
                LocalizationUtil.SearchOtherContainersRadiusSetting,
                GetSearchOtherContainersRadius(), new Vector2(0.3f, 999f),
                (val) => { SetSearchOtherContainersRadius(val); SaveConfig(); }, 1, 6);

            // 搜索土堆/藏匿点
            ModSettingAPI.AddToggle("SearchHiddenContainers",
                LocalizationUtil.SearchHiddenContainersSetting,
                GetSearchHiddenContainers(),
                (val) => { SetSearchHiddenContainers(val); SaveConfig(); });

            // 土堆/藏匿点搜索半径
            ModSettingAPI.AddSlider("SearchHiddenContainersRadius",
                LocalizationUtil.SearchHiddenContainersRadiusSetting,
                GetSearchHiddenContainersRadius(), new Vector2(0.3f, 999f),
                (val) => { SetSearchHiddenContainersRadius(val); SaveConfig(); }, 1, 6);

            // 自动拆卸插槽
            ModSettingAPI.AddToggle("AutoUnplugSlots",
                LocalizationUtil.AutoUnplugSlotsSetting,
                GetAutoUnplugSlots(),
                (val) => { SetAutoUnplugSlots(val); SaveConfig(); });

            // 保留搜索时间
            ModSettingAPI.AddToggle("SearchTimeKeep",
                LocalizationUtil.SearchTimeKeepSetting,
                GetSearchTimeKeep(),
                (val) => { SetSearchTimeKeep(val); SaveConfig(); });

            // 生成临时垃圾堆
            ModSettingAPI.AddToggle("GenerateTempTrashCan",
                LocalizationUtil.GenerateTempTrashCanSetting,
                GetGeneratorTempTrashCan(),
                (val) => { SetGeneratorTempTrashCan(val); SaveConfig(); });

            // 垃圾堆阈值
            ModSettingAPI.AddSlider("GenerateTempTrashCanThreshold",
                LocalizationUtil.GenerateTempTrashCanThresholdSetting,
                GetGeneratorTempTrashCanThreshold(), 0, 999,
                (val) => { SetGeneratorTempTrashCanThreshold(val); SaveConfig(); }, 5);

            Debug.Log("[LootNearbyItem] ModSetting配置注册完成");
        }
    }
}
