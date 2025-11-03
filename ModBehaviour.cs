using ItemStatsSystem;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace LootNearbyItem
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {

        private float lastHKeyPressTime = 0f;
        private const float KEY_DEBOUNCE_TIME = 0.5f; // 防抖时间500毫秒

        void OnEnable()
        {

        }


        void OnDisable()
        {

        }

        void Update()
        {
            // 检测空格键按下
            if (Input.GetKeyDown(KeyCode.H))
            {
                // 防抖检查 - 防止连续触发
                if (Time.time - lastHKeyPressTime < KEY_DEBOUNCE_TIME)
                {
                    Debug.Log("H键触发过于频繁，已忽略");
                    return;
                }
                // 更新最后按键时间
                lastHKeyPressTime = Time.time;
                Debug.Log("H key pressed!");
                // 检查是否已有战利品界面打开
                if (null != DynamicLootBoxManager.Instance && DynamicLootBoxManager.Instance.IsLootViewOpen())
                {
                    Debug.Log("战利品界面已打开，忽略新请求");
                    return;
                }

                // 在这里执行你的逻辑
                List<InteractablePickup> pickups = SearchPickUpAround();
                // 添加初始物品
                if (pickups.Count > 0)
                {
                    GenerateAndOpenRandomLoot(pickups.Select(p => p.ItemAgent.Item).ToList());
                }
                else
                {
                    // TODO: 人物吐气泡说：不要找啦，周围没有散落物！
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

        public static List<InteractablePickup> SearchPickUpAround()
        {
            Collider[] colliders = new Collider[100];
            LayerMask interactLayers = 1 << LayerMask.NameToLayer("Interactable");
            CharacterMainControl? main = LevelManager.Instance?.MainCharacter;

            if (null == main || !main.IsMainCharacter)
            {
                return new List<InteractablePickup>();
            }

            int num = Physics.OverlapSphereNonAlloc(main.transform.position + Vector3.up * 0.5f + main.CurrentAimDirection * 0.2f, 0.3f, colliders, interactLayers);
            if (num <= 0)
            {
                return new List<InteractablePickup>();
            }

            HashSet<InteractablePickup> uniqueItems = new HashSet<InteractablePickup>();
            for (int i = 0; i < num; i++)
            {
                Collider collider = colliders[i];
                InteractablePickup tmp = collider.GetComponent<InteractablePickup>();
                if (null != tmp)
                {
                    uniqueItems.Add(tmp);
                }

            }
            return uniqueItems.ToList();
        }

    }
}