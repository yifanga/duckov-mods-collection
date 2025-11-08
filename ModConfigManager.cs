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

        private const string MOD_NAME = "LootNearByItem";
        private const string DEFAULT_SEARCH_KEY = "H";
        private const bool DEFAULT_SEARCH_CONTAINERS = false;
        private const float DEFAULT_SEARCH_CONTAINERS_RADIUS = 10f;
        private const float DEFAULT_SEARCH_PICKUP_RADIUS = 0.3f;
        private const KeyCode DEFAULT_SEARCH_KEY_CODE = KeyCode.H;
        // 设置配置文件夹路径
        private static string ModFolderPath = Path.Combine(ModManager.DefaultModFolderPath, MOD_NAME);
        private static string ConfigFilePath =  Path.Combine(ModManager.DefaultModFolderPath, MOD_NAME, MOD_NAME);

        // 常用特殊键
        private static readonly string[] ValidKeys =
            {
                // 鼠标键
                "Mouse0", "Mouse1", "Mouse2", "Mouse3", "Mouse4", "Mouse5", "Mouse6", // 左键、右键、中键
                
                // 特殊键
                "Space", "Tab", "Return", "Escape", "Backspace", "Delete",
                
                // 方向键
                "UpArrow", "DownArrow", "LeftArrow", "RightArrow",
                
                // 修饰键
                "LeftShift", "RightShift", "LeftControl", "RightControl", "LeftAlt", "RightAlt",
                
                // 其他常用键
                "CapsLock", "PageUp", "PageDown", "Home", "End"
            };

        [Serializable]
        public class ConfigData
        {
            public string searchKey = DEFAULT_SEARCH_KEY;
            public bool searchContainers = DEFAULT_SEARCH_CONTAINERS;
            public float searchContainersRadius = DEFAULT_SEARCH_CONTAINERS_RADIUS;
            public float searchPickupRadius = DEFAULT_SEARCH_PICKUP_RADIUS;
            
            [NonSerialized]
            public KeyCode searchKeyCode = DEFAULT_SEARCH_KEY_CODE;
        }

        private static ConfigData CreateDefaultConfig()
        {
            return new ConfigData
            {
                searchKey = DEFAULT_SEARCH_KEY,
                searchContainers = DEFAULT_SEARCH_CONTAINERS,
                searchContainersRadius = DEFAULT_SEARCH_CONTAINERS_RADIUS,
                searchPickupRadius = DEFAULT_SEARCH_PICKUP_RADIUS,
                searchKeyCode = DEFAULT_SEARCH_KEY_CODE
            };
        }

        private static ConfigData _current = CreateDefaultConfig();

        public static void Init(string modRootFolder)
        {
            // 创建默认配置
            _current = CreateDefaultConfig();

            // 设置配置文件夹路径
            ModFolderPath = Path.Combine(modRootFolder, MOD_NAME);
            ConfigFilePath = Path.Combine(ModFolderPath, "config.txt");

            try
            {
                // 确保目录存在
                if (!Directory.Exists(ModFolderPath))
                    Directory.CreateDirectory(ModFolderPath);

                LoadOrCreateConfig();
            }
            catch (Exception ex)
            {
                Debug.LogError($"ModConfig initialization failed: {ex.Message}");
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
                    ConfigData tmpConfig = JsonUtility.FromJson<ConfigData>(json);
                    if (ValidateConfig(tmpConfig))
                    {
                        if (Enum.TryParse(tmpConfig.searchKey, true, out KeyCode keyCode))
                        {
                            tmpConfig.searchKeyCode = keyCode;
                        }
                        Debug.Log("Mod configuration loaded successfully");
                        _current = tmpConfig;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load config: {ex.Message}. Creating new config.");
                }
            }

            // 如果上面有任何异常，使用默认配置
            SaveFile(ConfigFilePath, _current);
        }

        public static void SaveConfig()
        {
            if (null == _current)
            {
                return;
            }
            try
            {
                // 确保目录存在
                if (!Directory.Exists(ModFolderPath))
                    Directory.CreateDirectory(ModFolderPath);

                SaveFile(ConfigFilePath, _current);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ModConfig save config failed: {ex.Message}");
            }
        }


        private static bool ValidateConfig(ConfigData tmpConfig)
        {
            if (!IsValidKey(tmpConfig.searchKey))
            {
                Debug.LogWarning($"Invalid key: {tmpConfig.searchKey}. Using default 'H'.");
                return false;
            }
            return true;
        }

        private static bool IsValidKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            // 单字符键 (字母或数字)
            if (key.Length == 1)
            {
                char c = key[0];
                if (char.IsLetterOrDigit(c)) return true;
            }

            // 功能键 (F1-F12)
            if (key.Length == 2 || key.Length == 3)
            {
                if (key[0] == 'F' && int.TryParse(key.Substring(1), out int num))
                {
                    return num >= 1 && num <= 12; // F1-F12
                }
            }
            return Array.Exists(ValidKeys, k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        private static void SaveFile(string configFilePath, ConfigData configData)
        {
            if (configData == null) return;
            try
            {
                string json = "// Loot Nearby Item Mod Configuration\n" +
                              "// ---------------------------------\n" +
                              "// searchKey: 搜索快捷键 (支持值):\n" +
                              "//  字母键: \"A\" 到 \"Z\" (例: \"H\")\n" +
                              "//  数字键: \"1\" 到 \"0\" (例: \"5\")\n" +
                              "//  功能键: \"F1\" 到 \"F12\" (例: \"F3\")\n" +
                              "//  鼠标键: \"Mouse0\"(左键), \"Mouse1\"(右键), \"Mouse2\"(中键), \"Mouse3\"到\"Mouse6\"(侧键)\n" +
                              "//  特殊键: \"Space\", \"Tab\", \"Return\", \"Escape\"\n" +
                              "//  方向键: \"UpArrow\", \"DownArrow\", \"LeftArrow\", \"RightArrow\"\n" +
                              "// searchContainers: true/false - 是否搜索附近战利品容器（击杀掉落）\n" +
                              "// searchContainersRadius: 10.0 - 搜索附近战利品容器的距离半径(0.3m-20m)游戏默认0.3m,这里默认为10米\n" +
                              "// searchPickupRadius: 0.3 - 搜索附近物品的距离半径(0.3m-20m)游戏默认0.3m,建议不要修改\n\n" +
                              JsonUtility.ToJson(configData, true); // 使用美化格式

                File.WriteAllText(configFilePath, json);
                Debug.Log("Mod configuration saved");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save config: {ex.Message}");
            }
        }

        private static string RemoveLineComments(string json)
        {
            var lines = json.Split('\n');
            var result = new StringBuilder();
            foreach (var line in lines)
            {
                // 只移除行首的//（保留行内的//）
                if (!line.TrimStart().StartsWith("//"))
                    result.AppendLine(line);
            }
            return result.ToString();
        }

        public static KeyCode GetSearchKeyCode()
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");

            return _current.searchKeyCode; // 直接返回缓存值
        }

        public static void SetSearchKeyCode(string keyCodeStr)
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");
            if (!IsValidKey(keyCodeStr) || !Enum.TryParse(keyCodeStr, true, out KeyCode keyCode))
            {
                Debug.LogWarning($"Invalid key: {keyCodeStr}. Keep key  unchanged {_current.searchKey}.");
                return;
            }
            _current.searchKey = keyCodeStr;
            _current.searchKeyCode = keyCode;
        }

        public static float GetSearchContainersRadius()
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");
            float clampVal = Mathf.Clamp(_current.searchContainersRadius, 0.3f, 20f);
            _current.searchContainersRadius = clampVal;
            return clampVal; // 直接返回缓存值
        }

        public static void SetSearchContainersRadius(float radius)
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");

            _current.searchContainersRadius = Mathf.Clamp(radius, 0.3f, 20f); // 直接返回缓存值
        }

        public static float GetSearchPickupRadius()
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");
            float clampVal = Mathf.Clamp(_current.searchPickupRadius, 0.3f, 20f);
            _current.searchPickupRadius = clampVal;
            return clampVal; // 直接返回缓存值
        }

        public static void SetSearchPickupRadius(float radius)
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");

            _current.searchPickupRadius = Mathf.Clamp(radius, 0.3f, 20f); // 直接返回缓存值
        }

        public static bool GetSearchContainers()
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");

            return _current.searchContainers; // 直接返回缓存值
        }

        public static void SetSearchContainers(bool enableSearch)
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");

            _current.searchContainers = enableSearch; // 直接返回缓存值
        }

        public static void OnModConfigMenuActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                Debug.Log("DisplayItemValue: ModConfig activated!");
                SetupModConfig();
                LoadConfigFromModConfig();
            }
        }

        public static void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                Debug.LogWarning("DisplayItemValue: ModConfig not available");
                return;
            }

            Debug.Log("准备添加ModConfig配置项");

            // 添加配置变更监听
            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);
            ConfigData defaultConfig = CreateDefaultConfig();

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "SearchPickupRadiusSetting",
                LocalizationUtil.SearchPickupRadiusSetting,
                typeof(float),
                GetSearchPickupRadius(),
                new Vector2(0.3f, 20f)
            );
            
            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "SearchContainersRadiusSetting",
                LocalizationUtil.SearchContainersRadiusSetting,
                typeof(float),
                GetSearchContainersRadius(),
                new Vector2(0.3f, 20f)
            );

            // 添加配置项
            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "SearchContainersSetting",
                LocalizationUtil.SearchContainersSetting,
                GetSearchContainers()
            );

            ModConfigAPI.SafeAddDropdownList(
                MOD_NAME,
                "SearchHotKeySetting",
                LocalizationUtil.SearchHotKeySetting,
                ConvertToObjectDictionary(LocalizationUtil.GetKeyMappingDictionary()),
                typeof(string),
                GetSearchKeyCode().ToString()
            );
            
            Debug.Log("DisplayItemValue: ModConfig setup completed");
        }

        public static void OnModConfigOptionsChanged(string key)
        {
            if (!key.StartsWith(MOD_NAME + "_"))
                return;

            // 使用新的 LoadConfig 方法读取配置
            LoadConfigFromModConfig();

            // 保存到本地配置文件
            SaveConfig();

            Debug.Log($"DisplayItemValue: ModConfig updated - {key}");
        }

        public static void LoadConfigFromModConfig()
        {
            // 使用新的 LoadConfig 方法读取所有配置
            SetSearchKeyCode(ModConfigAPI.SafeLoad<string>(MOD_NAME, "SearchHotKeySetting", DEFAULT_SEARCH_KEY));
            SetSearchContainers(ModConfigAPI.SafeLoad<bool>(MOD_NAME, "SearchContainersSetting", DEFAULT_SEARCH_CONTAINERS));
            SetSearchContainersRadius(ModConfigAPI.SafeLoad<float>(MOD_NAME, "SearchContainersRadiusSetting", DEFAULT_SEARCH_CONTAINERS_RADIUS));
            SetSearchPickupRadius(ModConfigAPI.SafeLoad<float>(MOD_NAME, "SearchPickupRadiusSetting", DEFAULT_SEARCH_PICKUP_RADIUS));
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
    }
}