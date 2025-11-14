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

        // 参考 ModConfigManager 的有效按键范围

        public static void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable()) return;

            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);

            // 创建按键选项字典
            var keyOptions = LocalizationUtil.GetKeyMappingDictionary();

            // 注册配置项
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
        
    }
}