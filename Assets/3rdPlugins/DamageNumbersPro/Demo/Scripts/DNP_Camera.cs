using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamageNumbersPro.Demo
{
    public class DNP_Camera : MonoBehaviour
    {
        //Instance:
        public static DNP_Camera instance;

        //Shooting:
        GameObject cubeHighlight;
        float nextShotTime;
        float nextRaycastTime;
        float lookTime;

        //Movement:
        Vector3 velocity;

        void Awake()
        {
            instance = this;

            cubeHighlight = GameObject.Find("Special").transform.Find("Prefabs/Other/Cube Highlight").gameObject;
        }

        void Update()
        {
            HandleMovement();
            HandleShooting();

            //Escape:
            if(DNP_InputHandler.GetEscape())
            {
                ShowMouse();
                Invoke("ShowMouse", 0.1f);
                Invoke("ShowMouse", 0.2f);
                Invoke("ShowMouse", 0.25f);
                Invoke("ShowMouse", 0.3f);
                CancelInvoke("HideMouse");
            }
        }

        void ShowMouse()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        void HideMouse()
        {
            if(DNP_InputHandler.GetLeftHeld())
            {
                Invoke("HideMouse", 0.1f);
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        void LateUpdate()
        {
            HandleLooking();
        }

        //Functions:
        void HandleShooting()
        {
            if(DNP_InputHandler.GetLeftClick())
            {
                Shoot();
                nextShotTime = Time.time + 0.3f;
            } else if(DNP_InputHandler.GetRightHeld() && Time.time > nextShotTime)
            {
                Shoot();
                nextShotTime = Time.time + 0.06f;
            }

            //Detection:
            if (Time.time > nextRaycastTime)
            {
                nextRaycastTime = Time.time + 0.11f;

                RaycastHit raycast;
                if (Physics.Raycast(transform.position, transform.forward, out raycast, 100))
                {
                    DNP_Crosshair.targetEnemy = raycast.collider.gameObject.layer == 1;
                }
            }
        }
        void Shoot()
        {
            if (Cursor.visible)
            {
                if(!IsInvoking("HideMouse"))
                {
                    Invoke("HideMouse", 0.1f);
                    lookTime = Time.time + 0.3f;
                }
                return;
            }

            RaycastHit raycast;

            if(Physics.Raycast(transform.position, transform.forward, out raycast, 100))
            {
                //Create Damage Number:
                DNP_PrefabSettings settings = DNP_DemoManager.instance.GetSettings();
                DamageNumber prefab = DNP_DemoManager.instance.GetCurrent();


                float number = 1 + Mathf.Pow(Random.value, 2.2f) * settings.numberRange;
                if(prefab.digitSettings.decimals == 0)
                {
                    number = Mathf.Floor(number);
                }

                DamageNumber newDamageNumber = prefab.Spawn(raycast.point, number);

                //Apply Demo Settings:
                settings.Apply(newDamageNumber);

                //Create Cube:
                if (raycast.collider.gameObject.layer != 1)
                {
                    Vector3 cubePosition = raycast.point - raycast.normal * 0.1f;
                    cubePosition.x = Mathf.FloorToInt(cubePosition.x) + 0.5f;
                    cubePosition.y = Mathf.FloorToInt(cubePosition.y) + 0.5f;
                    cubePosition.z = Mathf.FloorToInt(cubePosition.z) + 0.5f;

                    GameObject newCube = Instantiate<GameObject>(cubeHighlight);
                    newCube.transform.position = cubePosition;
                    newCube.SetActive(true);

                    DNP_Crosshair.instance.HitWall();
                }
                else
                {
                    DNP_Target target = raycast.collider.GetComponent<DNP_Target>();
                    DNP_Crosshair.instance.HitTarget();

                    if (target != null)
                    {
                        if(newDamageNumber.spamGroup != "")
                        {
                            newDamageNumber.spamGroup += target.GetInstanceID();
                        }

                        newDamageNumber.enableFollowing = true;
                        newDamageNumber.followedTarget = target.transform;

                        target.Hit();
                    }
                }
            }
        }
        void HandleLooking()
        {
            if (Time.time < lookTime || Cursor.visible) return;

            Vector2 mouseDelta = DNP_InputHandler.GetMouseDelta();

            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.y += mouseDelta.x;
            eulerAngles.x -= mouseDelta.y;

            if (eulerAngles.x > 180)
            {
                eulerAngles.x -= 360;
            }
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, -80f, 80f);

            transform.eulerAngles = eulerAngles;
        }
        void HandleMovement()
        {
            Vector3 desiredDirection = Vector3.zero;

            if (DNP_InputHandler.GetRight())
            {
                desiredDirection.x += 1;
            }
            if (DNP_InputHandler.GetLeft())
            {
                desiredDirection.x += -1;
            }
            if (DNP_InputHandler.GetForward())
            {
                desiredDirection.z += 1;
            }
            if (DNP_InputHandler.GetBack())
            {
                desiredDirection.z += -1;
            }
            if (DNP_InputHandler.GetUp())
            {
                desiredDirection.y += 1;
            }
            if (DNP_InputHandler.GetDown())
            {
                desiredDirection.y += -1;
            }

            if(desiredDirection.magnitude > 0.1f)
            {
                desiredDirection.Normalize();
            }

            velocity = Vector3.Lerp(velocity, (desiredDirection.z * transform.forward + desiredDirection.x * transform.right + desiredDirection.y * Vector3.up) * 7f, Time.deltaTime * 6f);
            
            Vector3 position = transform.position;
            Vector3 clampedPosition = new Vector3(Mathf.Clamp(position.x, -4f, 4f), Mathf.Clamp(position.y, 1f, 6f), Mathf.Clamp(position.z, -12f, 4f));
            position = Vector3.Lerp(position, clampedPosition, Time.deltaTime * 15f) + velocity * Time.deltaTime;

            //Apply:
            transform.position = position;
        }
    }
}