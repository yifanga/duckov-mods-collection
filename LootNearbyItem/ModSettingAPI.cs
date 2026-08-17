using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duckov.Modding;
using UnityEngine;

namespace LootNearbyItem
{
    /// <summary>
    /// ModSetting API 反射封装，参考 CombatMaid 的实现方式
    /// 在 OnAfterSetup 中通过 this.info 初始化
    /// </summary>
    public static class ModSettingAPI
    {
        public const string MOD_NAME = "ModSetting";
        private const string TYPE_NAME = "ModSetting.ModBehaviour";

        private static Type _modBehaviour;
        private static ModInfo _modInfo;
        private static Dictionary<string, Delegate> _methodCache = new Dictionary<string, Delegate>();

        private static readonly string[] RequiredMethods = new string[]
        {
            "AddDropDownList", "AddSlider", "AddToggle", "GetValue", "SetValue",
            "RemoveUI", "RemoveMod", "AddInput", "HasConfig", "GetSavedValue",
            "AddKeybindingWithDefault", "AddButton", "AddGroup", "AddKeybindingWithKey", "Clear"
        };

        public static bool IsInit { get; private set; }

        /// <summary>
        /// 初始化ModSetting API，在ModBehaviour.OnAfterSetup中调用
        /// </summary>
        public static bool Init(ModInfo modInfo)
        {
            if (IsInit) return true;

            if (modInfo.name == "ModSetting")
            {
                Debug.LogError("[LootNearbyItem] 不能使用ModSetting自己的info初始化");
                return false;
            }

            _modInfo = modInfo;
            _modBehaviour = FindTypeInAssemblies(TYPE_NAME);
            if (_modBehaviour == null)
            {
                Debug.Log("[LootNearbyItem] 未找到ModSetting.ModBehaviour类型，ModSetting可能未安装");
                return false;
            }

            // 检查所有必需方法是否存在
            foreach (var methodName in RequiredMethods)
            {
                var methods = _modBehaviour.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.Name == methodName).ToArray();
                if (methods.Length == 0)
                {
                    Debug.LogError($"[LootNearbyItem] ModSetting缺少方法: {methodName}");
                    return false;
                }
            }

            IsInit = true;
            Debug.Log($"[LootNearbyItem] ModSetting API初始化成功，mod: {_modInfo.name}");
            return true;
        }

        public static bool AddToggle(string key, string description, bool enable, Action<bool> onValueChange = null)
        {
            if (!Available(key)) return false;
            return InvokeMethod("AddToggle", "AddToggle",
                new object[] { _modInfo, key, description, enable, onValueChange },
                typeof(Action<ModInfo, string, string, bool, Action<bool>>),
                new Type[] { typeof(ModInfo), typeof(string), typeof(string), typeof(bool), typeof(Action<bool>) });
        }

        public static bool AddSlider(string key, string description, float defaultValue, Vector2 sliderRange,
            Action<float> onValueChange = null, int decimalPlaces = 1, int characterLimit = 5)
        {
            if (!Available(key)) return false;
            return InvokeMethod("AddSliderFloat", "AddSlider",
                new object[] { _modInfo, key, description, defaultValue, sliderRange, onValueChange, decimalPlaces, characterLimit },
                typeof(Action<ModInfo, string, string, float, Vector2, Action<float>, int, int>),
                new Type[] { typeof(ModInfo), typeof(string), typeof(string), typeof(float), typeof(Vector2), typeof(Action<float>), typeof(int), typeof(int) });
        }

        public static bool AddSlider(string key, string description, int defaultValue, int minValue, int maxValue,
            Action<int> onValueChange = null, int characterLimit = 5)
        {
            if (!Available(key)) return false;
            return InvokeMethod("AddSliderInt", "AddSlider",
                new object[] { _modInfo, key, description, defaultValue, minValue, maxValue, onValueChange, characterLimit },
                typeof(Action<ModInfo, string, string, int, int, int, Action<int>, int>),
                new Type[] { typeof(ModInfo), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(Action<int>), typeof(int) });
        }

        public static bool AddKeybinding(string key, string description, KeyCode keyCode, KeyCode defaultKeyCode,
            Action<KeyCode> onValueChange = null)
        {
            if (!Available(key)) return false;
            return InvokeMethod("AddKeybindingWithDefault", "AddKeybindingWithDefault",
                new object[] { _modInfo, key, description, keyCode, defaultKeyCode, onValueChange },
                typeof(Action<ModInfo, string, string, KeyCode, KeyCode, Action<KeyCode>>),
                new Type[] { typeof(ModInfo), typeof(string), typeof(string), typeof(KeyCode), typeof(KeyCode), typeof(Action<KeyCode>) });
        }

        public static bool AddDropdownList(string key, string description, List<string> options, string defaultValue,
            Action<string> onValueChange = null)
        {
            if (!Available(key)) return false;
            return InvokeMethod("AddDropDownList", "AddDropDownList",
                new object[] { _modInfo, key, description, options, defaultValue, onValueChange },
                typeof(Action<ModInfo, string, string, List<string>, string, Action<string>>),
                new Type[] { typeof(ModInfo), typeof(string), typeof(string), typeof(List<string>), typeof(string), typeof(Action<string>) });
        }

        public static bool AddInput(string key, string description, string defaultValue, int characterLimit = 40,
            Action<string> onValueChange = null)
        {
            if (!Available(key)) return false;
            return InvokeMethod("AddInput", "AddInput",
                new object[] { _modInfo, key, description, defaultValue, characterLimit, onValueChange },
                typeof(Action<ModInfo, string, string, string, int, Action<string>>),
                new Type[] { typeof(ModInfo), typeof(string), typeof(string), typeof(string), typeof(int), typeof(Action<string>) });
        }

        public static bool AddButton(string key, string description, string buttonText, Action onClick = null)
        {
            if (!Available(key)) return false;
            return InvokeMethod("AddButton", "AddButton",
                new object[] { _modInfo, key, description, buttonText, onClick },
                typeof(Action<ModInfo, string, string, string, Action>),
                new Type[] { typeof(ModInfo), typeof(string), typeof(string), typeof(string), typeof(Action) });
        }

        public static bool AddGroup(string key, string description, List<string> keys, float scale = 0.7f,
            bool topInsert = false, bool open = false)
        {
            if (!Available(key)) return false;
            return InvokeMethod("AddGroup", "AddGroup",
                new object[] { _modInfo, key, description, keys, scale, topInsert, open },
                typeof(Action<ModInfo, string, string, List<string>, float, bool, bool>),
                new Type[] { typeof(ModInfo), typeof(string), typeof(string), typeof(List<string>), typeof(float), typeof(bool), typeof(bool) });
        }

        public static bool GetSavedValue<T>(string key, out T value)
        {
            value = default(T);
            if (!Available(key)) return false;

            var method = GetStaticPublicMethodInfo("GetSavedValue");
            if (method == null) return false;

            var genericMethod = method.MakeGenericMethod(typeof(T));
            object[] args = new object[] { _modInfo, key, null };
            bool result = (bool)genericMethod.Invoke(null, args);
            if (result && args[2] != null)
                value = (T)args[2];
            return result;
        }

        public static bool Clear(Action<bool> callback = null)
        {
            if (!Available()) return false;
            return InvokeMethod("Clear", "Clear",
                new object[] { _modInfo, callback },
                typeof(Action<ModInfo, Action<bool>>),
                new Type[] { typeof(ModInfo), typeof(Action<bool>) });
        }

        private static bool Available()
        {
            return IsInit && _modInfo.name != null;
        }

        private static bool Available(string key)
        {
            return IsInit && _modInfo.name != null && key != null;
        }

        private static Type FindTypeInAssemblies(string typeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(typeName);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }

        private static MethodInfo GetStaticPublicMethodInfo(string methodName, Type[] parameterTypes = null)
        {
            if (!IsInit) return null;
            var bindingAttr = BindingFlags.Static | BindingFlags.Public;

            if (parameterTypes != null)
            {
                return _modBehaviour.GetMethods(bindingAttr)
                    .Where(m => m.Name == methodName)
                    .Where(m =>
                    {
                        var ps = m.GetParameters();
                        if (ps.Length != parameterTypes.Length) return false;
                        for (int i = 0; i < ps.Length; i++)
                        {
                            if (!IsParameterTypeMatch(ps[i].ParameterType, parameterTypes[i]))
                                return false;
                        }
                        return true;
                    }).FirstOrDefault();
            }
            return _modBehaviour.GetMethod(methodName, bindingAttr);
        }

        private static bool IsParameterTypeMatch(Type parameterType, Type providedType)
        {
            if (parameterType == providedType) return true;
            if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == providedType) return true;
            if (parameterType.IsAssignableFrom(providedType)) return true;
            return false;
        }

        private static bool InvokeMethod(string cacheKey, string methodName, object[] parameters, Type delegateType, Type[] paramTypes)
        {
            if (!_methodCache.ContainsKey(cacheKey))
            {
                var method = GetStaticPublicMethodInfo(methodName, paramTypes);
                if (method == null)
                {
                    Debug.LogError($"[LootNearbyItem] 找不到ModSetting方法: {methodName}");
                    return false;
                }
                _methodCache[cacheKey] = Delegate.CreateDelegate(delegateType, method);
            }
            try
            {
                _methodCache[cacheKey].DynamicInvoke(parameters);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LootNearbyItem] 调用ModSetting.{methodName}失败: {ex.Message}");
                return false;
            }
        }
    }
}
