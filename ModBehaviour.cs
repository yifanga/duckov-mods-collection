using System;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Reflection;
using Duckov.MasterKeys.UI;
using System.Collections.Generic;
using System.Linq;

namespace LootNearbyItem
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {

        private const string HarmonyId = "duckovMods.TagInventoryWeight";

        // private Harmony harmony;

        void OnEnable()
        {
            InteractHUD h;
            InteractSelectionHUD a;
            InteractablePickup p;
            // Debug.Log("TagInventoryWeight Loaded!!!");
            // // 创建Harmony实例
            // harmony = new Harmony(HarmonyId);
            // // 直接应用补丁
            // harmony.PatchAll(Assembly.GetExecutingAssembly());

        }


        void OnDisable()
        {
            // if (harmony != null)
            // {
            //     // 更精确的卸载方式 - 只移除本mod的补丁
            //     harmony.UnpatchAll(HarmonyId);
            //     harmony = null;
            //     Debug.Log("Harmony patches removed");
            // }

        }

        void Update()
        {
            // 检测空格键按下
            if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log("H key pressed!");
                // 在这里执行你的逻辑
                LogPickUps();
            }
        }

        void LogPickUps()
        {
            CharacterMainControl? main = LevelManager.Instance?.MainCharacter;
            if (main != null)
            {
                Debug.Log($"main pos at {main.transform.position}");
            }
            else
            {
                Debug.Log("Test no main");
                return;
            }

            InteractableBase? around = main.interactAction.MasterInteractableAround;
            if (null != around)
            {
                foreach (InteractableBase pickupBase in around.GetInteractableList())
                {
                    if (pickupBase is InteractablePickup pickup)
                    {
                        Item item = pickup.ItemAgent.Item;
                        Debug.Log("find Interactable item " + item.name + " " + pickup.isActiveAndEnabled + " at pos " + pickup.ItemAgent.transform.position);
                    }
                }
                var otherGroup = SearchPickUpAround();
                foreach (InteractablePickup pickup in otherGroup)
                {
                    Item item = pickup.ItemAgent.Item;
                    Debug.Log("find other item " + item.name + " " + pickup.isActiveAndEnabled + " at pos " + pickup.ItemAgent.transform.position + " distance: " + Vector3.Distance(main.transform.position, item.transform.position));
                }

            }

            //太慢了，而且会卡
            // InteractablePickup[] pickups = UnityEngine.Object.FindObjectsOfType<InteractablePickup>();
            // foreach (InteractablePickup pickup in pickups)
            // {
            //     if (pickup != null)
            //     {
            //         Item item = pickup.ItemAgent.Item;
            //         Debug.Log("find item" + item.name + " " + pickup.isActiveAndEnabled);
            //         Debug.Log(pickup.ItemAgent);
            //         CharacterMainControl main = CharacterMainControl.Main;
            //     }
            // }
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