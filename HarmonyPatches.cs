using System.Reflection;
using Duckov.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TagInventoryWeight
{



    [HarmonyPatch]
    public static class WeightBarComplex_Patch
    {

        private const string TEXT_OBJECT_NAME = "CustomWeightStatusText";

        [HarmonyPatch(typeof(WeightBarComplex), "OnEnable")]
        [HarmonyPostfix]
        private static void Postfix_OnEnable(WeightBarComplex __instance)
        {
            // 确保只创建一次文本
            var text = FindTag(__instance);
            if (text != null)
            {
                text.SetActive(true);
                return;
            }
            else
            {
                // 创建状态文本
                CreateStatusText(__instance);

                // 初始更新文本
                UpdateWeightText(__instance);
            }

        }

        [HarmonyPatch(typeof(WeightBarComplex), "OnDisable")]
        [HarmonyPostfix]

        private static void Postfix_OnDisable(WeightBarComplex __instance)
        {
            // 销毁文本对象
            var text = FindTag(__instance);
            if (text != null)
            {
                text.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(WeightBarComplex), "RefreshMarkStatus")]
        [HarmonyPostfix]
        private static void Postfix_RefreshMarkStatus(WeightBarComplex __instance)
        {
            Debug.Log("TagInventoryWeight Postfix_RefreshMarkStatus!!!");
            UpdateWeightText(__instance);
        }


        private static void CreateStatusText(WeightBarComplex instance)
        {
            // 创建文本对象作为重量条的兄弟对象
            GameObject textObj = new GameObject(TEXT_OBJECT_NAME);

            // 设置层级关系：重量条次级
            textObj.transform.SetParent(instance.transform);


            // 添加TextMeshPro组件
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "重量状态";
            text.fontSize = 16;
            text.alignment = TextAlignmentOptions.Left;
            text.enableAutoSizing = true;
            text.autoSizeTextContainer = true;
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;
            text.outlineWidth = 0.1f;
            text.outlineColor = Color.black;

            // 设置位置在重量条右侧
            RectTransform rect = textObj.GetComponent<RectTransform>();
            RectTransform barRect = (RectTransform)instance.transform.parent;

            if (barRect != null)
            {
                // 使用锚点定位
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);

                // 设置位置偏移
                rect.anchoredPosition = new Vector2(
                    barRect.rect.width * 1.3f,
                    0
                );

            }
            // 设置尺寸
            rect.sizeDelta = new Vector2(600, 100);

            textObj.SetActive(true);
            // 添加调试标记
            // textObj.AddComponent<WeightStatusDebugMarker>();
        }

        private static void UpdateWeightText(WeightBarComplex instance)
        {
            // 查找文本组件
            var textObj = FindTag(instance);
            if (textObj == null)
            {
                Debug.LogWarning("WeightBarComplex_Patch: 未找到文本组件，尝试重新创建");
                CreateStatusText(instance);
                textObj = FindTag(instance);
                if (textObj == null) return;
            }

            var text = textObj.GetComponent<TextMeshProUGUI>();
            // 获取目标角色
            CharacterMainControl? target = GetTarget();
            if (target == null)
            {
                text.text = "<color=#FF0000>目标角色为空</color>";
                return;
            }

            // 获取当前重量
            float currentWeight = target.CharacterItem.TotalWeight;

            // 获取最大重量（使用反射）
            float maxWeight = GetMaxWeight();
            float lightWeight = maxWeight * 0.25f;
            float middleWeight = maxWeight * 0.5f;
            float superHeavyWeight = maxWeight * 0.75f;

            // 计算超重信息
            float overweightAmount = currentWeight - maxWeight;

            float remainingCapacity = maxWeight - currentWeight;

            // 更新文本内容
            if (currentWeight > maxWeight)
            {
                text.text = $"<color=#FF0000>超重: {currentWeight - maxWeight:F1}kg</color>";
            }
            else if (currentWeight > maxWeight * 0.9f) // 接近超重时
            {
                text.text = $"<color=#FFFF00>距超重: {maxWeight - currentWeight:F1}kg</color>";
            }
            else if (currentWeight > superHeavyWeight) // >=负重时
            {
                text.text = $"<color=#FFFF00>负重: {currentWeight - superHeavyWeight:F1}kg</color>";
            }
            else if (currentWeight > middleWeight) // <=负重时
            {
                text.text = $"距负重: {superHeavyWeight - currentWeight:F1}kg";
            }
            else if (currentWeight > lightWeight) // >=轻盈时
            {
                text.text = $"距轻盈: {currentWeight - lightWeight:F1}kg";
            }
            else if (currentWeight < lightWeight) // >=轻盈时
            {
                text.text = $"<color=#00FF00>轻盈: {lightWeight - currentWeight:F1}kg</color>";
            }
            else
            {
                text.text = $"<color=#00FF00>剩余容量: {currentWeight:F1}kg</color>";
            }

            // 强制更新渲染
            // text.ForceMeshUpdate();
            textObj.SetActive(true);
            Debug.Log($"WeightBarComplex_Patch: 更新文本: {text.text}");
        }

        // 辅助方法：查找文本组件
        private static GameObject? FindTag(WeightBarComplex instance)
        {
            // 在重量条的兄弟对象中查找
            if (instance.transform != null)
            {
                foreach (Transform child in instance.transform)
                {
                    if (child.name == TEXT_OBJECT_NAME)
                    {
                        return child.gameObject;
                    }
                }
            }

            // 在整个场景中查找（备用）
            return null;
        }

        private static CharacterMainControl? GetTarget()
        {
            // 尝试直接访问属性（如果编译器允许）
            return LevelManager.Instance?.MainCharacter;

        }

        // 使用反射获取私有属性 MaxWeight 的值
        private static float GetMaxWeight()
        {
            return GetTarget()?.MaxWeight ?? 0f;
        }
    }
}