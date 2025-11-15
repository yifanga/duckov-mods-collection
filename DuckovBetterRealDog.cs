using Duckov.Modding;
using Duckov.Scenes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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


        private string targetWord = "GODDOGGODDOGGODDOGGODDOGGODGODDOGGODDOGGODDOGGODDOGGODGODDOGGODDOGGODDOGGODDOGGOD";
        private Dictionary<char, List<Vector2>> letterPatterns = new Dictionary<char, List<Vector2>>();
        private int currentLetterIndex = 0;
        private List<InteractableLootbox> arrangedBoxes = new List<InteractableLootbox>(); // 已经排列的箱子

        private float letterFacingAngle = 20f;

        #endregion

        #region Lifecycle Methods

        private void OnEnable()
        {
            LevelManager.OnAfterLevelInitialized += Initialize;

            // 初始化配置
            ModConfigManager.Init();
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

        private void OnDisable()
        {
            LevelManager.OnAfterLevelInitialized -= Initialize;
            ModManager.OnModActivated -= ModConfigManager.OnModConfigMenuActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(ModConfigManager.OnModConfigOptionsChanged);
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

            // 初始化字母模式
            InitializeLetterPatterns();

            // 重置状态
            ResetLetterFormation();
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
            if (Input.GetKeyDown(ModConfigManager.ToggleSearchKey))
            {
                isHoldingL = true;
                holdTimerL = 0f;
            }

            if (Input.GetKeyUp(ModConfigManager.ToggleSearchKey))
            {
                if (isHoldingL)
                {
                    if (holdTimerL < HOLD_THRESHOLD_L)
                    {
                       TogglePickMode();
                    }
                    else 
                    {
                        TogglePickMode();
                    }
                }
                isHoldingL = false;
                
            }

            if (isHoldingL)
            {
                holdTimerL += Time.deltaTime;
            }

            // Handle V key (drop all boxes)
            if (Input.GetKeyDown(ModConfigManager.UnloadItemsKey))
            {
                isHoldingV = true;
                holdTimerV = 0f;
            }

            if (Input.GetKeyUp(ModConfigManager.UnloadItemsKey))
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
            if (petAI.standBy)
            {
                petAI.standByPos = petAI.transform.position;
            }
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
                if (null == box || !box.needInspect)
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

                // 收集完成后，尝试更新字母构建
                if (!ModConfigManager.ToggleNormalPattern)
                {
                    UpdateLetterFormationImmediately();
                }
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
            player.PopText("开始卸货", 3f);
            var localPickMode = isPickMode;
            isPickMode = false;


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
            // 重置字母构建状态
            ResetLetterFormation();

            // Reset back state
            isPickMode = localPickMode;
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

        #region build word



        // 在Initialize方法中初始化字母模式
        private void InitializeLetterPatterns()
        {
            // G字母模式 (使用9个点)
            letterPatterns['G'] = new List<Vector2>
            {
                // 从下到上构建
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(-1, 1), new Vector2(2, 1),
                new Vector2(-1, 2), new Vector2(1, 2),new Vector2(2, 2),
                new Vector2(-1, 3),
                new Vector2(0, 4),new Vector2(1, 4),new Vector2(2, 4)
            };

            // O字母模式 (使用8个点)
            letterPatterns['O'] = new List<Vector2>
            {
                // 从下到上构建
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(-1,1), new Vector2(2, 1),
                new Vector2(-1, 2), new Vector2(2, 2),
                new Vector2(-1, 3), new Vector2(2, 3),
                new Vector2(0, 4), new Vector2(1, 4),
            };

            // D字母模式 (使用8个点)
            letterPatterns['D'] = new List<Vector2>
            {
                // 从下到上构建
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(-1, 0),
                new Vector2(-1,1), new Vector2(2, 1),
                new Vector2(-1, 2), new Vector2(2, 2),
                new Vector2(-1, 3), new Vector2(2, 3),
                new Vector2(-1, 4), new Vector2(0, 4), new Vector2(1, 4),
            };
        }

        // 更新字母构建
        // 立即更新字母构建（无动画，无提示）
        // 立即更新字母构建（竖直堆叠，支持角度）
        private void UpdateLetterFormationImmediately()
        {
            if (currentLetterIndex >= targetWord.Length) return;

            // 持续检查并构建字母，直到没有足够的箱子或完成所有字母
            while (currentLetterIndex < targetWord.Length)
            {
                char currentLetter = targetWord[currentLetterIndex];
                if (!letterPatterns.ContainsKey(currentLetter))
                {
                    currentLetterIndex++;
                    continue;
                }

                List<Vector2> pattern = letterPatterns[currentLetter];
                int previousCount = GetPreviousLettersBoxCount();
                int alreadyPlaced = arrangedBoxes.Count - previousCount;

                // 如果当前字母已经完成，进入下一个字母
                if (alreadyPlaced >= pattern.Count)
                {
                    currentLetterIndex++;
                    continue;
                }

                // 计算可用的箱子数量
                int totalCollected = carriedBoxes.Count;
                int usedInFormation = arrangedBoxes.Count;
                int availableBoxes = totalCollected - usedInFormation;

                if (availableBoxes <= 0) break;

                // 构建当前字母的剩余部分
                BuildCurrentLetterImmediately(pattern, previousCount, alreadyPlaced, availableBoxes, currentLetterIndex);

                // 检查当前字母是否完成
                alreadyPlaced = arrangedBoxes.Count - previousCount;
                if (alreadyPlaced >= pattern.Count)
                {
                    currentLetterIndex++;
                    // 如果完成了3个字母，则人物吐泡泡
                    if (currentLetterIndex > 0 && currentLetterIndex % 3 == 0)
                    {
                        player.PopText(GetRandomMessage(), 3f);
                        break;
                    }
                }
                else
                {
                    break; // 箱子不够，等待更多箱子
                }
            }
        }

        public static string GetRandomMessage()
        {
            // 定义所有可能的字符串
            string[] messages = {
                // "🐶+📦= GOD 🙏",
                "世风日下",
                "真的狗！",
                "狗，上帝！",
                "连劳登都不如！"
            };

            // 使用 Unity 的随机数生成器
            int randomIndex = Random.Range(0, messages.Length);

            return messages[randomIndex];
        }

        /// <summary>
        /// 使用罗德里格公式旋转向量
        /// </summary>
        /// <param name="v">原始向量</param>
        /// <param name="k">旋转轴（会自动归一化）</param>
        /// <param name="theta">旋转角度（弧度）</param>
        /// <returns>旋转后的向量</returns>
        public static Vector3 RodriguesRotate(Vector3 v, Vector3 k, float theta)
        {
            // 归一化旋转轴
            k = k.normalized;
            theta *= Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(theta);
            float sinTheta = Mathf.Sin(theta);

            // 罗德里格公式的三个部分
            Vector3 term1 = v * cosTheta;
            Vector3 term2 = Vector3.Cross(k, v) * sinTheta;
            Vector3 term3 = k * Vector3.Dot(k, v) * (1 - cosTheta);

            return term1 + term2 + term3;
        }

        // 旋转点
        private Vector3 RotatePoint(Vector3 point, float angleDegrees)
        {
            float angleRad = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            var res = new Vector3(
                point.x * cos - point.z * sin,
                point.y,
                point.x * sin + point.z * cos
            );
            angleRad = 30 * Mathf.Deg2Rad;
            cos = Mathf.Cos(angleRad);
            sin = Mathf.Sin(angleRad);

            res = new Vector3(
                res.x,
                res.y * cos - res.z * sin,
                res.y * sin + res.z * cos
            );

            // return res;
            angleRad = 60 * Mathf.Deg2Rad;
            cos = Mathf.Cos(angleRad);
            sin = Mathf.Sin(angleRad);
            return new Vector3(
                res.y * sin + res.x * cos,
                res.y * cos - res.x * sin,
                res.z
            );
        }

        // 获取之前字母使用的箱子总数
        private int GetPreviousLettersBoxCount()
        {
            int count = 0;
            for (int i = 0; i < currentLetterIndex; i++)
            {
                char letter = targetWord[i];
                if (letterPatterns.ContainsKey(letter))
                {
                    count += letterPatterns[letter].Count;
                }
            }
            return count;
        }
        // 立即构建当前字母（竖直堆叠，支持角度）
        private void BuildCurrentLetterImmediately(List<Vector2> pattern, int previousCount, int alreadyPlaced, int availableBoxes, float letterCount)
        {
            Vector2 margin = new Vector2(0.6f, 0.65f);
            // 计算这次要放置多少个箱子
            int toPlace = Mathf.Min(availableBoxes, pattern.Count - alreadyPlaced);

            for (int i = 0; i < toPlace; i++)
            {
                int patternIndex = alreadyPlaced + i;
                int boxIndex = arrangedBoxes.Count;

                if (patternIndex < pattern.Count && boxIndex < carriedBoxes.Count)
                {
                    var box = carriedBoxes[boxIndex];
                    if (box != null && !arrangedBoxes.Contains(box))
                    {
                        Vector2 patternPos = pattern[patternIndex];

                        // 应用旋转角度
                        Vector2 marginPos = patternPos * margin;
                        Vector3 offsetPos = new Vector3(
                            marginPos.x,
                            marginPos.y,
                            0
                        );

                        //竖直要叠加上个字母的高度,以及在向上偏移一个单位
                        offsetPos.y += letterCount * 5 * margin.y + margin.y;

                        // offsetPos = RotatePoint(offsetPos, letterFacingAngle);

                        offsetPos = RodriguesRotate(offsetPos, new Vector3(0, 1, 0), -30);
                        offsetPos = RodriguesRotate(offsetPos, new Vector3(Mathf.Sqrt(3), 0, 1), 30);

                        // 旋转位置，直接设置位置，无动画
                        // 计算世界坐标（相对于狗的位置）
                        Vector3 worldPos = pet.transform.position + offsetPos;
                        box.transform.position = worldPos;
                        box.transform.rotation = Quaternion.Euler(0, 60, 0);
                        arrangedBoxes.Add(box);
                    }
                }
            }
        }

        // 重置字母构建状态
        private void ResetLetterFormation()
        {
            currentLetterIndex = 0;
            arrangedBoxes.Clear();
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