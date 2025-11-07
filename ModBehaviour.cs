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

            ModManager.OnModActivated += ModManager_OnModActivated;
            ModManager.OnModWillBeDeactivated += ModManager_OnModWillBeDeactivated;
        }


        void OnDisable()
        {
            ModManager.OnModActivated -= ModManager_OnModActivated;
            ModManager.OnModWillBeDeactivated -= ModManager_OnModWillBeDeactivated;

            // 记录当前配置并保存
            ModConfig.SaveConfig(ModManager.DefaultModFolderPath);
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
                List<Item> targetItems = SearchItemAround(DEFAULT_SEARCH_RADIUS, ModConfig.GetSearchContainers(), ModConfig.GetSearchRadius(), true);
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
                        if (SearchItemAround(DEFAULT_SEARCH_RADIUS * 5f, ModConfig.GetSearchContainers(), ModConfig.GetSearchRadius(), false).Count > 0)
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

        public static List<Item> SearchItemAround(float pickupRadius, bool enableLootbox, float lootboxRadius, bool forLoot)
        {
            Collider[] colliders = new Collider[100];
            LayerMask interactLayers = 1 << LayerMask.NameToLayer("Interactable");
            CharacterMainControl? main = LevelManager.Instance?.MainCharacter;

            if (null == main || !main.IsMainCharacter)
            {
                return new List<Item>();
            }
            float searchRadius = enableLootbox ? Math.Max(pickupRadius, lootboxRadius) : pickupRadius;
            Vector3 mainPosition = main.transform.position + Vector3.up * 0.5f + main.CurrentAimDirection * 0.2f;
            int num = Physics.OverlapSphereNonAlloc(mainPosition, searchRadius, colliders, interactLayers);
            if (num <= 0)
            {
                return new List<Item>();
            }

            HashSet<Item> uniqueItems = new HashSet<Item>();
            for (int i = 0; i < num; i++)
            {
                Collider collider = colliders[i];
                float distance = Vector3.Distance(mainPosition, collider.transform.position);
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
                            if (forLoot)
                            {
                                tmpBox.SetMarkerUsed();
                                tmpBox.needInspect = false;
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
            }
            return uniqueItems.ToList();
        }

        private void AddUI()
        {
            ModSettingAPI.AddKeybinding("K1", "一键搜索散落物快捷键", ModConfig.GetSearchKeyCode(), ModConfig.SetSearchKeyCode);
            ModSettingAPI.AddToggle("T1", "是否搜索战利品容器(击杀掉落的容器)", ModConfig.GetSearchContainers(), ModConfig.SetSearchContainers);

            ModSettingAPI.AddSlider("S1", "搜索战利品容器的半径(单位m)", ModConfig.GetSearchRadius(), new Vector2(0.3f, 20f), ModConfig.SetSearchRadius);
            // ModSettingAPI.AddButton("B1", "恢复所有默认值","重置",Reset);

        }
        private void Reset()
        {
            //注意：SetValue只是单方面通知UI设置值,也就是说UI的onValueChange不会被调用
            //如果需要同步，应该先设置此mod的值，再将此mod的值设置给ModSetting。如：Dropdown1这样，其余的都只改变了UI的值并没有改变此mod的值。

            ModSettingAPI.SetValue("K1", KeyCode.H);
            ModSettingAPI.SetValue("T1", false);
            ModSettingAPI.SetValue("S1", 10f);
        }

        private void ModManager_OnModWillBeDeactivated(ModInfo arg1, Duckov.Modding.ModBehaviour arg2)
        {
            if (arg1.name != ModSettingAPI.MOD_NAME || !ModSettingAPI.Init(info)) return;
            //禁用ModSetting的时候移除监听
            // Setting.OnSlider1ValueChanged -= Setting_OnSlider1ValueChanged;
        }

        //下面两个函数需要实现，实现后的效果是：ModSetting和mod之间不需要启动顺序，两者无论谁先启动都能正常添加设置
        private void ModManager_OnModActivated(ModInfo arg1, Duckov.Modding.ModBehaviour arg2)
        {
            if (arg1.name != ModSettingAPI.MOD_NAME || !ModSettingAPI.Init(info)) return;
            //(触发时机:此mod在ModSetting之前启用)检查启用的mod是否是ModSetting,是进行初始化
            AddUI();
        }
        protected override void OnAfterSetup()
        {
            //(触发时机:此mod在ModSetting之后启用)此mod，Setup后,尝试进行初始化
            if (ModSettingAPI.Init(info)) AddUI();
        }

    }
}