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

        private float lastHKeyPressTime = 0f;
        private float lastBubbleTime = 0f;

        void OnEnable()
        {
            // 初始化配置
            ModConfig.Init(ModManager.DefaultModFolderPath);
        }


        void OnDisable()
        {

        }

        void Update()
        {
            // 检测按键按下
            KeyCode hotKey = ModConfig.GetSearchKeyCode();
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
                List<Item> targetItems = SearchItemAround(DEFAULT_SEARCH_RADIUS, true);
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

                        // 扩大检索范围3倍，确认下附近有没有可拾取的物品
                        if (SearchItemAround(DEFAULT_SEARCH_RADIUS * 3f, false).Count > 0)
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

        public static List<Item> SearchItemAround(float radius, bool forLoot)
        {
            Collider[] colliders = new Collider[100];
            LayerMask interactLayers = 1 << LayerMask.NameToLayer("Interactable");
            CharacterMainControl? main = LevelManager.Instance?.MainCharacter;

            if (null == main || !main.IsMainCharacter)
            {
                return new List<Item>();
            }

            int num = Physics.OverlapSphereNonAlloc(main.transform.position + Vector3.up * 0.5f + main.CurrentAimDirection * 0.2f, radius, colliders, interactLayers);
            if (num <= 0)
            {
                return new List<Item>();
            }

            HashSet<Item> uniqueItems = new HashSet<Item>();
            for (int i = 0; i < num; i++)
            {
                Collider collider = colliders[i];
                if (ModConfig.GetSearchContainers())
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
                            if (forLoot)
                            {
                                tmpBox.SetMarkerUsed();
                                tmpBox.needInspect = false;
                            }
                        }
                    }

                }
                InteractablePickup tmpPickup = collider.GetComponent<InteractablePickup>();
                if (null != tmpPickup)
                {
                    uniqueItems.Add(tmpPickup.ItemAgent.Item);
                }
            }
            return uniqueItems.ToList();
        }

    }
}