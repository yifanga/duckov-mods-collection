using System;
using System.Reflection;
using Duckov.Modding;
using Duckov.UI;
using UnityEngine;
using UnityEngine.UI;


namespace LootNearbyItem
{

    public class DynamicHarmonyPatcher
    {
        public static FieldInfo LootViewPickAllButtonField = typeof(LootView).GetField("pickAllButton",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo LootViewStoreAllButtonField = typeof(LootView).GetField("storeAllButton",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private const string HARMONY_ID = "LootNearbyItem.DynamicHarmonyPatcher";
        private static bool _isPatched = false;

        private static object? _harmonyInstance;

        internal static void OnModConfigMenuActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            Initialize();
        }

        // 初始化并应用补丁
        public static void Initialize()
        {
            if (_isPatched)
            {
                Debug.Log("LootNearbyItem Harmony already patched!");
                return;
            }
            if (!IsHarmonyAvailable())
            {
                Debug.Log("LootNearbyItem Harmony not available, skipping patching");
                return;
            }

            ApplyPatch();
        }

        // 应用补丁
        public static void ApplyPatch()
        {
            Debug.Log("LootNearbyItem Harmony try start ApplyPatch");
            if (_isPatched) return;

            try
            {
                Debug.Log("LootNearbyItem Harmony start GetHarmonyType");
                // 获取 Harmony 类型
                var harmonyType = GetHarmonyType();
                if (harmonyType == null) return;

                // 创建 Harmony 实例
                _harmonyInstance = Activator.CreateInstance(harmonyType, HARMONY_ID);

                // 获取要修补的方法
                var originalMethod = typeof(LootView).GetMethod("RefreshPickAllButton",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (originalMethod == null) return;

                // 获取补丁方法
                var postfixMethod = typeof(DynamicHarmonyPatcher).GetMethod("Postfix",
                    BindingFlags.Static | BindingFlags.NonPublic);
                if (postfixMethod == null) return;

                // 创建 Harmony 方法
                var harmonyMethodType = GetHarmonyMethodType();
                var harmonyMethod = Activator.CreateInstance(harmonyMethodType, postfixMethod);

                // 应用补丁
                Debug.Log("LootNearbyItem Harmony start get patch method");
                var patchMethod = harmonyType.GetMethod("Patch", new[] { typeof(MethodBase), harmonyMethodType, harmonyMethodType, harmonyMethodType, harmonyMethodType });
                patchMethod.Invoke(_harmonyInstance, new[] { originalMethod, null, harmonyMethod, null, null });
                Debug.Log("LootNearbyItem Harmony start end patch method");

                _isPatched = true;
                Debug.Log("LootNearbyItem Harmony success patch");
            }
            catch (Exception ex)
            {
                Debug.LogError($"LootNearbyItem Patch failed: {ex}");
            }
        }

        // 移除补丁
        public static void RemovePatch()
        {
            Debug.Log("LootNearbyItem Harmony try start RemovePatch");
            if (!_isPatched) return;

            if (_harmonyInstance == null)
            {
                _isPatched = false;
                return;
            }

            try
            {
                Debug.Log("LootNearbyItem Harmony available, start UnpatchAll");
                var unpatchAllMethod = _harmonyInstance.GetType().GetMethod("UnpatchAll");
                unpatchAllMethod!.Invoke(_harmonyInstance, new object[] { HARMONY_ID });
                _isPatched = false;
                Debug.Log("LootNearbyItem Harmony available, end UnpatchAll");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unpatch failed: {ex}");
            }
        }

        // 补丁方法
        private static void Postfix(LootView __instance)
        {   
            // 除仓库外，启用排序和拾取全部按钮
            // Debug.Log("LootNearbyItem Harmony postfix, start");
            if (null != LootView.Instance &&  PlayerStorage.Inventory != LootView.Instance.TargetInventory)
            {
                ((Button)LootViewPickAllButtonField.GetValue(__instance))?.gameObject.SetActive(true);
                ((Button)LootViewStoreAllButtonField.GetValue(__instance))?.gameObject.SetActive(true);
            }
            // Debug.Log("LootNearbyItem Harmony postfix, end");
        }

        // 检查 Harmony 可用性
        private static bool IsHarmonyAvailable()
        {
            try
            {
                return GetHarmonyType() != null;
            }
            catch
            {
                return false;
            }
        }

        // 获取 Harmony 类型
        private static Type GetHarmonyType()
        {
            return Type.GetType("HarmonyLib.Harmony, 0Harmony");
        }

        // 获取 HarmonyMethod 类型
        private static Type GetHarmonyMethodType()
        {
            return Type.GetType("HarmonyLib.HarmonyMethod, 0Harmony");
        }

    }

}