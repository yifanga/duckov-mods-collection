using System;
using System.Collections.Generic;
using System.IO;
using Duckov.Modding;
using UnityEngine;

namespace DuckovBetterRealDog
{
    // 宠物配置数据类
    [Serializable]
    public class PetConfigData
    {
        public string togglePetSearchKey = "L";
        public string unloadPetItemsKey = "V";

        public bool toggleNormalPattern  = false;

        public string targetWord = "GODDOG";

        public bool togglePetAlwaysFollow  = false;

        public int petSearchMode = 0;

        [NonSerialized] public KeyCode togglePetSearchKeyCode = KeyCode.L;
        [NonSerialized] public KeyCode unloadPetItemsKeyCode = KeyCode.V;
    }

    // 配置管理器基类（可重用）
    public abstract class BaseModConfigManager<T> where T : class, new()
    {
        protected static T _current = new T();
        protected const string MOD_NAME = "DuckovBetterRealDog";
        protected static string ModFolderPath => Path.Combine(ModManager.DefaultModFolderPath, MOD_NAME);
        protected static string ConfigFilePath => Path.Combine(ModFolderPath, "config.txt");

        public static void Init()
        {
            EnsureDirectoryExists();
            LoadOrCreateConfig();
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(ModFolderPath))
                Directory.CreateDirectory(ModFolderPath);
        }

        private static void LoadOrCreateConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    _current = JsonUtility.FromJson<T>(json);
                    PostLoadValidationAndFix();
                    Debug.Log($"{MOD_NAME}: 配置加载成功");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"{MOD_NAME}: 加载配置失败: {ex.Message}, 使用默认配置");
                    ResetToDefaults();
                }
            }
            else
            {
                ResetToDefaults();
                SaveConfig();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                string json = JsonUtility.ToJson(_current, true);
                File.WriteAllText(ConfigFilePath, json);
                Debug.Log($"{MOD_NAME}: 配置保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{MOD_NAME}: 保存配置失败: {ex.Message}");
            }
        }

        protected static void ResetToDefaults()
        {
            _current = new T();
        }

        protected static void PostLoadValidationAndFix()
        {
            // 子类可以重写此方法进行验证
        }

        // 获取当前配置实例
        public static T Current => _current;
    }

    // 宠物专用配置管理器
    public class ModConfigManager : BaseModConfigManager<PetConfigData>
    {
        private const string MOD_NAME = "DuckovBetterRealDog";

        private const string VALID_CHAR = "ABCDEFGHIJKLMNOPQRSTUVWXYZ!?0123456789";

        // 参考 ModConfigManager 的有效按键范围

        public static void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable()) return;

            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);

            // 创建按键选项字典
            var keyOptions = LocalizationUtil.GetKeyMappingDictionary();
            var modeOptions = LocalizationUtil.GetSearchModeDictionary();

            ModConfigAPI.SafeAddDropdownList(
                MOD_NAME,
                "PetSearchModeSetting",
                LocalizationUtil.PetSearchModeSetting,
                modeOptions,
                typeof(int),
                _current.petSearchMode
            );

            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "TogglePetAlwaysFollowSetting",
                LocalizationUtil.TogglePetAlwaysFollowSetting,
                _current.togglePetAlwaysFollow
            );

            // 注册配置项
            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "WordPatternSetting",
                LocalizationUtil.WordPatternSetting,
                typeof(string),
                _current.targetWord,
                null
            );

            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "ToggleNormalPatternSetting",
                LocalizationUtil.ToggleNormalPatternSetting,
                _current.toggleNormalPattern
            );

            ModConfigAPI.SafeAddDropdownList(
                MOD_NAME,
                "UnloadPetItemsKey",
                LocalizationUtil.PetDropBoxKeySetting,
                keyOptions,
                typeof(string),
                _current.unloadPetItemsKey
            );

            ModConfigAPI.SafeAddDropdownList(
                MOD_NAME,
                "TogglePetSearchKey",
                LocalizationUtil.PetSearchToggleKeySetting,
                keyOptions,
                typeof(string),
                _current.togglePetSearchKey
            );
        }

        public static void OnModConfigOptionsChanged(string key)
        {
            if (!key.StartsWith(MOD_NAME + "_")) return;

            LoadConfigFromModConfig();
            SaveConfig();

            Debug.Log($"PetControl: 配置更新 - {key}");
        }

        public static void LoadConfigFromModConfig()
        {
            string newToggleKey = ModConfigAPI.SafeLoad<string>(MOD_NAME, "TogglePetSearchKey", "L");
            string newUnloadKey = ModConfigAPI.SafeLoad<string>(MOD_NAME, "UnloadPetItemsKey", "V");

            // 验证按键有效性
            _current.togglePetSearchKey = newToggleKey;
            if (Enum.TryParse(newToggleKey, true, out KeyCode tk))
                _current.togglePetSearchKeyCode = tk;
        
            _current.unloadPetItemsKey = newUnloadKey;
            if (Enum.TryParse(newUnloadKey, true, out KeyCode uk))
                _current.unloadPetItemsKeyCode = uk;

            _current.toggleNormalPattern = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "ToggleNormalPatternSetting", false);

        
            string wordPattern = ModConfigAPI.SafeLoad<string>(MOD_NAME, "WordPatternSetting", "GODDOG");
            if(null == wordPattern || wordPattern.Length == 0)
            {
                ModConfigAPI.SafeSave<string>(MOD_NAME, "WordPatternSetting", "GODDOG");
                throw new ArgumentException("Invalid Empty wordPattern! " + wordPattern);
            }
            foreach (char c in wordPattern)
            {
                if (!VALID_CHAR.Contains(c))
                {
                    ModConfigAPI.SafeSave<string>(MOD_NAME, "WordPatternSetting", "GODDOG");
                    throw new ArgumentException("Invalid Char! " + c);
                }
            }
            _current.targetWord = wordPattern;

            _current.togglePetAlwaysFollow = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "TogglePetAlwaysFollowSetting", false);
            _current.petSearchMode = ModConfigAPI.SafeLoad<int>(MOD_NAME, "PetSearchModeSetting", 0);
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

       
        // 快捷访问器
        public static KeyCode ToggleSearchKey => _current.togglePetSearchKeyCode;
        public static KeyCode UnloadItemsKey => _current.unloadPetItemsKeyCode;
        public static bool  ToggleNormalPattern => _current.toggleNormalPattern;
        public static string TargetWord => _current.targetWord;
        public static bool TogglePetAlwaysFollow => _current.togglePetAlwaysFollow;
        public static int PetSearchMode => _current.petSearchMode;
        
    }
}