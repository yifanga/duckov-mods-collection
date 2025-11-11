using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using Duckov.UI;
using System.Reflection;
using System.Linq;
using ItemStatsSystem.Items;
using Duckov.Utilities;

namespace LootNearbyItem
{


    public class DynamicLootBoxManager : MonoBehaviour
    {

        public const int GENERATOR_TEMP_TRASH_CAN_THRESHOLD = 15;
        // 单例模式
        public static DynamicLootBoxManager Instance { get; private set; }

        public static FieldInfo LootboxDisplayNameKeyField = typeof(InteractableLootbox).GetField("displayNameKey",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo LootboxShowSortButtonField = typeof(InteractableLootbox).GetField("showSortButton",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static FieldInfo LootboxBaseOtherInterablesInGroupField = typeof(InteractableBase).GetField("otherInterablesInGroup",
                BindingFlags.NonPublic | BindingFlags.Instance);

        // 获取私有属性信息
        private static PropertyInfo GunBulletProperty = typeof(ItemSetting_Gun).GetProperty("bulletCount",
                BindingFlags.NonPublic | BindingFlags.Instance);

        // 获取Inventory属性（包含非公共成员）
        PropertyInfo ItemInventoryProperty = typeof(Item).GetProperty("Inventory", 
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        // 当前隐藏箱子
        private InteractableLootbox currentHiddenLootBox;

        // 箱子状态
        public bool IsBoxOpen { get; private set; }
        public Inventory CurrentBoxInventory => currentHiddenLootBox?.Inventory;

        // 事件回调
        public event Action OnBoxOpened;
        public event Action OnBoxClosed;

        private void Awake()
        {
            // 单例设置
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

        // 创建新的隐藏箱子
        public void CreateNewHiddenLootBox()
        {
            // 清理旧箱子
            if (currentHiddenLootBox != null)
            {
                Destroy(currentHiddenLootBox.gameObject);
            }

            // 使用预制体创建lootbox
            currentHiddenLootBox = Instantiate(InteractableLootbox.Prefab);
            currentHiddenLootBox.transform.SetParent(this.transform);

            if (currentHiddenLootBox == null)
            {
                // 创建新箱子对象
                GameObject lootBoxObject = new GameObject("DynamicHiddenLootBox");
                lootBoxObject.SetActive(false);
                // 附加到当前单例上
                lootBoxObject.transform.SetParent(this.transform);
                // 添加必要组件
                currentHiddenLootBox = lootBoxObject.AddComponent<InteractableLootbox>();
            }
            

            //设置名称
            LootboxDisplayNameKeyField?.SetValue(currentHiddenLootBox, LocalizationUtil.ScatteredObjectsText);
            LootboxShowSortButtonField?.SetValue(currentHiddenLootBox, true);
            LootboxBaseOtherInterablesInGroupField?.SetValue(currentHiddenLootBox, new List<InteractableBase>());
        }

        // 配置箱子加载器


        // 添加以下两个辅助方法到类中
        public static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        public static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            if (field != null)
            {
                return (T)field.GetValue(target);
            }

            return default(T);
        }

        // 向箱子添加物品（动态传入物品列表）
        public async UniTask AddItemsToBox(List<Item> items)
        {
            Transform? mainTrans = GetMainTransform();
            if (mainTrans == null)
            {
                Debug.LogError("Get MainTrans Failed!");
                return;
            }

            if (currentHiddenLootBox == null)
            {
                CreateNewHiddenLootBox();
            }

            // 确保箱子库存已创建
            if (currentHiddenLootBox.Inventory == null)
            {
                await UniTask.WaitUntil(() => currentHiddenLootBox.Inventory != null);
            }
            if (currentHiddenLootBox == null || currentHiddenLootBox.Inventory == null)
            {
                Debug.LogError("Create HiddenLootBox Failed!");
                return;
            }

            // 调整容量
            int n = items.Count;

            // 预估容量增大，避免拆卸子弹导致添加物品失败
            currentHiddenLootBox.Inventory.SetCapacity(near35(n + 70));

            // 再添加新物品
            foreach (var item in items)
            {
                if (item == null) continue;

                // 尝试拆分出子弹
                var bullets = TryGetBullets(item);
                foreach (var bullet in bullets)
                {
                    // 添加到库存
                    AddMergeOrDropItem(bullet, mainTrans);
                }

                //如果开启自动拆卸插槽，则尝试拆卸
                if (ModConfigManager.GetAutoUnplugSlots())
                {
                    var slotItems = TryGetSlotItems(item);
                    foreach (var slotItem in slotItems)
                    {
                        AddMergeOrDropItem(slotItem, mainTrans);
                    }
                }

                // 添加到库存
                AddMergeOrDropItem(item, mainTrans);
            }
            // 添加完毕后，由于物品堆叠，需要压缩一下Inventory容量
            int maxIdx = currentHiddenLootBox.Inventory.GetLastItemPosition();
            currentHiddenLootBox.Inventory.SetCapacity(near35(maxIdx + 1));
            // 自动整理物品
            currentHiddenLootBox.Inventory.Sort();
        }

        private void AddMergeOrDropItem(Item item, Transform mainTrans)
        {
            // 必要的前置清理
            item.AgentUtilities.ReleaseActiveAgent();
            item.Detach();
            // 如果不保留搜索时间，则将物品全部设置为已搜索过
            if (!ModConfigManager.GetSearchTimeKeep())
            {
                item.Inspected = true;
            }
            // 添加到库存
            if (!currentHiddenLootBox.Inventory.AddAndMerge(item))
            {
                if (!currentHiddenLootBox.Inventory.AddItem(item))
                {
                    Debug.LogWarning($"无法添加物品到箱子: {item.DisplayName}");
                    item.Drop(mainTrans.position, createRigidbody: true, Vector3.forward, 360f);
                }
            }
        }

        private static int near35(int n)
        {
            return Math.Max(35, n % 35 == 0 ? n : n + 35 - n % 35);
        }

        // 打开箱子（显示战利品界面）
        public void OpenLootBox()
        {
            if (currentHiddenLootBox == null)
            {
                Debug.LogWarning("没有可用的隐藏箱子");
                return;
            }

            // 激活箱子
            currentHiddenLootBox.gameObject.SetActive(true);

            // 触发战利品界面
            CharacterMainControl? main = LevelManager.Instance?.MainCharacter;
            if (main == null)
            {
                Debug.Log("main is null");
                return;
            }
            // 打开箱子
            main.Interact(currentHiddenLootBox);

            // 标记状态
            IsBoxOpen = true;

            // 触发事件
            OnBoxOpened?.Invoke();

            // 开始监听关闭事件
            StartCoroutine(MonitorBoxClose());
        }

        // 监听箱子关闭
        private System.Collections.IEnumerator MonitorBoxClose()
        {
            // 等待箱子开启(如果已开启，此步会直接跳过)
            yield return new WaitWhile(() =>
                (LootView.Instance == null || !LootView.Instance.open) &&
                currentHiddenLootBox != null);
            // 等待箱子关闭
            yield return new WaitWhile(() =>
                LootView.Instance != null && LootView.Instance.open &&
                currentHiddenLootBox != null);

            // 处理箱子关闭
            HandleBoxClosed();
        }

        // 处理箱子关闭
        private void HandleBoxClosed()
        {
            if (!IsBoxOpen) return;

            Transform? mainTrans = GetMainTransform();
            if (mainTrans == null)
            {
                Debug.LogError("Get MainTrans Failed!");
                return;
            }

            // 获取剩余物品
            List<Item> remainItems = new List<Item>();
            if (currentHiddenLootBox != null && currentHiddenLootBox.Inventory != null)
            {
                foreach (var item in currentHiddenLootBox.Inventory)
                {
                    if (item != null)
                    {
                        remainItems.Add(item);
                    }
                }
            }
            // 实验性功能，支持临时生成垃圾堆
            if (null != currentHiddenLootBox && ModConfigManager.GetGeneratorTempTrashCan()
                    && remainItems.Count > GENERATOR_TEMP_TRASH_CAN_THRESHOLD)
            {
                Debug.Log($"箱子关闭，剩余物品数量: {remainItems.Count}, 物品较多开始生成盒子");
                // 临时构造item用于初始化lootbox
                GameObject itemObject = new GameObject("AgentItem");
                Item agentItem = itemObject.AddComponent<Item>();
                ItemInventoryProperty.SetValue(agentItem, currentHiddenLootBox.Inventory);

                InteractableLootbox tmpBox = InteractableLootbox.CreateFromItem(agentItem, mainTrans.position, mainTrans.rotation, moveToMainScene: true, GameplayDataSettings.Prefabs.LootBoxPrefab, filterDontDropOnDead: false);
                LootboxDisplayNameKeyField.SetValue(tmpBox, LocalizationUtil.TempTrashCanText);
                // 放大1.5倍做区分
                tmpBox.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                // 清理临时构造的item
                ItemInventoryProperty.SetValue(agentItem, null);
                Destroy(itemObject);
            }
            else
            {
                Debug.Log($"箱子关闭，剩余物品数量: {remainItems.Count}, 开始丢出至地上腾空箱子");
                foreach (var item in remainItems)
                {
                    // Debug.Log($"丢出剩余物品: {item.DisplayName}");
                    item.Drop(mainTrans.position, createRigidbody: true, Vector3.forward, 360f);
                }
                Debug.Log($"丢出剩余物品完毕");
            }

            // 标记状态
            IsBoxOpen = false;

            // 触发关闭事件
            OnBoxClosed?.Invoke();

            // 隐藏箱子
            if (currentHiddenLootBox != null)
            {
                currentHiddenLootBox.gameObject.SetActive(false);
            }
            ClearLootBox();
            DestroyCurrentLootBox();
        }

        // 清空箱子
        public void ClearLootBox()
        {
            if (currentHiddenLootBox != null && currentHiddenLootBox.Inventory != null)
            {
                currentHiddenLootBox.Inventory.DestroyAllContent();
            }
        }

        // 销毁当前箱子
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

        // 检查战利品界面是否已打开
        public bool IsLootViewOpen()
        {
            // 方法1：使用LootView单例
            if (LootView.Instance != null && LootView.Instance.open)
            {
                return true;
            }
            if (currentHiddenLootBox != null)
            {
                return true;
            }
            return false;
        }

        public static Transform? GetMainTransform()
        {
            CharacterMainControl? main = LevelManager.Instance?.MainCharacter;
            if (main == null)
            {
                Debug.Log($"main is null");
                return null;
            }
            return main.transform;
        }

        public static IEnumerable<Item> TryGetBullets(Item item)
        {
            if (null == item)
            {
                return Enumerable.Empty<Item>();
            }
            var gunSetting = item.GetComponent<ItemSetting_Gun>();
            if (null == gunSetting)
            {
                return Enumerable.Empty<Item>();
            }
            // 获取子弹Item（每个Item可能包含多个堆叠）
            var bullets = new HashSet<Item>();
            foreach (var subItem in item.Inventory)
            {
                if (subItem != null && subItem.GetBool("IsBullet"))
                {
                    bullets.Add(subItem);
                }
            }
            // 调用枪的子弹数更新函数，更新缓存
            GunBulletProperty.SetValue(gunSetting, 0);
            return bullets.ToList();
        }

        public static IEnumerable<Item> TryGetSlotItems(Item item)
        {
            if (null == item)
            {
                return Enumerable.Empty<Item>();
            }
            var itemSlots = item.Slots;
            if (null == itemSlots)
            {
                return Enumerable.Empty<Item>();
            }
            // 获取插槽内的内容
            var slotSubItems = new HashSet<Item>();
            foreach (Slot slot in itemSlots)
            {
                Item slotItem = slot.Unplug();
                if (slotItem != null)
                {
                    slotSubItems.Add(slotItem);
                }
            }
            return slotSubItems.ToList();
        }

    }
}