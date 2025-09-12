using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DamageNumbersPro.Demo
{
    public class DNP_Crosshair : MonoBehaviour
    {
        public static DNP_Crosshair instance;
        public static bool targetEnemy;

        public Color defaultColor = new Color(1, 1, 1, 0.6f);
        public float defaultScale = 1f;

        public Color enemyColor = new Color(1, 0.2f, 0.2f, 0.8f);
        public float enemyScale = 1.15f;

        Image image;

        void Awake()
        {
            instance = this;
            image = GetComponent<Image>();
        }

        void FixedUpdate()
        {
            if(Cursor.visible)
            {
                image.color = Color.Lerp(image.color, new Color(1,1,1,0), Time.fixedDeltaTime * 7f);
            }
            else if(targetEnemy)
            {
                image.color = Color.Lerp(image.color, enemyColor, Time.fixedDeltaTime * 7f);

                float scale = Mathf.Lerp(transform.localScale.x, enemyScale, Time.fixedDeltaTime * 7f);
                transform.localScale = new Vector3(scale, scale, 1);

            }
            else
            {
                image.color = Color.Lerp(image.color, defaultColor, Time.fixedDeltaTime * 7f);

                float scale = Mathf.Lerp(transform.localScale.x, defaultScale, Time.fixedDeltaTime * 7f);
                transform.localScale = new Vector3(scale, scale, 1);
            }
        }

        public void HitTarget()
        {
            transform.localScale = new Vector3(1.7f, 1.7f, 1f);
            image.color = Color.red;
        }

        public void HitWall()
        {
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            image.color = Color.white;
        }
    }

}
