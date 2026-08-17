using System;
using System.Reflection;
using Duckov.Modding;
using Duckov.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LootNearbyItem
{
    public class DynamicHarmonyPatcher
    {
        public static FieldInfo LootViewPickAllButtonField = typeof(LootView).GetField("pickAllButton", BindingFlags.Instance | BindingFlags.NonPublic);

        private const string HARMONY_ID = "LootNearbyItem.DynamicHarmonyPatcher";
        private static bool _isPatched = false;
        private static Harmony _harmonyInstance;

        internal static void OnModConfigMenuActivated(Duckov.Modding.ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (_isPatched)
            {
                Debug.Log("LootNearbyItem Harmony already patched!");
                return;
            }
            ApplyPatch();
        }

        public static void ApplyPatch()
        {
            if (_isPatched) return;
            try
            {
                Debug.Log("LootNearbyItem Harmony start ApplyPatch");
                _harmonyInstance = new Harmony(HARMONY_ID);

                MethodInfo original = typeof(LootView).GetMethod("RefreshPickAllButton", BindingFlags.Instance | BindingFlags.NonPublic);
                if (original == null)
                {
                    Debug.LogWarning("LootNearbyItem: LootView.RefreshPickAllButton not found, skipping patch");
                    return;
                }

                MethodInfo postfix = typeof(DynamicHarmonyPatcher).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
                if (postfix == null)
                {
                    Debug.LogWarning("LootNearbyItem: Postfix method not found");
                    return;
                }

                _harmonyInstance.Patch(original, postfix: new HarmonyMethod(postfix));
                _isPatched = true;
                Debug.Log("LootNearbyItem Harmony patch success");
            }
            catch (Exception ex)
            {
                Debug.LogError($"LootNearbyItem Patch failed: {ex}");
            }
        }

        public static void RemovePatch()
        {
            if (!_isPatched || _harmonyInstance == null)
            {
                _isPatched = false;
                return;
            }
            try
            {
                _harmonyInstance.UnpatchAll(HARMONY_ID);
                _isPatched = false;
                Debug.Log("LootNearbyItem Harmony unpatch success");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unpatch failed: {ex}");
            }
        }

        private static void Postfix(LootView __instance)
        {
            if (DynamicLootBoxManager.Instance != null
                && LootView.Instance != null
                && DynamicLootBoxManager.Instance.CurrentBoxInventory != null
                && DynamicLootBoxManager.Instance.CurrentBoxInventory == LootView.Instance.TargetInventory)
            {
                if (LootViewPickAllButtonField != null)
                {
                    Button btn = LootViewPickAllButtonField.GetValue(__instance) as Button;
                    if (btn != null)
                        btn.gameObject.SetActive(true);
                }
            }
        }
    }
}
