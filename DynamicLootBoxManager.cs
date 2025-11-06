using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using Duckov.UI;
using System.Reflection;

namespace LootNearbyItem
{


    public class DynamicLootBoxManager : MonoBehaviour
    {
        // 单例模式
        public static DynamicLootBoxManager Instance { get; private set; }

        public static FieldInfo LootboxDisplayNameKeyField = typeof(InteractableLootbox).GetField("displayNameKey",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static FieldInfo LootboxShowSortButtonField = typeof(InteractableLootbox).GetField("showSortButton",
                BindingFlags.NonPublic | BindingFlags.Instance);

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

            // 创建新箱子对象
            GameObject lootBoxObject = new GameObject("DynamicHiddenLootBox");
            lootBoxObject.SetActive(false);

            // 附加到当前单例上
            lootBoxObject.transform.SetParent(this.transform);

            // 添加必要组件
            currentHiddenLootBox = lootBoxObject.AddComponent<InteractableLootbox>();

            //设置名称
            // SetPrivateField(currentHiddenLootBox, "displayNameKey", LocalizationUtil.ScatteredObjectsText);
            LootboxDisplayNameKeyField?.SetValue(currentHiddenLootBox, LocalizationUtil.ScatteredObjectsText);
            // SetPrivateField(currentHiddenLootBox, "showSortButton", true);
            LootboxShowSortButtonField?.SetValue(currentHiddenLootBox, true);
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
            if(mainTrans == null)
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
            currentHiddenLootBox.Inventory.SetCapacity(Math.Max(35, n % 35 == 0 ? n : n + 35 - n % 35));

            // 再添加新物品
            foreach (var item in items)
            {
                if (item == null) continue;
                // 物品移动到箱子
                item.AgentUtilities.ReleaseActiveAgent();
                item.Detach();
                item.Inspected = true;

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
            CallStartLootMethod();

            // 标记状态
            IsBoxOpen = true;

            // 触发事件
            OnBoxOpened?.Invoke();

            // 开始监听关闭事件
            StartCoroutine(MonitorBoxClose());
        }

        private void CallStartLootMethod()
        {
            try
            {
                // 使用反射获取 StartLoot 方法
                var startLootMethod = typeof(InteractableLootbox).GetMethod(
                    "StartLoot",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (startLootMethod != null)
                {
                    // 调用 StartLoot 方法
                    startLootMethod.Invoke(currentHiddenLootBox, null);
                }
                else
                {
                    Debug.LogError("无法找到 StartLoot 方法");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"调用 StartLoot 失败: {ex.Message}");
            }
        }

        // 监听箱子关闭
        private System.Collections.IEnumerator MonitorBoxClose()
        {
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
            if(mainTrans == null)
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

            Debug.Log($"箱子关闭，剩余物品数量: {remainItems.Count}, 开始丢出至地上腾空箱子");
            foreach (var item in remainItems)
            {
                Debug.Log($"丢出剩余物品: {item.DisplayName}");
                item.Drop(mainTrans.position, createRigidbody: true, Vector3.forward, 360f);
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

    }
}