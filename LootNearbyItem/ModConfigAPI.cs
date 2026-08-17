using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LootNearbyItem
{
    public static class ModConfigAPI
    {
        public const string ModConfigName = "ModConfig";
        private const int ModConfigVersion = 1;
        private static readonly string TAG = $"ModConfig_v{ModConfigVersion}";

        private static Type modBehaviourType;
        private static Type optionsManagerType;
        public static bool isInitialized = false;
        private static bool versionChecked = false;
        private static bool isVersionCompatible = false;

        private static bool CheckVersionCompatibility()
        {
            if (versionChecked) return isVersionCompatible;
            try
            {
                FieldInfo field = modBehaviourType.GetField("VERSION", BindingFlags.Static | BindingFlags.Public);
                if (field != null && field.FieldType == typeof(int))
                {
                    int ver = (int)field.GetValue(null);
                    isVersionCompatible = ver == ModConfigVersion;
                    if (!isVersionCompatible)
                    {
                        Debug.LogError($"[{TAG}] 版本不匹配！API版本: {ModConfigVersion}, ModConfig版本: {ver}");
                        return false;
                    }
                    Debug.Log($"[{TAG}] 版本检查通过: {ModConfigVersion}");
                    versionChecked = true;
                    return true;
                }
                Debug.LogWarning($"[{TAG}] 未找到版本信息字段，跳过版本检查");
                isVersionCompatible = true;
                versionChecked = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 版本检查失败: {ex.Message}");
                isVersionCompatible = false;
                versionChecked = true;
                return false;
            }
        }

        public static bool Initialize()
        {
            try
            {
                if (isInitialized) return true;
                modBehaviourType = FindTypeInAssemblies("ModConfig.ModBehaviour");
                if (modBehaviourType == null)
                {
                    Debug.LogWarning($"[{TAG}] ModConfig.ModBehaviour 类型未找到，ModConfig 可能未加载");
                    return false;
                }
                optionsManagerType = FindTypeInAssemblies("ModConfig.OptionsManager_Mod");
                if (optionsManagerType == null)
                {
                    Debug.LogWarning($"[{TAG}] ModConfig.OptionsManager_Mod 类型未找到");
                    return false;
                }
                if (!CheckVersionCompatibility())
                {
                    Debug.LogWarning($"[{TAG}] ModConfig version mismatch!!!");
                    return false;
                }
                string[] requiredMethods = { "AddDropdownList", "AddInputWithSlider", "AddBoolDropdownList", "AddOnOptionsChangedDelegate", "RemoveOnOptionsChangedDelegate" };
                foreach (string method in requiredMethods)
                {
                    if (modBehaviourType.GetMethod(method, BindingFlags.Static | BindingFlags.Public) == null)
                    {
                        Debug.LogError($"[{TAG}] 必要方法 {method} 未找到");
                        return false;
                    }
                }
                isInitialized = true;
                Debug.Log($"[{TAG}] ModConfigAPI 初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 初始化失败: {ex.Message}");
                return false;
            }
        }

        private static Type FindTypeInAssemblies(string typeName)
        {
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        Type type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            Debug.Log($"[{TAG}] 在程序集 {assembly.FullName} 中找到类型 {typeName}");
                            return type;
                        }
                    }
                    catch { }
                }
                Debug.LogWarning($"[{TAG}] 在所有程序集中未找到类型 {typeName}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 程序集扫描失败: {ex.Message}");
                return null;
            }
        }

        public static bool SafeAddOnOptionsChangedDelegate(Action<string> action)
        {
            if (!Initialize() || action == null) return false;
            try
            {
                MethodInfo method = modBehaviourType.GetMethod("AddOnOptionsChangedDelegate", BindingFlags.Static | BindingFlags.Public);
                method.Invoke(null, new object[] { action });
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 添加选项变更事件委托失败: {ex.Message}");
                return false;
            }
        }

        public static bool SafeRemoveOnOptionsChangedDelegate(Action<string> action)
        {
            if (!Initialize() || action == null) return false;
            try
            {
                MethodInfo method = modBehaviourType.GetMethod("RemoveOnOptionsChangedDelegate", BindingFlags.Static | BindingFlags.Public);
                method.Invoke(null, new object[] { action });
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 移除选项变更事件委托失败: {ex.Message}");
                return false;
            }
        }

        public static bool SafeAddDropdownList(string modName, string key, string description, SortedDictionary<string, object> options, Type valueType, object defaultValue)
        {
            key = modName + "_" + key;
            if (!Initialize()) return false;
            try
            {
                MethodInfo method = modBehaviourType.GetMethod("AddDropdownList", BindingFlags.Static | BindingFlags.Public);
                method.Invoke(null, new object[] { modName, key, description, options, valueType, defaultValue });
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 添加下拉列表失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        public static bool SafeAddInputWithSlider(string modName, string key, string description, Type valueType, object defaultValue, Vector2? sliderRange = null)
        {
            key = modName + "_" + key;
            if (!Initialize()) return false;
            try
            {
                MethodInfo method = modBehaviourType.GetMethod("AddInputWithSlider", BindingFlags.Static | BindingFlags.Public);
                object[] parameters = sliderRange.HasValue
                    ? new object[] { modName, key, description, valueType, defaultValue, sliderRange.Value }
                    : new object[] { modName, key, description, valueType, defaultValue, null };
                method.Invoke(null, parameters);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 添加滑条输入框失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        public static bool SafeAddBoolDropdownList(string modName, string key, string description, bool defaultValue)
        {
            key = modName + "_" + key;
            if (!Initialize()) return false;
            try
            {
                MethodInfo method = modBehaviourType.GetMethod("AddBoolDropdownList", BindingFlags.Static | BindingFlags.Public);
                method.Invoke(null, new object[] { modName, key, description, defaultValue });
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 添加布尔下拉列表失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        public static T SafeLoad<T>(string mod_name, string key, T defaultValue = default(T))
        {
            key = mod_name + "_" + key;
            if (!Initialize() || string.IsNullOrEmpty(key)) return defaultValue;
            try
            {
                MethodInfo method = optionsManagerType.GetMethod("Load", BindingFlags.Static | BindingFlags.Public);
                if (method == null) return defaultValue;
                MethodInfo generic = method.MakeGenericMethod(typeof(T));
                object result = generic.Invoke(null, new object[] { key, defaultValue });
                return (T)result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 加载配置失败 {key}: {ex.Message}");
                return defaultValue;
            }
        }

        public static bool SafeSave<T>(string mod_name, string key, T value)
        {
            key = mod_name + "_" + key;
            if (!Initialize() || string.IsNullOrEmpty(key)) return false;
            try
            {
                MethodInfo method = optionsManagerType.GetMethod("Save", BindingFlags.Static | BindingFlags.Public);
                if (method == null) return false;
                MethodInfo generic = method.MakeGenericMethod(typeof(T));
                generic.Invoke(null, new object[] { key, value });
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 保存配置失败 {key}: {ex.Message}");
                return false;
            }
        }

        public static bool IsAvailable() => Initialize();

        public static bool IsVersionCompatible()
        {
            if (!Initialize()) return false;
            return isVersionCompatible;
        }
    }
}
