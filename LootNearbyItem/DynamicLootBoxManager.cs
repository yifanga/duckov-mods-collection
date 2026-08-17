using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

namespace LootNearbyItem
{
    public class DynamicLootBoxManager : MonoBehaviour
    {
        public const int GENERATOR_TEMP_TRASH_CAN_THRESHOLD = 15;

        public static FieldInfo LootboxDisplayNameKeyField = typeof(InteractableLootbox).GetField("displayNameKey", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo LootboxShowSortButtonField = typeof(InteractableLootbox).GetField("showSortButton", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo LootboxBaseOtherInterablesInGroupField = typeof(InteractableBase).GetField("otherInterablesInGroup", BindingFlags.Instance | BindingFlags.NonPublic);
        private static PropertyInfo GunBulletProperty = typeof(ItemSetting_Gun).GetProperty("bulletCount", BindingFlags.Instance | BindingFlags.NonPublic);
        private PropertyInfo ItemInventoryProperty = typeof(Item).GetProperty("Inventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private InteractableLootbox currentHiddenLootBox;

        public static DynamicLootBoxManager Instance { get; private set; }
        public bool IsBoxOpen { get; private set; }

        public Inventory CurrentBoxInventory
        {
            get { return currentHiddenLootBox != null ? currentHiddenLootBox.Inventory : null; }
        }

        public event Action OnBoxOpened;
        public event Action OnBoxClosed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void CreateNewHiddenLootBox()
        {
            if (currentHiddenLootBox != null)
                Destroy(currentHiddenLootBox.gameObject);

            currentHiddenLootBox = Instantiate(InteractableLootbox.Prefab);
            currentHiddenLootBox.transform.SetParent(transform);

            if (currentHiddenLootBox == null)
            {
                GameObject go = new GameObject("DynamicHiddenLootBox");
                go.SetActive(false);
                go.transform.SetParent(transform);
                currentHiddenLootBox = go.AddComponent<InteractableLootbox>();
            }

            LootboxDisplayNameKeyField?.SetValue(currentHiddenLootBox, LocalizationUtil.ScatteredObjectsText);
            LootboxShowSortButtonField?.SetValue(currentHiddenLootBox, true);
            LootboxBaseOtherInterablesInGroupField?.SetValue(currentHiddenLootBox, new List<InteractableBase>());
        }

        public async UniTask AddItemsToBox(List<Item> items)
        {
            Transform mainTrans = GetMainTransform();
            if (mainTrans == null)
            {
                Debug.LogError("Get MainTrans Failed!");
                return;
            }

            if (currentHiddenLootBox == null)
                CreateNewHiddenLootBox();

            if (currentHiddenLootBox.Inventory == null)
                await UniTask.WaitUntil(() => currentHiddenLootBox.Inventory != null);

            if (currentHiddenLootBox == null || currentHiddenLootBox.Inventory == null)
            {
                Debug.LogError("Create HiddenLootBox Failed!");
                return;
            }

            int n = items.Count;
            currentHiddenLootBox.Inventory.SetCapacity(near35(n + 70));

            foreach (Item item in items)
            {
                if (item == null) continue;

                // 处理枪械子弹
                foreach (Item bullet in TryGetBullets(item))
                {
                    AddMergeOrDropItem(bullet, mainTrans);
                }

                // 自动拆卸插槽
                if (ModConfigManager.GetAutoUnplugSlots())
                {
                    foreach (Item slotItem in TryGetSlotItems(item))
                    {
                        AddMergeOrDropItem(slotItem, mainTrans);
                    }
                }

                AddMergeOrDropItem(item, mainTrans);
            }

            int maxIdx = currentHiddenLootBox.Inventory.GetLastItemPosition();
            currentHiddenLootBox.Inventory.SetCapacity(near35(maxIdx + 1));
            currentHiddenLootBox.Inventory.Sort();
        }

        private void AddMergeOrDropItem(Item item, Transform mainTrans)
        {
            if (item == null) return;
            try
            {
                item.AgentUtilities.ReleaseActiveAgent();
                item.Detach();

                if (!ModConfigManager.GetSearchTimeKeep())
                    item.Inspected = true;

                if (!ItemUtilities.AddAndMerge(currentHiddenLootBox.Inventory, item, 0)
                    && !currentHiddenLootBox.Inventory.AddItem(item))
                {
                    Debug.LogWarning("无法添加物品到箱子: " + item.DisplayName);
                    ItemExtensions.Drop(item, mainTrans.position, true, Vector3.forward, 360f);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"AddMergeOrDropItem error for {item?.DisplayName}: {ex.Message}");
            }
        }

        private static int near35(int n)
        {
            return Math.Max(35, (n % 35 == 0) ? n : (n + 35 - n % 35));
        }

        public void OpenLootBox()
        {
            if (currentHiddenLootBox == null)
            {
                Debug.LogWarning("没有可用的隐藏箱子");
                return;
            }

            currentHiddenLootBox.gameObject.SetActive(true);

            LevelManager level = LevelManager.Instance;
            CharacterMainControl main = level != null ? level.MainCharacter : null;
            if (main == null)
            {
                Debug.Log("main is null");
                return;
            }

            main.Interact(currentHiddenLootBox);
            IsBoxOpen = true;
            OnBoxOpened?.Invoke();
            StartCoroutine(MonitorBoxClose());
        }

        private IEnumerator MonitorBoxClose()
        {
            // 等待箱子打开
            yield return new WaitWhile(() =>
                (LootView.Instance == null || !LootView.Instance.open)
                && currentHiddenLootBox != null);

            // 等待箱子关闭
            yield return new WaitWhile(() =>
                LootView.Instance != null && LootView.Instance.open
                && currentHiddenLootBox != null);

            HandleBoxClosed();
        }

        private void HandleBoxClosed()
        {
            if (!IsBoxOpen) return;

            Transform mainTransform = GetMainTransform();
            if (mainTransform == null)
            {
                Debug.LogError("Get MainTrans Failed!");
                return;
            }

            List<Item> remaining = new List<Item>();
            if (currentHiddenLootBox != null && currentHiddenLootBox.Inventory != null)
            {
                foreach (Item item in currentHiddenLootBox.Inventory)
                {
                    if (item != null) remaining.Add(item);
                }
            }

            if (currentHiddenLootBox != null
                && ModConfigManager.GetGeneratorTempTrashCan()
                && remaining.Count > ModConfigManager.GetGeneratorTempTrashCanThreshold())
            {
                Debug.Log($"箱子关闭，剩余物品数量: {remaining.Count}, 物品较多开始生成盒子");
                try
                {
                    GameObject agentGo = new GameObject("AgentItem");
                    Item agentItem = agentGo.AddComponent<Item>();
                    ItemInventoryProperty.SetValue(agentItem, currentHiddenLootBox.Inventory);

                    InteractableLootbox box = InteractableLootbox.CreateFromItem(
                        agentItem, mainTransform.position, mainTransform.rotation, true,
                        GameplayDataSettings.Prefabs.LootBoxPrefab, false);

                    LootboxDisplayNameKeyField.SetValue(box, LocalizationUtil.TempTrashCanText);
                    box.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                    ItemInventoryProperty.SetValue(agentItem, null);
                    Destroy(agentGo);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"生成临时垃圾堆失败: {ex.Message}");
                    // 降级为丢出物品
                    foreach (Item item in remaining)
                        ItemExtensions.Drop(item, mainTransform.position, true, Vector3.forward, 360f);
                }
            }
            else
            {
                Debug.Log($"箱子关闭，剩余物品数量: {remaining.Count}, 开始丢出至地上腾空箱子");
                foreach (Item item in remaining)
                {
                    ItemExtensions.Drop(item, mainTransform.position, true, Vector3.forward, 360f);
                }
                Debug.Log("丢出剩余物品完毕");
            }

            IsBoxOpen = false;
            OnBoxClosed?.Invoke();

            if (currentHiddenLootBox != null)
                currentHiddenLootBox.gameObject.SetActive(false);

            ClearLootBox();
            DestroyCurrentLootBox();
        }

        public void ClearLootBox()
        {
            if (currentHiddenLootBox != null && currentHiddenLootBox.Inventory != null)
                currentHiddenLootBox.Inventory.DestroyAllContent();
        }

        public void DestroyCurrentLootBox()
        {
            if (currentHiddenLootBox != null)
            {
                Destroy(currentHiddenLootBox.gameObject);
                currentHiddenLootBox = null;
            }
        }

        private void OnDestroy()
        {
            DestroyCurrentLootBox();
        }

        public bool IsLootViewOpen()
        {
            if (LootView.Instance != null && LootView.Instance.open) return true;
            if (currentHiddenLootBox != null) return true;
            return false;
        }

        public static Transform GetMainTransform()
        {
            LevelManager level = LevelManager.Instance;
            CharacterMainControl main = level != null ? level.MainCharacter : null;
            if (main == null)
            {
                Debug.Log("main is null");
                return null;
            }
            return main.transform;
        }

        public static IEnumerable<Item> TryGetBullets(Item item)
        {
            if (item == null) return Enumerable.Empty<Item>();

            ItemSetting_Gun gun = item.GetComponent<ItemSetting_Gun>();
            if (gun == null) return Enumerable.Empty<Item>();

            HashSet<Item> bullets = new HashSet<Item>();
            foreach (Item subItem in item.Inventory)
            {
                if (subItem != null && subItem.GetBool("IsBullet", false))
                    bullets.Add(subItem);
            }

            if (GunBulletProperty != null)
                GunBulletProperty.SetValue(gun, 0);

            return bullets.ToList();
        }

        public static IEnumerable<Item> TryGetSlotItems(Item item)
        {
            if (item == null) return Enumerable.Empty<Item>();

            SlotCollection slots = item.Slots;
            if (slots == null) return Enumerable.Empty<Item>();

            HashSet<Item> slotItems = new HashSet<Item>();
            foreach (Slot slot in slots)
            {
                Item removed = slot.Unplug();
                if (removed != null) slotItems.Add(removed);
            }
            return slotItems.ToList();
        }
    }
}
