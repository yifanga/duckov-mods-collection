using Duckov.Scenes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

namespace DuckovBetterRealDog
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        CharacterMainControl petControl;
        AICharacterController petAIController;
        PetAI petAI;
        CharacterMainControl characterControl;

        List<InteractableLootbox> allBox = new List<InteractableLootbox>();
        List<InteractableLootbox> dogGetBox = new List<InteractableLootbox>();
        List<InteractableLootbox> destoryBox = new List<InteractableLootbox>();
        bool openPick = false;

        Transform dogBoxParent;

        void OnEnable()
        {
            LevelManager.OnAfterLevelInitialized += DogBInit;

        }


        void OnDisable()
        {
            LevelManager.OnAfterLevelInitialized -= DogBInit;

        }

        void OnLongPressTriggered()
        {
            if (openPick)//关闭
            {
                openPick = false;

                petAI.standBy = false;

                petControl.transform.position = characterControl.transform.position + characterControl.transform.forward.normalized * 1f;

                
                
                characterControl.PopText("狗子歇歇吧～",5f);

            }
            else
            {
                openPick = true;
                //petAI.standBy = true;

                characterControl.PopText("上！", 5f);
            }
        }

        void ReSetFunc()
        {
            characterControl.PopText("开始卸货", 5f);
            //功能关闭
            openPick = false;
            //宠物位置和跟随重置
            if(petAI == null)
                petAI = LevelManager.Instance.PetCharacter.GetComponentInChildren<AICharacterController>().GetComponent<PetAI>();
            petAI.standBy = false;

            if (petControl == null)
                petControl = LevelManager.Instance.PetCharacter;

            if (characterControl == null)
                characterControl = LevelManager.Instance.MainCharacter;

            petControl.transform.position = characterControl.transform.position + characterControl.transform.forward.normalized * 1f;

            //宠物携带包裹重置
            if (dogGetBox.Count > 0)
            {
                Vector3 dir = petControl.transform.forward;
                Vector3 rotateAxis = Vector3.up;
                Quaternion rot = Quaternion.AngleAxis(360f / dogGetBox.Count, rotateAxis);

                for (int i = 0; i < dogGetBox.Count; i++)
                {
                    InteractableLootbox tempLoot = dogGetBox[i];
                    if (tempLoot != null)//避免被剔除造成影响
                    {
                        Vector3 newDir = rot * dir;
                        dir = newDir;

                        InteractableOnlyDrop oD = tempLoot.GetComponentInChildren<InteractableOnlyDrop>(true);
                        if (oD != null)
                        {
                            StartCoroutine(Drop(oD, tempLoot, newDir));
                        }
                    }
                }
            }

            //记录destory
            List<InteractableLootbox> backDestory = new List<InteractableLootbox>();
            backDestory.AddRange(destoryBox);
            //清理缓存
            DogBInit();
            destoryBox.Clear();
            destoryBox.AddRange(backDestory);

            openPick = false;
        }

        // 长按所需时间（秒）
        public float requiredHoldTime = 1f;
        // 记录当前按住的时间
        private float currentHoldTime = 0f;
        // 是否正在按住按键
        private bool isHolding = false;

        private bool isLongHolding = false;
        private float currentLongHoldTime = 0f;
        void Update()
        {
            // 检测L键是否被按下
            if (Input.GetKeyDown(KeyCode.L))
            {
                isHolding = true;
                currentHoldTime = 0f; // 重置计时

            }

            // 检测L键是否被按下
            if (Input.GetKeyDown(KeyCode.V))
            {
                isLongHolding = true;
                currentLongHoldTime = 0f;
            }

            // 当按键被按住时计时
            if (isHolding && Input.GetKey(KeyCode.L))
            {
                currentHoldTime += Time.deltaTime;

                // 当按住时间达到要求时触发事件
                if (currentHoldTime >= requiredHoldTime)
                {
                    OnLongPressTriggered();
                    isHolding = false; // 防止重复触发
                }
            }

            // 当按键被按住时计时
            if (isLongHolding && Input.GetKey(KeyCode.V))
            {
                currentLongHoldTime += Time.deltaTime;

                // 当按住时间达到要求时触发事件
                if (currentLongHoldTime >= 2f)
                {
                    ReSetFunc();
                    isLongHolding = false; // 防止重复触发
                }
            }


            // 当按键松开时重置状态
            if (Input.GetKeyUp(KeyCode.L))
            {
                isHolding = false;

            }

            if (Input.GetKeyUp(KeyCode.V))
            {
 
                isLongHolding = false;
            }



            RunDog();


        }

        void DogBInit()
        {
            petControl = LevelManager.Instance.PetCharacter;
            characterControl = LevelManager.Instance.MainCharacter;
            petAIController = petControl.GetComponentInChildren<AICharacterController>();
            petAI = petAIController.GetComponent<PetAI>();

            allBox.Clear();
            dogGetBox.Clear();
            destoryBox.Clear();
            openPick = true;
            getting = false;
            waitDogMove = false;
            tempLootBox = null;
            IsInMove = false;

            if (petControl.transform.Find("SxerDogBParent") != null)
                dogBoxParent = petControl.transform.Find("SxerDogBParent");
            else
            {
                GameObject tempObj = new GameObject("SxerDogBParent");
                tempObj.transform.SetParent(petControl.transform);
         
                dogBoxParent = tempObj.transform;
                dogBoxParent.transform.localPosition = Vector3.zero;
            }
            //if (petControl != null)
            //    petControl.GetComponent<Collider>().enabled = false;

        }


        void RunDog()
        {
            if (openPick)
            {
                //获取场景所有盒子
                GetAllBoxInScene();

                //取盒子
                GetOneBox();


                //狗子位置
                //if(!getting && !waitDogMove)
                //{
                //    petAI.standBy = false;

                //    if(petControl!=null && characterControl != null)
                //    {
                //        if (petControl.GetComponent<Collider>().enabled)
                //            petControl.GetComponent<Collider>().enabled = false;

                //        if (Vector3.Distance(petControl.transform.position, characterControl.transform.position) > 15f)
                //        {
                //            petControl.transform.position = characterControl.transform.position;
                //        }
                //    }
                //}

            }
        }



       

        private void GetAllBoxInScene()
        {
            if (MultiSceneCore.Instance == null)
                return;
            InteractableLootbox[] findBox = MultiSceneCore.Instance.gameObject.GetComponentsInChildren<InteractableLootbox>();
            if (findBox != null && findBox.Length>0)
            {
                for (int i = 0; i < findBox.Length; i++)
                {
                    if (!allBox.Contains(findBox[i]) && !dogGetBox.Contains(findBox[i]) && !destoryBox.Contains(findBox[i]))
                        allBox.Add(findBox[i]);
                }
            }
        }

        float petHeight = 0.7f;
        float boxHeight = 0.5f;


        bool getting = false;
        bool waitDogMove = false;
        InteractableLootbox tempLootBox = null;

        float dogMoveOnceTime = 0;
        private void GetOneBox()
        {
            if (IsInMove)
                return;

            if (allBox.Count > 0 && !getting)
            {
                if (petAI != null)
                {
                    getting = true;

                    tempLootBox = allBox[0];
                    if(tempLootBox == null) //其他mod删除对象，做空判断
                    {
                        tempLootBox = null;
                        allBox.RemoveAt(0);
                        getting = false;
                    }
                    else
                    {
                        //狗去添盒子
                        petAI.standByPos = tempLootBox.transform.position;
                        petAI.standBy = true;
                        waitDogMove = true;
                    }
                }
            }
            //等待狗移动到盒子上
            if (getting && waitDogMove)
            {
                if(petControl == null)
                {
                    petControl = LevelManager.Instance.PetCharacter;
                }
                if (petControl == null || tempLootBox==null)
                    return;

                Vector3 tempPetXZ = tempLootBox.transform.position;
                tempPetXZ.y = petControl.transform.position.y;
                float dis = Vector3.Distance(petControl.transform.position, tempPetXZ);
                if (dis > 10)
                {
                    petControl.transform.position = tempLootBox.transform.position;
                    dogMoveOnceTime = 0;
                }
                else if (dis < 1f)
                {
                    waitDogMove = false;


                    if (ChangeLootBox())
                    {
                        dogGetBox.Add(tempLootBox);
                    }
                    else//如果执行失败，这个包不要了
                    {
                        destoryBox.Add(tempLootBox);
                    }

                    allBox.Remove(tempLootBox);
                    tempLootBox = null;
                    if (petAI == null)
                        petAI = petControl.GetComponentInChildren<PetAI>();

                    petAI.standBy = false;
                    getting = false;
                    dogMoveOnceTime = 0;
                }
                else//
                {
                    dogMoveOnceTime += Time.deltaTime;
                    //狗子5秒移不到位置的，直接顺序上去
                    if (dogMoveOnceTime > 5)
                    {
                        dogMoveOnceTime = 0;
                        petControl.transform.position = tempLootBox.transform.position;
                    }
                }
            }

        }

        private bool ChangeLootBox()
        {
            try
            {
                if (tempLootBox.GetComponentInChildren<InteractableOnlyDrop>(true) == null)
                {
                    //新的交互动作添加
                    GameObject tempDropComponent = new GameObject("SxerInteractableOnlyDrop");
                    InteractableOnlyDrop onlyDrop = tempDropComponent.AddComponent<InteractableOnlyDrop>();
                    onlyDrop.InteractName = "移除";
                    onlyDrop.enabled = true;
                    onlyDrop.MarkerActive = false;
                    onlyDrop.whenDrop += SortOnce;
                    tempDropComponent.transform.SetParent(tempLootBox.transform);
                    tempDropComponent.transform.localPosition = Vector3.zero;
                    //原盒子交互组添加新的动作
                    List<InteractableBase> tempListBase = Traverse.Create(tempLootBox).Field("otherInterablesInGroup").GetValue<List<InteractableBase>>();
                    tempListBase.Add(onlyDrop);
                    tempLootBox.GetInteractableList();//更新
                                                      
                   
                }
                else
                {
                    tempLootBox.GetComponentInChildren<InteractableOnlyDrop>(true).gameObject.SetActive(true);
                }


                //对原始的修改
                //interList添加新的对象并更新
                //把Carriable隐藏
                //取消物理学效果
                //设置父物体
                //转移位置


                //取消物理运动学效果
                SetRigidbodyActive(false, tempLootBox.GetComponent<Rigidbody>(), tempLootBox);
                //隐藏掉搬运动作
                tempLootBox.GetComponentInChildren<InteractableCarriable>().gameObject.SetActive(false);
               
                //到狗身上
                tempLootBox.transform.SetParent(dogBoxParent);
                tempLootBox.transform.localPosition = new Vector3(0, petHeight + boxHeight * dogGetBox.Count, 0);

                return true;
            }
            catch
            {
                //还原


                return false;


            }

        }


        public void SortOnce(InteractableOnlyDrop dropItem)
        {
            if (dropItem == null)
                return;
            InteractableLootbox tempLootBoxX = dropItem.transform.parent.GetComponent<InteractableLootbox>();
            if (tempLootBoxX != null)
            {

                StartCoroutine(Drop(dropItem, tempLootBoxX, CharacterMainControl.Main.transform.forward));

                destoryBox.Add(tempLootBoxX);
                dogGetBox.Remove(tempLootBoxX);
                StartCoroutine(MoveAction());
            }
        }

        bool IsInMove = false;
        List<Vector3> targetPositions = new List<Vector3>();
        IEnumerator MoveAction()
        {
            yield return new WaitForEndOfFrame();

            if (dogGetBox.Count < 1)
                yield break;

            IsInMove = true;
            // 计算需要移动的总距离（第一个物体需要移动到petHeight，其他物体同步移动相同距离）
            float totalMoveDistance = dogGetBox[0].transform.localPosition.y - petHeight;

            // 提前计算所有物体的目标位置（固定值，不随移动变化）
            targetPositions.Clear();
            foreach (var obj in dogGetBox)
            {
                // 每个物体的目标y值 = 当前y值 - 总移动距离（整体下移相同距离）
                Vector3 targetPos = new Vector3(0, obj.transform.localPosition.y - totalMoveDistance, 0);
                targetPositions.Add(targetPos);
            }

            // 移动循环：所有物体同时移动，直到第一个物体到达目标
            while (IsInMove)
            {
                // 标记是否所有物体都到达目标
                bool allReached = true;

                // 同时移动所有物体（同一帧内完成，避免卡顿）
                for (int i = 0; i < dogGetBox.Count; i++)
                {
                    var obj = dogGetBox[i];
                    var targetPos = targetPositions[i];

                    // 向目标位置移动（速度可自定义，这里用0.5f作为平滑系数）
                    obj.transform.localPosition = Vector3.MoveTowards(
                        obj.transform.localPosition,
                        targetPos,
                        Time.deltaTime * 5f // 移动速度（可调整）
                    );

                    // 检查是否到达目标（允许微小误差）
                    if (Vector3.Distance(obj.transform.localPosition, targetPos) > 0.01f)
                    {
                        allReached = false;
                    }
                }

                // 如果所有物体都到达目标，终止移动
                if (allReached)
                {
                    IsInMove = false;
                    // 强制设置到精确位置（避免误差）
                    for (int i = 0; i < dogGetBox.Count; i++)
                    {
                        dogGetBox[i].transform.localPosition = targetPositions[i];
                    }
                }

                yield return null; // 等待下一帧
            }
        }





        IEnumerator Drop(InteractableOnlyDrop dropComponent,InteractableLootbox lootBox,Vector3 dir)
        {
            //隐藏交互
            dropComponent.gameObject.SetActive(false);

            //移动到原父物体
            MultiSceneCore.MoveToActiveWithScene(lootBox.gameObject, SceneManager.GetActiveScene().buildIndex);
            SetRigidbodyActive(true, lootBox.GetComponent<Rigidbody>(), lootBox);
            lootBox.GetComponent<Rigidbody>().velocity = dir * 5.5f + lootBox.transform.up * 0.5f;


            //List<InteractableBase> tempListBase = Traverse.Create(lootBox).Field("otherInterablesInGroup").GetValue<List<InteractableBase>>();
            //tempListBase.Remove(dropComponent);
            //baseLootBox.GetInteractableList();//更新
            //打开carry
            lootBox.GetComponentInChildren<InteractableCarriable>(true).gameObject.SetActive(true);
            yield return new WaitForSeconds(3f);
            SetRigidbodyActive(false, lootBox.GetComponent<Rigidbody>(), lootBox);


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


    }
}
