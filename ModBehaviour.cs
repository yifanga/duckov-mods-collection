using System;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Reflection;
using Duckov.MasterKeys.UI;

namespace AutoFilterKeyAndFormula
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {

        private const string HarmonyId = "duckovMods.AutoFilterKeyAndFormula";

        private Harmony harmony;

        void Awake()
        {
            Debug.Log("AutoPickKeyAndRecipe Loaded!!!");
            // 创建Harmony实例
            harmony = new Harmony(HarmonyId);

            // 直接应用补丁
            var originalMethod = AccessTools.Method(typeof(FormulasRegisterView), "OnOpen");
            var postfixMethod = AccessTools.Method(typeof(CustomFilters), "OnFormulaOpenPostfix");

            if (originalMethod != null && postfixMethod != null)
            {
                harmony.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
                Debug.Log("Successfully patched FormulasRegisterView.OnOpen");
            }
            else
            {
                Debug.LogError("Failed to find patching methods");
            }

             // 直接应用补丁
            var originalKeyMethod = AccessTools.Method(typeof(MasterKeysRegisterView), "OnOpen");
            var postfixKeyMethod = AccessTools.Method(typeof(CustomFilters), "OnKeyOpenPostfix");

            if (originalKeyMethod != null && postfixKeyMethod != null)
            {
                harmony.Patch(originalKeyMethod, postfix: new HarmonyMethod(postfixKeyMethod));
                Debug.Log("Successfully patched MasterKeysRegisterView.OnOpen");
            }
            else
            {
                Debug.LogError("Failed to find patching methods");
            }

            // BitcoinMethod
            var originalBitcoinMethod = AccessTools.Method(typeof(BitcoinMinerView), "OnOpen");
            var postfixBitcoinMethod = AccessTools.Method(typeof(CustomFilters), "OnBitcoinMinerOpenPostfix");

            if (originalBitcoinMethod != null && postfixBitcoinMethod != null)
            {
                harmony.Patch(originalBitcoinMethod, postfix: new HarmonyMethod(postfixBitcoinMethod));
                Debug.Log("Successfully patched MasterKeysRegisterView.OnOpen");
            }
            else
            {
                Debug.LogError("Failed to find patching methods");
            }

            // ItemDecomposeView itemDecomposeView;

        }
        void OnDestroy()
        {
            if (harmony != null)
            {
                // 更精确的卸载方式 - 只移除本mod的补丁
                harmony.UnpatchAll(HarmonyId);
                harmony = null;
                Debug.Log("Harmony patches removed");
            }

        }

    }
}