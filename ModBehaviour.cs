using ItemStatsSystem;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using Duckov.UI.DialogueBubbles;
using Duckov.Modding;
using System;

namespace LootNearbyItem
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static readonly float KEY_DEBOUNCE_TIME = 0.5f; // 防抖时间500毫秒
        private static readonly float BUBBLES_TIME = 1.5f; // 气泡防抖时间500毫秒
        private static readonly float DEFAULT_SEARCH_RADIUS = 0.3f; // 默认搜索半径0.3m
        private static readonly int MAX_SEARCH_COUNT = 165; // 默认最大搜索物品数量（基本是5页，每页一般35个）

        private float lastHKeyPressTime = 0f;
        private float lastBubbleTime = 0f;

        void OnEnable()
        {
            // 初始化配置
            ModConfigManager.Init(ModManager.DefaultModFolderPath);
            // 监听配置，并随着配置更改随时保存
            ModManager.OnModActivated += ModConfigManager.OnModConfigMenuActivated;

            // 立即检查一次，防止 ModConfig 已经加载但事件错过了
            if (ModConfigAPI.IsAvailable())
            {
                Debug.Log("LootNearbyItem: ModConfig already available!");
                ModConfigManager.SetupModConfig();
                ModConfigManager.LoadConfigFromModConfig();
            }
        }


        void OnDisable()
        {
            // 清理监听配置
            ModManager.OnModActivated += ModConfigManager.OnModConfigMenuActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(ModConfigManager.OnModConfigOptionsChanged);
        }

        void Update()
        {
            // 检测按键按下
            KeyCode hotKey = ModConfigManager.GetSearchKeyCode();
            if (Input.GetKeyDown(hotKey))
            {
                // 防抖检查 - 防止连续触发
                if (Time.time - lastHKeyPressTime < KEY_DEBOUNCE_TIME)
                {
                    Debug.Log("按键触发过于频繁，已忽略");
                    return;
                }
                // 更新最后按键时间
                lastHKeyPressTime = Time.time;
                Debug.Log($"{hotKey} key pressed!");
                // 检查是否已有战利品界面打开
                if (null != DynamicLootBoxManager.Instance && DynamicLootBoxManager.Instance.IsLootViewOpen())
                {
                    Debug.Log("战利品界面已打开，忽略新请求");
                    return;
                }

                // 执行战利品或掉落物搜索逻辑
                List<Item> targetItems = SearchItemAroundForLoot(DEFAULT_SEARCH_RADIUS, ModConfigManager.GetSearchContainers(), ModConfigManager.GetSearchContainersRadius());
                // 添加初始物品
                if (targetItems.Count > 0)
                {
                    GenerateAndOpenRandomLoot(targetItems);
                }
                else
                {
                    //人物吐气泡说：不要找啦，周围没有散落物！
                    Transform? mainTrans = DynamicLootBoxManager.GetMainTransform();
                    if (null != mainTrans)
                    {
                        if (Time.time - lastBubbleTime < BUBBLES_TIME)
                        {
                            Debug.Log("气泡触发过于频繁，已忽略");
                            return;
                        }
                        // 更新最后气泡时间
                        lastBubbleTime = Time.time;

                        // 扩大检索范围5倍，确认下附近有没有可拾取的物品
                        if (SearchItemAroundForNotify(DEFAULT_SEARCH_RADIUS * 5f, ModConfigManager.GetSearchContainers(),
                                ModConfigManager.GetSearchContainersRadius() + DEFAULT_SEARCH_RADIUS * 4f))
                        {
                            DialogueBubblesManager.Show(LocalizationUtil.ItemOutOfRangeText, mainTrans, speed: 100f, duration: 1.2f);
                        }
                        else
                        {
                            DialogueBubblesManager.Show(LocalizationUtil.NoScatteredObjectsText, mainTrans, speed: 100f, duration: 1.2f);
                        }
                    }
                }
            }
        }

        private async void GenerateAndOpenRandomLoot(List<Item> randomItems)
        {
            if (null == DynamicLootBoxManager.Instance)
            {
                Debug.Log("创建DynamicLootBoxManager!");
                LevelManager.Instance.transform.AddComponent<DynamicLootBoxManager>();
            }
            if (null == DynamicLootBoxManager.Instance)
            {
                Debug.Log("创建DynamicLootBoxManager失败!");
                return;
            }
            Debug.Log("创建新箱子!");
            // 创建新箱子
            DynamicLootBoxManager.Instance.CreateNewHiddenLootBox();

            // 添加物品
            Debug.Log("添加物品!");
            await DynamicLootBoxManager.Instance.AddItemsToBox(randomItems);

            // 打开箱子
            Debug.Log("打开箱子!");
            DynamicLootBoxManager.Instance.OpenLootBox();

            // 注册关闭事件
            Debug.Log("注册关闭事件!");
            DynamicLootBoxManager.Instance.OnBoxClosed += HandleBoxClosed;
        }

        private void HandleBoxClosed()
        {
            Debug.Log($"箱子已经关闭，剩余物品已经丢出，箱子即将销毁");
            DynamicLootBoxManager.Instance.OnBoxClosed -= HandleBoxClosed;
        }

        public static List<Item> SearchItemAroundForLoot(float pickupRadius, bool enableLootbox, float lootboxRadius)
        {
            Debug.Log($"search for loot pickupRadius {pickupRadius} enableLootbox {enableLootbox} lootboxRadius  {lootboxRadius}");
            // 为应对极端场景，最大匹配数量提高到1000，然后取最近的大概不到175个物品
            Collider[] colliders = new Collider[1000];
            LayerMask interactLayers = 1 << LayerMask.NameToLayer("Interactable");
            CharacterMainControl? main = LevelManager.Instance?.MainCharacter;

            if (null == main || !main.IsMainCharacter)
            {
                return new List<Item>();
            }
            float searchRadius = enableLootbox ? Math.Max(pickupRadius, lootboxRadius) : pickupRadius;
            Vector3 mainPosition = main.transform.position + Vector3.up * 0.5f + main.CurrentAimDirection * 0.2f;

            // 实际搜索到的碰撞体数量
            int num = Physics.OverlapSphereNonAlloc(mainPosition, searchRadius, colliders, interactLayers);
            if (num <= 0)
            {
                return new List<Item>();
            }

            // 逐个计算距离和排序
            float[] distances = new float[num];
            for (int i = 0; i < num; i++)
            {
                Collider collider = colliders[i];
                float distance = Vector3.Distance(mainPosition, collider.ClosestPoint(mainPosition));
                distances[i] = distance;
                Debug.Log("collider distance: " + distance);
            }
            Array.Sort(distances, colliders, 0, num);

            HashSet<Item> uniqueItems = new HashSet<Item>();
            // 从近到远遍历处理可交互物品
            for (int i = 0; i < num; i++)
            {
                Collider collider = colliders[i];
                float distance = distances[i];
                if (distance <= pickupRadius)
                {
                    InteractablePickup tmpPickup = collider.GetComponent<InteractablePickup>();
                    if (null != tmpPickup)
                    {
                        uniqueItems.Add(tmpPickup.ItemAgent.Item);
                    }
                }

                if (enableLootbox && distance < lootboxRadius)
                {
                    InteractableLootbox tmpBox = collider.GetComponent<InteractableLootbox>();
                    if (null != tmpBox)
                    {
                        string nameKey = (string)DynamicLootBoxManager.LootboxDisplayNameKeyField.GetValue(tmpBox);
                        Debug.Log($"find loot box name key {nameKey}");
                        // 只处理击杀掉落的战利品
                        if ("UI_LootBox_Loot".Equals(nameKey))
                        {
                            foreach (var item in tmpBox.Inventory)
                            {
                                if (null != item)
                                {
                                    uniqueItems.Add(item);
                                }
                            }
                            // 如果后续是为了拾取，则提前标记好箱子状态为已搜索
                            tmpBox.SetMarkerUsed();
                            tmpBox.needInspect = false;
                        }
                    }
                }
                // 如果超出了单次搜索数量，提前结束搜索
                if (uniqueItems.Count >= MAX_SEARCH_COUNT)
                {
                    break;
                }
            }
            return uniqueItems.ToList();
        }

        

        public static bool SearchItemAroundForNotify(float pickupRadius, bool enableLootbox, float lootboxRadius)
        {
            Debug.Log($"search for notify pickupRadius {pickupRadius} enableLootbox {enableLootbox} lootboxRadius  {lootboxRadius}");
            Collider[] colliders = new Collider[100];
            LayerMask interactLayers = 1 << LayerMask.NameToLayer("Interactable");
            CharacterMainControl? main = LevelManager.Instance?.MainCharacter;

            if (null == main || !main.IsMainCharacter)
            {
                return false;
            }
            float searchRadius = enableLootbox ? Math.Max(pickupRadius, lootboxRadius) : pickupRadius;
            Vector3 mainPosition = main.transform.position + Vector3.up * 0.5f + main.CurrentAimDirection * 0.2f;
            int num = Physics.OverlapSphereNonAlloc(mainPosition, searchRadius, colliders, interactLayers);
            if (num <= 0)
            {
                return false;
            }

            HashSet<Item> uniqueItems = new HashSet<Item>();
            for (int i = 0; i < num; i++)
            {
                Collider collider = colliders[i];
                float distance = Vector3.Distance(mainPosition, collider.ClosestPoint(mainPosition));
                if (enableLootbox && distance < lootboxRadius)
                {
                    InteractableLootbox tmpBox = collider.GetComponent<InteractableLootbox>();
                    if (null != tmpBox)
                    {
                        string nameKey = (string)DynamicLootBoxManager.LootboxDisplayNameKeyField.GetValue(tmpBox);
                        Debug.Log($"find loot box name key {nameKey}");
                        // 只处理击杀掉落的战利品
                        if ("UI_LootBox_Loot".Equals(nameKey))
                        {
                            foreach (var item in tmpBox.Inventory)
                            {
                                if (null != item)
                                {
                                    uniqueItems.Add(item);
                                }
                            }
                        }
                    }
                }
                if (distance <= pickupRadius)
                {
                    InteractablePickup tmpPickup = collider.GetComponent<InteractablePickup>();
                    if (null != tmpPickup)
                    {
                        uniqueItems.Add(tmpPickup.ItemAgent.Item);
                    }
                }
                if (uniqueItems.Count > 0)
                {
                    return true;
                }

            }
            return uniqueItems.Count > 0;
        }

    }
}