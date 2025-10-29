using System;
using System.Reflection;
using Duckov.MasterKeys.UI;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;

namespace AutoFilterKeyAndFormula
{
	public static class CustomFilters
	{


		// 后置补丁方法
		public static void OnFormulaOpenPostfix(FormulasRegisterView __instance)
		{
			try
			{
				// 使用反射获取私有字段
				var inventoryDisplayField = typeof(FormulasRegisterView).GetField("inventoryDisplay",
					BindingFlags.NonPublic | BindingFlags.Instance);

				var playerStorageInventoryDisplayField = typeof(FormulasRegisterView).GetField("playerStorageInventoryDisplay",
					BindingFlags.NonPublic | BindingFlags.Instance);

				if (inventoryDisplayField == null || playerStorageInventoryDisplayField == null)
				{
					Debug.LogError("Failed to find inventory display fields");
					return;
				}

				// 获取InventoryDisplay对象
				var inventoryDisplay = inventoryDisplayField.GetValue(__instance) as InventoryDisplay;
				var playerStorageInventoryDisplay = playerStorageInventoryDisplayField.GetValue(__instance) as InventoryDisplay;

				if (inventoryDisplay == null || playerStorageInventoryDisplay == null)
				{
					Debug.LogError("Failed to get inventory display instances");
					return;
				}

				// 应用自定义过滤器
				Func<Item, bool> filter = FormulaFilter();
				inventoryDisplay.SetFilter(filter);
				playerStorageInventoryDisplay.SetFilter(filter);

				Debug.Log("Custom formula filters applied successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error in CustomFormulaFilters: {ex}");
			}
		}

		private static Func<Item, bool> FormulaFilter()
		{

			return (Item e) => (null != e && (e.Tags.Contains("Formula") || e.Tags.Contains("Formula_Blueprint") || e.Tags.Contains("Formula_Medic") || e.Tags.Contains("Formula_Normal") || e.Tags.Contains("Formula_Cook"))) ? true : false;
		}


		public static void OnKeyOpenPostfix(MasterKeysRegisterView __instance)
		{
			try
			{

				// 使用反射获取私有字段
				var inventoryDisplayField = typeof(MasterKeysRegisterView).GetField("inventoryDisplay",
					BindingFlags.NonPublic | BindingFlags.Instance);

				var playerStorageInventoryDisplayField = typeof(MasterKeysRegisterView).GetField("playerStorageInventoryDisplay",
					BindingFlags.NonPublic | BindingFlags.Instance);

				if (inventoryDisplayField == null || playerStorageInventoryDisplayField == null)
				{
					Debug.LogError("Failed to find inventory display fields");
					return;
				}

				// 获取InventoryDisplay对象
				var inventoryDisplay = inventoryDisplayField.GetValue(__instance) as InventoryDisplay;
				var playerStorageInventoryDisplay = playerStorageInventoryDisplayField.GetValue(__instance) as InventoryDisplay;

				if (inventoryDisplay == null || playerStorageInventoryDisplay == null)
				{
					Debug.LogError("Failed to get inventory display instances");
					return;
				}

				// 应用自定义过滤器
				Func<Item, bool> filter = KeyFilter();
				inventoryDisplay.SetFilter(filter);
				playerStorageInventoryDisplay.SetFilter(filter);

				Debug.Log("Custom formula filters applied successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error in CustomFormulaFilters: {ex}");
			}
		}


		private static Func<Item, bool> KeyFilter()
		{
			return (Item e) => ((bool)e && e.Tags.Contains("Key") && !e.Tags.Contains("SpecialKey")) ? true : false;
		}


		public static void OnBitcoinMinerOpenPostfix(BitcoinMinerView __instance)
		{
			try
			{

				// 使用反射获取私有字段
				var inventoryDisplayField = typeof(BitcoinMinerView).GetField("inventoryDisplay",
					BindingFlags.NonPublic | BindingFlags.Instance);

				var playerStorageInventoryDisplayField = typeof(BitcoinMinerView).GetField("storageDisplay",
					BindingFlags.NonPublic | BindingFlags.Instance);

				if (inventoryDisplayField == null || playerStorageInventoryDisplayField == null)
				{
					Debug.LogError("Failed to find inventory display fields");
					return;
				}

				// 获取InventoryDisplay对象
				var inventoryDisplay = inventoryDisplayField.GetValue(__instance) as InventoryDisplay;
				var playerStorageInventoryDisplay = playerStorageInventoryDisplayField.GetValue(__instance) as InventoryDisplay;

				if (inventoryDisplay == null || playerStorageInventoryDisplay == null)
				{
					Debug.LogError("Failed to get inventory display instances");
					return;
				}

				// 应用自定义过滤器
				Func<Item, bool> filter = BitcoinMinerFilter();
				// inventoryDisplay.SetFilter(filter);
				playerStorageInventoryDisplay.SetFilter(filter);

				Debug.Log("Custom formula filters applied successfully");
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error in CustomFormulaFilters: {ex}");
			}
		}


		private static Func<Item, bool> BitcoinMinerFilter()
		{
			return (Item e) => ((bool)e && (e.Tags.Contains("ComputerParts_GPU") || e.TypeID.Equals(388))) ? true : false;
		}



	}
}