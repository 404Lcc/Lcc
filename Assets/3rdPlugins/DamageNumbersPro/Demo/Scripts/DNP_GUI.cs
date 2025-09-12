using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamageNumbersPro.Demo
{
    public class DNP_GUI : MonoBehaviour
    {
        public static DNP_GUI instance;

        float nextShotTime;
        RectTransform canvasRect;

        void Awake()
        {
            instance = this;
            canvasRect = GetComponent<RectTransform>();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        void Update()
        {
            HandleShooting();
        }

        void HandleShooting()
        {
            if (DNP_UIArea.CanSpawn() == false) return;

            if (DNP_InputHandler.GetLeftClick())
            {
                Shoot();
                nextShotTime = Time.time + 0.3f;
            }
            else if (DNP_InputHandler.GetRightHeld() && Time.time > nextShotTime)
            {
                Shoot();
                nextShotTime = Time.time + 0.06f;
            }
        }

        void Shoot()
        {
            //Select Damage Number:
            DNP_PrefabSettings settings = DNP_DemoManager.instance.GetSettings();
            DamageNumber prefab = DNP_DemoManager.instance.GetCurrent();
            DNP_UIArea.OnSpawn();

            //Number:
            float number = 1 + Mathf.Pow(Random.value, 2.2f) * settings.numberRange;
            if (prefab.digitSettings.decimals == 0)
            {
                number = Mathf.Floor(number);
            }

            //Get Parent:
            RectTransform rectParent = DNP_UIArea.GetRect();
            if (rectParent == null)
            {
                rectParent = canvasRect;
            }

            //Create Damage Number:
            DamageNumber newDamageNumber = prefab.Spawn(Vector3.zero, number);
            newDamageNumber.SetToMousePosition(rectParent, Camera.main);

            if(rectParent != canvasRect)
            {
                newDamageNumber.enableFollowing = true;
                newDamageNumber.followedTarget = rectParent;
            }

            //Apply Demo Settings:
            settings.Apply(newDamageNumber);
        }
    }
}