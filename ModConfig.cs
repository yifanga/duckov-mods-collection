using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace LootNearbyItem
{

    public static class ModConfig
    {

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
            public string searchKey = "H";
            public bool searchContainers = false;

            public float searchContainersRadius = 10f;

            [NonSerialized]
            public KeyCode searchKeyCode = KeyCode.H;
        }

        private static ConfigData CreateDefaultConfig()
        {
            return new ConfigData
            {
                searchKey = "H",
                searchContainers = false,
                searchKeyCode = KeyCode.H,
                searchContainersRadius = 10f
            };
        }

        private static ConfigData _current = CreateDefaultConfig();

        public static void Init(string modRootFolder)
        {
            // 创建默认配置
            _current = CreateDefaultConfig();

            // 设置配置文件夹路径
            var modFolderPath = Path.Combine(modRootFolder, "LootNearbyItem");
            var configFilePath = Path.Combine(modFolderPath, "config.txt");

            try
            {
                // 确保目录存在
                if (!Directory.Exists(modFolderPath))
                    Directory.CreateDirectory(modFolderPath);

                LoadOrCreateConfig(configFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ModConfig initialization failed: {ex.Message}");
            }
        }

        private static void LoadOrCreateConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(configFilePath);
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
            SaveFile(configFilePath, _current);
        }

        public static void SaveConfig(string modRootFolder)
        {
            if (null == _current)
            {
                return;
            }
            // 设置配置文件夹路径
            var modFolderPath = Path.Combine(modRootFolder, "LootNearbyItem");
            var configFilePath = Path.Combine(modFolderPath, "config.txt");

            try
            {
                // 确保目录存在
                if (!Directory.Exists(modFolderPath))
                    Directory.CreateDirectory(modFolderPath);

                SaveFile(configFilePath, _current);
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
                Debug.LogWarning($"Invalid key: {_current.searchKey}. Using default 'H'.");
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
                              "// searchContainers: true/false - 是否搜索附近容器\n\n" +
                              "// searchContainersRadius: 10.0 - 搜索附近容器的距离半径(0.3m-20m)\n\n" +
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

        public static void SetSearchKeyCode(KeyCode keyCode)
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");

            _current.searchKeyCode = keyCode;
        }

        public static float GetSearchRadius()
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");
            if (_current.searchContainersRadius <= 0.3f)
            {
                _current.searchContainersRadius = 0.3f;
                return 0.3f;
            }
            if (_current.searchContainersRadius >= 20f)
            {
                _current.searchContainersRadius = 20f;
                return 20f;
            }
            return _current.searchContainersRadius; // 直接返回缓存值
        }

        public static void SetSearchRadius(float radius)
        {
            if (_current == null)
                throw new InvalidOperationException("ModConfig not initialized. Call Init() first.");

            if (radius <= 0.3f)
            {
                radius = 0.3f;

            }
            if (radius >= 20f)
            {
                radius = 20f;
            }
            _current.searchContainersRadius = radius; // 直接返回缓存值
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

    }
}