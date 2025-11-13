using Duckov.Scenes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DuckovBetterRealDog
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        #region Fields and Properties

        public static FieldInfo LootboxDisplayNameKeyField = typeof(InteractableLootbox).GetField("displayNameKey",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static FieldInfo LootboxBaseOtherInterablesInGroupField = typeof(InteractableBase).GetField("otherInterablesInGroup",
                BindingFlags.NonPublic | BindingFlags.Instance);
        
        // Components
        private CharacterMainControl player;
        private CharacterMainControl pet;
        private PetAI petAI;
        private Transform dogBoxParent;
        
        // Box collections
        private readonly List<InteractableLootbox> allBoxes = new();
        private readonly List<InteractableLootbox> carriedBoxes = new();
        private readonly List<InteractableLootbox> discardedBoxes = new();
        
        // State management
        private bool isPickMode = true;
        private bool isMovingToBox = false;
        private bool isHoldingL = false;
        private bool isHoldingV = false;
        private float holdTimerL = 0f;
        private float holdTimerV = 0f;
        private const float HOLD_THRESHOLD_L = 1f;
        private const float HOLD_THRESHOLD_V = 2f;
        
        // Current target
        private InteractableLootbox targetBox;
        
        // Constants
        private const float PET_HEIGHT = 0.7f;
        private const float BOX_HEIGHT = 0.5f;
        private const float MOVE_TIMEOUT = 5f;
        private const float DROP_FORCE = 5.5f;
        private const float DROP_UPWARD_FORCE = 0.5f;
        
        #endregion

        #region Lifecycle Methods

        private void OnEnable()
        {
            LevelManager.OnAfterLevelInitialized += Initialize;
        }

        private void OnDisable()
        {
            LevelManager.OnAfterLevelInitialized -= Initialize;
        }

        private void Initialize()
        {
            // Get references
            player = LevelManager.Instance.MainCharacter;
            pet = LevelManager.Instance.PetCharacter;
            petAI = pet.GetComponentInChildren<PetAI>();
            
            // Setup container for carried boxes
            dogBoxParent = pet.transform.Find("SxerDogBParent");
            if (dogBoxParent == null)
            {
                var parentObj = new GameObject("SxerDogBParent");
                parentObj.transform.SetParent(pet.transform);
                parentObj.transform.localPosition = Vector3.zero;
                dogBoxParent = parentObj.transform;
            }
            
            // Clear collections
            allBoxes.Clear();
            carriedBoxes.Clear();
            discardedBoxes.Clear();
            
            // Reset state
            isPickMode = true;
            isMovingToBox = false;
            targetBox = null;
        }

        private void Update()
        {
            HandleInput();
            if (isPickMode)
            {
                FindAndCollectBoxes();
            }
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // Handle L key (toggle pick mode)
            if (Input.GetKeyDown(KeyCode.L))
            {
                isHoldingL = true;
                holdTimerL = 0f;
            }
            
            if (Input.GetKeyUp(KeyCode.L))
            {
                isHoldingL = false;
            }
            
            if (isHoldingL)
            {
                holdTimerL += Time.deltaTime;
                if (holdTimerL >= HOLD_THRESHOLD_L)
                {
                    TogglePickMode();
                    isHoldingL = false;
                }
            }

            // Handle V key (drop all boxes)
            if (Input.GetKeyDown(KeyCode.V))
            {
                isHoldingV = true;
                holdTimerV = 0f;
            }

            if (Input.GetKeyUp(KeyCode.V))
            {
                if (isHoldingV)
                {
                    // 按键释放时判断是短按还是长按
                    if (holdTimerV < HOLD_THRESHOLD_V)
                    {
                        // 短按 - 轻柔投掷
                        DropAllBoxes(0.2f);
                    }
                    else
                    {
                        // 长按 - 强力投掷
                        DropAllBoxes(DROP_FORCE);
                    }
                }
                isHoldingV = false;
            }

            if (isHoldingV)
            {
                holdTimerV += Time.deltaTime;
            }
        }

        private void TogglePickMode()
        {
            isPickMode = !isPickMode;
            petAI.standBy = !isPickMode;
            player.PopText(isPickMode ? "上！" : "狗子歇歇吧～", 5f);
        }

        #endregion

        #region Box Collection Logic

        private void FindAndCollectBoxes()
        {
            if (isMovingToBox) return;
            
            // Find all boxes in scene
            FindAllBoxes();
            
            // Collect nearest box if available
            if (allBoxes.Count > 0)
            {
                targetBox = allBoxes[0];
                if (targetBox != null)
                {
                    MoveToBox(targetBox);
                }
                else
                {
                    allBoxes.RemoveAt(0);
                }
            }
        }

        private void FindAllBoxes()
        {
            if (MultiSceneCore.Instance == null) return;
            
            var boxes = MultiSceneCore.Instance.GetComponentsInChildren<InteractableLootbox>();
            foreach (var box in boxes)
            {
                // 如果box搜索过，则跳过处理
                if(null == box || !box.needInspect)
                {
                    continue;
                }

                // 只有敌人的盒子才会去捡
                string nameKey = (string)LootboxDisplayNameKeyField.GetValue(box);
                bool isEnemyBox = "UI_LootBox_Loot".Equals(nameKey);

                if (isEnemyBox && !allBoxes.Contains(box) && 
                    !carriedBoxes.Contains(box) && 
                    !discardedBoxes.Contains(box))
                {
                    allBoxes.Add(box);
                }
            }
        }

        private void MoveToBox(InteractableLootbox box)
        {
            if (box == null || pet == null) return;
            
            isMovingToBox = true;
            petAI.standByPos = box.transform.position;
            petAI.standBy = true;
            
            StartCoroutine(MoveToBoxCoroutine(box));
        }

        private IEnumerator MoveToBoxCoroutine(InteractableLootbox box)
        {
            if (box == null || pet == null)
            {
                isMovingToBox = false;
                yield break;
            }
            
            float moveTimer = 0f;
            
            while (isMovingToBox)
            {
                if (pet == null || box == null)
                {
                    isMovingToBox = false;
                    yield break;
                }
                
                // Check distance to box
                Vector3 petXZ = new Vector3(box.transform.position.x, pet.transform.position.y, box.transform.position.z);
                float distance = Vector3.Distance(pet.transform.position, petXZ);
                
                if (distance < 1f)
                {
                    // Arrived at box - collect it
                    CollectBox(box);
                    break;
                }
                else if (distance > 10f)
                {
                    // Too far - teleport to box
                    pet.transform.position = box.transform.position;
                    moveTimer = 0f;
                }
                else
                {
                    // Keep moving
                    moveTimer += Time.deltaTime;
                    if (moveTimer > MOVE_TIMEOUT)
                    {
                        // Timeout - teleport to box
                        pet.transform.position = box.transform.position;
                        moveTimer = 0f;
                    }
                }
                
                yield return null;
            }
            
            isMovingToBox = false;
        }

        private void CollectBox(InteractableLootbox box)
        {
            if (box == null) return;
            
            try
            {
                // Add drop interaction if not exists
                var dropComponent = box.GetComponentInChildren<InteractableOnlyDrop>(true);
                if (dropComponent == null)
                {
                    var dropObj = new GameObject("SxerInteractableOnlyDrop");
                    dropComponent = dropObj.AddComponent<InteractableOnlyDrop>();
                    dropComponent.InteractName = "移除";
                    dropComponent.enabled = true;
                    dropComponent.MarkerActive = false;
                    dropComponent.onDrop += OnBoxDropped;
                    dropObj.transform.SetParent(box.transform);
                    dropObj.transform.localPosition = Vector3.zero;
                    LootboxBaseOtherInterablesInGroupField?.SetValue(dropComponent, new List<InteractableBase>());
                    
                    // Add to interaction group
                    var tempListBase = Traverse.Create(box).Field("otherInterablesInGroup").GetValue<List<InteractableBase>>();
                    if (tempListBase != null)
                    {
                        tempListBase.Add(dropComponent);
                        box.GetInteractableList();
                    }
                }
                else
                {
                    dropComponent.gameObject.SetActive(true);
                    dropComponent.onDrop += OnBoxDropped; // Ensure event is subscribed
                }
                
                // Disable physics and hide carry interaction
                var rb = box.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    SetRigidbodyActive(false, rb, box);
                }
                
                var carryComponent = box.GetComponentInChildren<InteractableCarriable>();
                if (carryComponent != null)
                {
                    carryComponent.gameObject.SetActive(false);
                }
                
                // Set as child of pet and position
                box.transform.SetParent(dogBoxParent);
                box.transform.localPosition = new Vector3(0, PET_HEIGHT + BOX_HEIGHT * carriedBoxes.Count, 0);
                
                // Update collections
                allBoxes.Remove(box);
                carriedBoxes.Add(box);
                targetBox = null;
                
                // Stop following
                petAI.standBy = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error collecting box: {ex.Message}");
                discardedBoxes.Add(box);
                allBoxes.Remove(box);
                targetBox = null;
            }
        }

        #endregion

        #region Box Dropping Logic

        private void DropAllBoxes(float dropForce)
        {
            player.PopText("开始卸货", 5f);
            
            if (carriedBoxes.Count <= 0) return;
            
            // Calculate drop direction
            Vector3 dropDirection = player.transform.forward;
            
            // Create rotation for spreading boxes
            Vector3 direction = dropDirection;
            Vector3 rotateAxis = Vector3.up;
            Quaternion rotation = Quaternion.AngleAxis(360f / carriedBoxes.Count, rotateAxis);
            
            // Drop each box
            var boxesToDrop = new List<InteractableLootbox>(carriedBoxes);
            for (int i = boxesToDrop.Count - 1; i >= 0; i--)
            {
                var box = boxesToDrop[i];
                if (box != null)
                {
                    Vector3 newDir = rotation * direction;
                    direction = newDir;
                    
                    var dropComponent = box.GetComponentInChildren<InteractableOnlyDrop>(true);
                    if (dropComponent != null)
                    {
                        StartCoroutine(DropBox(dropComponent, box, newDir, dropForce));
                    }
                }
            }
            
            // Update collections
            discardedBoxes.AddRange(boxesToDrop);
            carriedBoxes.Clear();
            
            // Reset state
            isPickMode = false;
        }

        private void OnBoxDropped(InteractableOnlyDrop dropComponent)
        {
            if (dropComponent == null) return;
            
            var box = dropComponent.transform.parent?.GetComponent<InteractableLootbox>();
            if (box != null)
            {
                StartCoroutine(DropBox(dropComponent, box, player.transform.forward));
                discardedBoxes.Add(box);
                carriedBoxes.Remove(box);
                StartCoroutine(RearrangeBoxes());
            }
        }

        private IEnumerator DropBox(InteractableOnlyDrop dropComponent, InteractableLootbox box, Vector3 direction, float dropForce = DROP_FORCE)
        {
            if (dropComponent == null || box == null) yield break;
            
            // Hide drop component
            dropComponent.gameObject.SetActive(false);
            
            // Move to active scene
            try
            {
                MultiSceneCore.MoveToActiveWithScene(box.gameObject, SceneManager.GetActiveScene().buildIndex);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to move box to active scene: {ex.Message}");
            }
            
            // Re-enable carry interaction first
            var carryComponent = box.GetComponentInChildren<InteractableCarriable>(true);
            if (carryComponent != null)
            {
                carryComponent.gameObject.SetActive(true);
            }
            
            // Enable physics properly
            var rb = box.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Reset transform before enabling physics
                box.transform.SetParent(null);
                
                // Configure rigidbody for proper physics
                SetRigidbodyActive(true, rb, box);
                
                // Apply velocity
                rb.velocity = direction.normalized * dropForce + Vector3.up * DROP_UPWARD_FORCE;
                
                // Reset angular velocity
                rb.angularVelocity = Vector3.zero;
            }

            // Restore collider settings
            if (box.interactCollider != null)
            {
                box.interactCollider.isTrigger = false;
            }
            yield return new WaitForSeconds(0.1f);
            if (rb != null && rb.gameObject.activeInHierarchy)
            {
                rb.velocity *= 0.8f; // 减速比例可以增大
            }
            // Final cleanup after delay
            yield return new WaitForSeconds(2.9f);
            if (rb != null)
            {
                SetRigidbodyActive(true, rb, box);
            }
        }

        private IEnumerator RearrangeBoxes()
        {
            yield return new WaitForEndOfFrame();
            
            if (carriedBoxes.Count <= 0) yield break;
            
            // Calculate movement distance
            float totalMoveDistance = carriedBoxes[0].transform.localPosition.y - PET_HEIGHT;
            
            // Move all boxes down by the same amount
            for (int i = 0; i < carriedBoxes.Count; i++)
            {
                var box = carriedBoxes[i];
                if (box != null)
                {
                    Vector3 targetPos = new Vector3(0, box.transform.localPosition.y - totalMoveDistance, 0);
                    box.transform.localPosition = targetPos;
                }
            }
        }

        public void SetRigidbodyActive(bool active, Rigidbody rb, InteractableLootbox lootbox)
        {
            if (active)
            {
                rb.isKinematic = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                if (lootbox && lootbox.interactCollider)
                {
                    lootbox.interactCollider.isTrigger = false;
                    return;
                }
            }
            else
            {
                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.None;
                if (lootbox && lootbox.interactCollider)
                {
                    lootbox.interactCollider.isTrigger = true;
                }
            }
        }

        #endregion
    }

    #region InteractableOnlyDrop Component

    public class InteractableOnlyDrop : InteractableBase
    {
        public Action<InteractableOnlyDrop> onDrop;

        protected override bool IsInteractable() => true;



        protected override void OnInteractFinished()
        {
            if (interactCharacter == null) return;
            onDrop?.Invoke(this);
        }

        public void ForceDrop(Vector3 direction)
        {
            if (interactCollider != null)
            {
                GameObject.Destroy(interactCollider);
            }
            StartCoroutine(Drop(direction));
        }

        private IEnumerator Drop(Vector3 direction)
        {
            yield return null;
            // Implementation handled in main class
        }
    }

    #endregion

    
}