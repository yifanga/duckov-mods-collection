using Duckov.Scenes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DuckovBetterRealDog
{
    public class InteractableOnlyDrop : InteractableBase
    {

        //bool isOnPet = false;

        public Action<InteractableOnlyDrop> whenDrop;

        protected override bool IsInteractable()
        {
            return true;
        }

        protected override void OnInteractFinished()
        {
            if (!this.interactCharacter)
            {
                return;
            }
            //if (isOnPet)//卸下
            {



                //GameObject.Destroy(this.interactCollider);
                //StartCoroutine(Drop(transform.forward));


                whenDrop?.Invoke(this);



            }

        }

        public void DoDropForce(Vector3 dir)
        {
            GameObject.Destroy(this.interactCollider);
            StartCoroutine(Drop(dir.normalized, false));
        }


        IEnumerator Drop(Vector3 dir, bool dropInvoke = true)
        {
            //移动到原父物体
            InteractableLootbox baseLootBox = transform.parent.GetComponent<InteractableLootbox>();
            MultiSceneCore.MoveToActiveWithScene(transform.parent.gameObject, SceneManager.GetActiveScene().buildIndex);
            SetRigidbodyActive(true, transform.parent.GetComponent<Rigidbody>(), baseLootBox);
            transform.parent.GetComponent<Rigidbody>().velocity = dir * 5.5f + transform.up * 0.5f;

            List<InteractableBase> tempListBase = Traverse.Create(baseLootBox).Field("otherInterablesInGroup").GetValue<List<InteractableBase>>();
            tempListBase.Remove(this);
            baseLootBox.GetInteractableList();//更新
            baseLootBox.GetComponentInChildren<InteractableCarriable>(true).gameObject.SetActive(true);

            //if(dropInvoke)
            //    whenDrop?.Invoke(this);

            yield return new WaitForSeconds(3f);
            SetRigidbodyActive(false, transform.parent.GetComponent<Rigidbody>(), baseLootBox);


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
