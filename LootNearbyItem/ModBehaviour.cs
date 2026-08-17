using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Duckov;
using Duckov.Modding;
using Duckov.UI;
using Duckov.UI.DialogueBubbles;
using ItemStatsSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace LootNearbyItem
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static FieldInfo DeadBodyManagerDeathInfosField = typeof(DeadBodyManager).GetField("deaths", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly float KEY_DEBOUNCE_TIME = 0.5f;
        private static readonly float BUBBLES_TIME = 1.5f;
        private static readonly float DEFAULT_SEARCH_RADIUS = 0.3f;
        private static readonly int MAX_SEARCH_COUNT = 165;

        private float lastHKeyPressTime = 0f;
        private float lastBubbleTime = 0f;

        private static List<InteractableLootbox> CacheLootBoxes = new List<InteractableLootbox>();
        private static List<InteractableLootbox> CacheTombBoxes = new List<InteractableLootbox>();

        private void OnEnable()
        {
            ModConfigManager.Init(ModManager.DefaultModFolderPath);
            ModManager.OnModActivated += ModConfigManager.OnModConfigMenuActivated;
            ModManager.OnModActivated += DynamicHarmonyPatcher.OnModConfigMenuActivated;

            DynamicHarmonyPatcher.Initialize();
            Debug.Log("[LootNearbyItem] mod enabled! 当前快捷键: " + ModConfigManager.GetSearchKeyCode()
                + ", 搜索土堆: " + ModConfigManager.GetSearchHiddenContainers());
        }

        protected override void OnAfterSetup()
        {
            base.OnAfterSetup();

            // 优先使用ModSetting（参考CombatMaid，在OnAfterSetup中通过this.info初始化）
            if (ModSettingAPI.Init(this.info))
            {
                Debug.Log("[LootNearbyItem] 使用ModSetting配置界面");
                ModConfigManager.SetupModSetting();
            }
            else if (ModConfigAPI.IsAvailable())
            {
                Debug.Log("[LootNearbyItem] ModSetting不可用，使用ModConfig配置界面（本地config.txt优先）");
                ModConfigManager.SetupModConfig();
            }
        }

        private void OnDisable()
        {
            ModManager.OnModActivated -= ModConfigManager.OnModConfigMenuActivated;
            ModManager.OnModActivated -= DynamicHarmonyPatcher.OnModConfigMenuActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(ModConfigManager.OnModConfigOptionsChanged);
            DynamicHarmonyPatcher.RemovePatch();
        }

        private void Update()
        {
            KeyCode searchKeyCode = ModConfigManager.GetSearchKeyCode();
            if (!Input.GetKeyDown(searchKeyCode)) return;

            Debug.Log($"[LootNearbyItem] 按键触发: {searchKeyCode}, pickupRadius={ModConfigManager.GetSearchPickupRadius()}, searchContainers={ModConfigManager.GetSearchContainers()}, searchOtherContainers={ModConfigManager.GetSearchOtherContainers()}");

            if (Time.time - lastHKeyPressTime < KEY_DEBOUNCE_TIME)
            {
                Debug.Log("[LootNearbyItem] 按键触发过于频繁，已忽略");
                return;
            }
            lastHKeyPressTime = Time.time;

            if (DynamicLootBoxManager.Instance != null && DynamicLootBoxManager.Instance.IsLootViewOpen())
            {
                Debug.Log("战利品界面已打开，忽略新请求");
                return;
            }

            List<Item> items = SearchItemAroundForLoot(
                ModConfigManager.GetSearchPickupRadius(),
                ModConfigManager.GetSearchContainers(),
                ModConfigManager.GetSearchContainersRadius(),
                ModConfigManager.GetSearchOtherContainers(),
                ModConfigManager.GetSearchOtherContainersRadius(),
                ModConfigManager.GetSearchHiddenContainers(),
                ModConfigManager.GetSearchHiddenContainersRadius());

            if (items.Count > 0)
            {
                GenerateAndOpenRandomLoot(items).Forget();
                return;
            }

            Transform mainTransform = DynamicLootBoxManager.GetMainTransform();
            if (mainTransform == null) return;

            if (Time.time - lastBubbleTime < BUBBLES_TIME)
            {
                Debug.Log("气泡触发过于频繁，已忽略");
                return;
            }
            lastBubbleTime = Time.time;

            bool hasNearby = SearchItemAroundForNotify(
                ModConfigManager.GetSearchPickupRadius() + DEFAULT_SEARCH_RADIUS * 4f,
                ModConfigManager.GetSearchContainers(),
                ModConfigManager.GetSearchContainersRadius() + DEFAULT_SEARCH_RADIUS * 4f,
                ModConfigManager.GetSearchOtherContainers(),
                ModConfigManager.GetSearchOtherContainersRadius() + DEFAULT_SEARCH_RADIUS * 4f,
                ModConfigManager.GetSearchHiddenContainers(),
                ModConfigManager.GetSearchHiddenContainersRadius() + DEFAULT_SEARCH_RADIUS * 4f);

            if (hasNearby)
                DialogueBubblesManager.Show(LocalizationUtil.ItemOutOfRangeText, mainTransform, -1f, false, false, 100f, 1.2f).Forget();
            else
                DialogueBubblesManager.Show(LocalizationUtil.NoScatteredObjectsText, mainTransform, -1f, false, false, 100f, 1.2f).Forget();
        }

        private async UniTaskVoid GenerateAndOpenRandomLoot(List<Item> randomItems)
        {
            if (DynamicLootBoxManager.Instance == null)
            {
                Debug.Log("创建DynamicLootBoxManager!");
                ComponentHolderProtocol.AddComponent<DynamicLootBoxManager>(LevelManager.Instance.transform);
            }

            if (DynamicLootBoxManager.Instance == null)
            {
                Debug.LogError("创建DynamicLootBoxManager失败!");
                return;
            }

            Debug.Log("创建新箱子!");
            DynamicLootBoxManager.Instance.CreateNewHiddenLootBox();

            Debug.Log("添加物品!");
            await DynamicLootBoxManager.Instance.AddItemsToBox(randomItems);

            Debug.Log("打开箱子!");
            DynamicLootBoxManager.Instance.OpenLootBox();

            Debug.Log("注册关闭事件!");
            DynamicLootBoxManager.Instance.OnBoxClosed += HandleBoxClosed;
        }

        private void HandleBoxClosed()
        {
            Debug.Log("箱子已经关闭，剩余物品已经丢出，箱子即将销毁");

            if (CacheTombBoxes != null && CacheTombBoxes.Count > 0)
            {
                foreach (InteractableLootbox tombBox in CacheTombBoxes)
                    TryFindDeathInfoAndTouch(tombBox);
            }

            if (CacheLootBoxes != null && CacheLootBoxes.Count > 0)
            {
                foreach (InteractableLootbox lootBox in CacheLootBoxes)
                {
                    if (lootBox != null)
                        lootBox.InternalStopInteract();
                }
                CacheLootBoxes.Clear();
            }

            DynamicLootBoxManager.Instance.OnBoxClosed -= HandleBoxClosed;
        }

        public static List<Item> SearchItemAroundForLoot(float pickupRadius, bool enableEnemyLootbox, float enemyLootboxRadius, bool enableOtherLootbox, float otherLootboxRadius, bool enableHiddenBox = false, float hiddenBoxRadius = 3f)
        {
            Debug.Log($"LootNearbyItem search for loot pickupRadius {pickupRadius} enableEnemyLootbox {enableEnemyLootbox} enemyLootboxRadius {enemyLootboxRadius} enableOtherLootbox {enableOtherLootbox} otherLootboxRadius {otherLootboxRadius} enableHiddenBox {enableHiddenBox} hiddenBoxRadius {hiddenBoxRadius}");

            Collider[] hits = new Collider[1000];
            int layerMask = 1 << LayerMask.NameToLayer("Interactable");

            LevelManager level = LevelManager.Instance;
            CharacterMainControl main = level != null ? level.MainCharacter : null;

            CacheLootBoxes.Clear();
            CacheTombBoxes.Clear();

            if (main == null || !main.IsMainCharacter)
                return new List<Item>();

            float maxRadius = Math.Max(pickupRadius, Math.Max(enableEnemyLootbox ? enemyLootboxRadius : 0f, enableOtherLootbox ? otherLootboxRadius : 0f));
            if (enableHiddenBox) maxRadius = Math.Max(maxRadius, hiddenBoxRadius);
            Vector3 searchCenter = main.transform.position + Vector3.up * 0.5f + main.CurrentAimDirection * 0.2f;

            int hitCount = Physics.OverlapSphereNonAlloc(searchCenter, maxRadius, hits, layerMask);
            if (hitCount <= 0) return new List<Item>();

            // 按距离排序
            float[] distances = new float[hitCount];
            for (int i = 0; i < hitCount; i++)
                distances[i] = Vector3.Distance(searchCenter, hits[i].ClosestPoint(searchCenter));
            Array.Sort(distances, hits, 0, hitCount);

            HashSet<Item> foundItems = new HashSet<Item>();

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = hits[i];
                float dist = distances[i];

                // 地面散落物
                if (dist <= pickupRadius)
                {
                    InteractablePickup pickup = col.GetComponent<InteractablePickup>();
                    if (pickup != null)
                    {
                        Item item = GetItemFromPickup(pickup);
                        if (item != null) foundItems.Add(item);
                    }
                }

                bool isEnemyBox = enableEnemyLootbox && dist < enemyLootboxRadius;
                bool isOtherBox = enableOtherLootbox && dist < otherLootboxRadius;
                bool isHiddenBox = enableHiddenBox && dist < hiddenBoxRadius;

                if (isEnemyBox || isOtherBox || isHiddenBox)
                {
                    InteractableLootbox lootbox = col.GetComponent<InteractableLootbox>();
                    if (lootbox != null)
                    {
                        string displayKey = (string)DynamicLootBoxManager.LootboxDisplayNameKeyField.GetValue(lootbox);
                        bool isEnemyLoot = "UI_LootBox_Loot".Equals(displayKey);
                        bool isHiddenLoot = "UI_LootBox_Hidden".Equals(displayKey);
                        bool isOtherLoot = !isEnemyLoot && !isHiddenLoot && (
                            (displayKey != null && displayKey.StartsWith("UI_LootBox"))
                            || "UI_Interact_Cloth".Equals(displayKey)
                            || "UI_Interact_Tomb".Equals(displayKey));

                        // 土堆/藏匿点不受requireItem限制
                        bool canSearch = (isEnemyBox && isEnemyLoot)
                            || (isOtherBox && !lootbox.requireItem && isOtherLoot)
                            || (isHiddenBox && isHiddenLoot);

                        if (canSearch)
                        {
                            foreach (Item item in lootbox.Inventory)
                            {
                                if (item != null) foundItems.Add(item);
                            }
                            lootbox.SetMarkerUsed();
                            lootbox.needInspect = false;
                            lootbox.Inventory.hasBeenInspectedInLootBox = true;
                            CacheLootBoxes.Add(lootbox);

                            if ("UI_Interact_Tomb".Equals(displayKey))
                                CacheTombBoxes.Add(lootbox);
                        }
                    }
                }

                if (foundItems.Count >= MAX_SEARCH_COUNT) break;
            }

            return foundItems.ToList();
        }

        public static bool SearchItemAroundForNotify(float pickupRadius, bool enableEnemyLootbox, float enemyLootboxRadius, bool enableOtherLootbox, float otherLootboxRadius, bool enableHiddenBox = false, float hiddenBoxRadius = 3f)
        {
            Collider[] hits = new Collider[1000];
            int layerMask = 1 << LayerMask.NameToLayer("Interactable");

            LevelManager level = LevelManager.Instance;
            CharacterMainControl main = level != null ? level.MainCharacter : null;

            if (main == null || !main.IsMainCharacter) return false;

            float maxRadius = Math.Max(pickupRadius, Math.Max(enableEnemyLootbox ? enemyLootboxRadius : 0f, enableOtherLootbox ? otherLootboxRadius : 0f));
            if (enableHiddenBox) maxRadius = Math.Max(maxRadius, hiddenBoxRadius);
            Vector3 searchCenter = main.transform.position + Vector3.up * 0.5f + main.CurrentAimDirection * 0.2f;

            int hitCount = Physics.OverlapSphereNonAlloc(searchCenter, maxRadius, hits, layerMask);
            if (hitCount <= 0) return false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = hits[i];
                float dist = Vector3.Distance(searchCenter, col.ClosestPoint(searchCenter));

                if (dist <= pickupRadius)
                {
                    InteractablePickup pickup = col.GetComponent<InteractablePickup>();
                    if (pickup != null && GetItemFromPickup(pickup) != null)
                        return true;
                }

                bool isEnemyBox = enableEnemyLootbox && dist < enemyLootboxRadius;
                bool isOtherBox = enableOtherLootbox && dist < otherLootboxRadius;
                bool isHiddenBox = enableHiddenBox && dist < hiddenBoxRadius;

                if (!isEnemyBox && !isOtherBox && !isHiddenBox) continue;

                InteractableLootbox lootbox = col.GetComponent<InteractableLootbox>();
                if (lootbox == null) continue;

                string displayKey = (string)DynamicLootBoxManager.LootboxDisplayNameKeyField.GetValue(lootbox);
                bool isEnemyLoot = "UI_LootBox_Loot".Equals(displayKey);
                bool isHiddenLoot = "UI_LootBox_Hidden".Equals(displayKey);
                bool isOtherLoot = !isEnemyLoot && !isHiddenLoot && (
                    (displayKey != null && displayKey.StartsWith("UI_LootBox"))
                    || "UI_Interact_Cloth".Equals(displayKey)
                    || "UI_Interact_Tomb".Equals(displayKey));

                bool canSearch = (isEnemyBox && isEnemyLoot)
                    || (isOtherBox && !lootbox.requireItem && isOtherLoot)
                    || (isHiddenBox && isHiddenLoot);

                if (!canSearch) continue;

                foreach (Item item in lootbox.Inventory)
                {
                    if (item != null) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 安全地从InteractablePickup获取Item，处理DuckovItemAgent类型
        /// </summary>
        private static Item GetItemFromPickup(InteractablePickup pickup)
        {
            if (pickup == null) return null;
            try
            {
                object agent = pickup.ItemAgent;
                if (agent == null) return null;

                // 尝试通过属性获取Item
                PropertyInfo itemProp = agent.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
                if (itemProp != null)
                    return itemProp.GetValue(agent) as Item;

                // 尝试直接转换
                if (agent is ItemAgent itemAgent)
                    return itemAgent.Item;

                Debug.LogWarning($"无法从 {agent.GetType().Name} 获取Item");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetItemFromPickup error: {ex.Message}");
                return null;
            }
        }

        private void TryFindDeathInfoAndTouch(InteractableLootbox box)
        {
            if (DeadBodyManager.Instance == null) return;

            List<DeadBodyManager.DeathInfo> deaths = DeadBodyManagerDeathInfosField.GetValue(DeadBodyManager.Instance) as List<DeadBodyManager.DeathInfo>;
            if (deaths == null) return;

            int checkCount = 10;
            foreach (DeadBodyManager.DeathInfo info in deaths.AsEnumerable().Reverse())
            {
                if (info != null && info.worldPosition == box.transform.position)
                {
                    info.touched = true;
                    break;
                }
                if (checkCount-- < 0) break;
            }
        }
    }
}
