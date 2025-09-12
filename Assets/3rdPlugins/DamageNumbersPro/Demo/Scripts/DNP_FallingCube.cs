using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DamageNumbersPro.Demo
{
    public class DNP_FallingCube : MonoBehaviour
    {
        public float fallSpeed = 100f;

        RectTransform rect;
        Image image;
        float currentSpeed;
        bool isBroken;

        void Start()
        {
            rect = GetComponent<RectTransform>();
            image = GetComponent<Image>();

            float speedAndSize = Random.value * 0.4f + 0.8f;
            currentSpeed = fallSpeed * speedAndSize;
            transform.localScale = Vector3.one * speedAndSize;
            rect.anchoredPosition3D = new Vector3(Random.value * 560 - 280f, 260f, 0);
            rect.localEulerAngles = new Vector3(0, 0, Random.value * 360f);


            image.color = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), image.color, speedAndSize - 0.2f);
        }

        void FixedUpdate()
        {
            rect.anchoredPosition += new Vector2(0, currentSpeed * Time.fixedDeltaTime);

            if(isBroken)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.fixedDeltaTime * 3f);
                transform.localScale += Vector3.one * Time.fixedDeltaTime * 3f;
                Color color = image.color;
                color.a -= Time.fixedDeltaTime * 5f;

                if(color.a <= 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    image.color = color;
                }
            }

            if(rect.anchoredPosition.y < -270f)
            {
                Destroy(gameObject);
            }
        }

        public void Break()
        {
            isBroken = true;
            image.raycastTarget = false;
        }
    }
}
