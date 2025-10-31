using System;
using System.Reflection;
using Duckov.MasterKeys.UI;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;

namespace AutoFilterKeyAndFormula
{
	[HarmonyPatch]
	public static class HarmonyPatches
	{

		public static InventoryDisplay? GetMemberValue(object instance, string memberName)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (string.IsNullOrEmpty(memberName)) throw new ArgumentException("成员名不能为空");

			Type type = instance.GetType();
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			// 尝试获取字段
			FieldInfo field = type.GetField(memberName, flags);
			if (field != null && field.FieldType == typeof(InventoryDisplay))
			{
				return (InventoryDisplay)field.GetValue(instance);
			}

			// 未找到匹配成员
			return null;
		}


		// 后置补丁方法
		[HarmonyPatch(typeof(FormulasRegisterView), "OnOpen")]
		[HarmonyPostfix]
		public static void OnFormulaOpenPostfix(FormulasRegisterView __instance)
		{
			try
			{
				// 使用反射获取私有字段 InventoryDisplay对象
				var inventoryDisplay = GetMemberValue(__instance, "inventoryDisplay");
				var playerStorageInventoryDisplay = GetMemberValue(__instance, "playerStorageInventoryDisplay");
				if (inventoryDisplay == null || playerStorageInventoryDisplay == null)
				{
					Debug.LogError("FormulasRegisterView Failed to get inventory display instances");
					return;
				}

				// 应用自定义过滤器
				Func<Item, bool> filter = (Item e) => (null != e && (e.Tags.Contains("Formula") || e.Tags.Contains("Formula_Blueprint") || e.Tags.Contains("Formula_Medic") || e.Tags.Contains("Formula_Normal") || e.Tags.Contains("Formula_Cook"))) ? true : false;
				inventoryDisplay.SetFilter(filter);
				playerStorageInventoryDisplay.SetFilter(filter);

				Debug.Log("FormulasRegisterView Custom filters applied successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"FormulasRegisterView Error in Custom Filters: {ex}");
			}
		}



		[HarmonyPatch(typeof(MasterKeysRegisterView), "OnOpen")]
		[HarmonyPostfix]
		public static void OnKeyOpenPostfix(MasterKeysRegisterView __instance)
		{
			try
			{
				// 获取InventoryDisplay对象
				var inventoryDisplay = GetMemberValue(__instance, "inventoryDisplay");
				var playerStorageInventoryDisplay = GetMemberValue(__instance, "playerStorageInventoryDisplay");

				if (inventoryDisplay == null || playerStorageInventoryDisplay == null)
				{
					Debug.LogError("MasterKeysRegisterView Failed to get inventory display instances");
					return;
				}

				// 应用自定义过滤器
				Func<Item, bool> filter = (Item e) => (null != e && e.Tags.Contains("Key") && !e.Tags.Contains("SpecialKey")) ? true : false;
				inventoryDisplay.SetFilter(filter);
				playerStorageInventoryDisplay.SetFilter(filter);

				Debug.Log("MasterKeysRegisterView custom filters applied successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"MasterKeysRegisterView Error in Custom Filters: {ex}");
			}
		}

		[HarmonyPatch(typeof(BitcoinMinerView), "OnOpen")]
		[HarmonyPostfix]
		public static void OnBitcoinMinerOpenPostfix(BitcoinMinerView __instance)
		{
			try
			{

				// 使用反射获取私有字段 获取InventoryDisplay对象

				var playerStorageInventoryDisplay = GetMemberValue(__instance, "storageDisplay");

				if (playerStorageInventoryDisplay == null)
				{
					Debug.LogError("BitcoinMinerView Failed to get inventory display instances");
					return;
				}

				// 应用自定义过滤器
				Func<Item, bool> filter = (Item e) => (null != e && (e.Tags.Contains("ComputerParts_GPU") || e.TypeID.Equals(388))) ? true : false;

				playerStorageInventoryDisplay.SetFilter(filter);

				Debug.Log("BitcoinMinerView Custom filters applied successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"BitcoinMinerView Error in Custom Filters: {ex}");
			}
		}



		[HarmonyPatch(typeof(ItemDecomposeView), "OnOpen")]
		[HarmonyPostfix]
		public static void OnDecomposeOpenPostfix(ItemDecomposeView __instance)
		{
			try
			{
				// 使用反射获取私有字段 InventoryDisplay对象
				var playerStorageInventoryDisplay = GetMemberValue(__instance, "storageDisplay");
				if (playerStorageInventoryDisplay == null)
				{
					Debug.LogError("ItemDecomposeView Failed to get inventory display instances");
					return;
				}

				// 应用自定义过滤器
				Func<Item, bool> filter = (Item e) => (null != e && DecomposeDatabase.CanDecompose(e)) ? true : false;
				playerStorageInventoryDisplay.SetFilter(filter);

				Debug.Log("ItemDecomposeView Custom filters applied successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"ItemDecomposeView Error in Custom Filters: {ex}");
			}
		}

		[HarmonyPatch(typeof(ItemRepairView), "OnOpen")]
		[HarmonyPostfix]
		public static void OnRepairOpenPostfix(ItemRepairView __instance)
		{
			try
			{
				// 使用反射获取私有字段 InventoryDisplay对象
				var playerStorageInventoryDisplay = GetMemberValue(__instance, "inventoryDisplay");

				if (playerStorageInventoryDisplay == null)
				{
					Debug.LogError("ItemRepairView Failed to get  inventory display instances");
					return;
				}

				// 应用自定义过滤器
				Func<Item, bool> filter = (Item e) => (null != e && e.Repairable) ? true : false;
				// inventoryDisplay.SetFilter(filter);
				playerStorageInventoryDisplay.SetFilter(filter);

				Debug.Log("ItemRepairView Custom  filters applied successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error in ItemRepairView: {ex}");
			}
		}

	}
}