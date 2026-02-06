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

        void OnEnable()
        {
            Debug.Log("AutoFilterKeyAndFormula Loaded!!!");
            // 创建Harmony实例
            harmony = new Harmony(HarmonyId);
            // 直接应用补丁
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        void OnDisable()
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