using System.Reflection;
using System.Runtime.CompilerServices;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TagInventoryWeight
{



    [HarmonyPatch]
    public static class WeightBarComplex_Patch
    {

        // 存储上次的重量值用于比较
        // 使用 ConditionalWeakTable 存储每个实例的状态
        private static readonly ConditionalWeakTable<WeightBarHUD, InstanceState> stateTable =
            new ConditionalWeakTable<WeightBarHUD, InstanceState>();

        // 每个实例的状态类
        private class InstanceState
        {
            public float LastWeight { get; set; } = -1f;
            public float LastMaxWeight { get; set; } = -1f;
        }

        [HarmonyPatch(typeof(WeightBarHUD), "Update")]
        [HarmonyPostfix]
        private static void ModifyWeightText(WeightBarHUD __instance)
        {

            // 获取当前实例的状态（如果不存在则创建）
            InstanceState state = stateTable.GetValue(__instance, inst => new InstanceState());

            // 获取私有字段值
            float weight = Traverse.Create(__instance).Field("weight").GetValue<float>();
            float maxWeight = Traverse.Create(__instance).Field("maxWeight").GetValue<float>();

            // 检查重量是否变化
            bool weightChanged = !Mathf.Approximately(weight, state.LastWeight) ||
                                !Mathf.Approximately(maxWeight, state.LastMaxWeight);

            // 只在重量变化时更新
            if (weightChanged)
            {
                // 更新存储的重量值
                state.LastWeight = weight;
                state.LastMaxWeight = maxWeight;

                // 只在超重时添加说明
                string originalText = string.Format(__instance.weightTextFormat, weight, maxWeight);
                string extraText = calculateTextByWeight(weight, maxWeight);
                __instance.weightText.text = $"{originalText} ({extraText})";
                RectTransform rectTransform = (RectTransform)__instance.transform;
                if (rectTransform.sizeDelta.x == 200 && rectTransform.sizeDelta.y == 10)
                {
                    rectTransform.sizeDelta = new Vector2(280, 10);
                }
            }
        }


        private static string calculateTextByWeight(float weight, float maxWeight)
        {
            // 获取当前重量
            float currentWeight = weight;

            // 获取最大重量（使用反射）
            float lightWeight = maxWeight * 0.25f;
            float middleWeight = maxWeight * 0.5f;
            float superHeavyWeight = maxWeight * 0.75f;

            // 更新文本内容
            if (currentWeight > maxWeight)
            {
                return $"超重{currentWeight - maxWeight:0.#}kg";
            }
            else if (currentWeight > maxWeight * 0.9f) // 接近超重时
            {

                return $"距超重{maxWeight - currentWeight:0.#}kg";
            }
            else if (currentWeight > superHeavyWeight) // >=负重时
            {
                return $"负重{currentWeight - superHeavyWeight:0.#}kg";
            }
            else if (currentWeight > middleWeight) // <=负重时
            {
                return $"距负重{superHeavyWeight - currentWeight:0.#}kg";
            }
            else if (currentWeight > lightWeight) // >=轻盈时
            {
                return $"距轻盈{currentWeight - lightWeight:0.#}kg";
            }
            else if (currentWeight < lightWeight) // >=轻盈时
            {
                return $"轻盈: {lightWeight - currentWeight:0.#}kg";
            }
            else
            {
                return $"剩余容量{currentWeight:0.#}kg";
            }
        }

    }
}