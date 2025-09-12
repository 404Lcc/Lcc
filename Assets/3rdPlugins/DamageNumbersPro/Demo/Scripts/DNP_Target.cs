using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamageNumbersPro.Demo
{
    public class DNP_Target : MonoBehaviour
    {
        public Vector3 movementOffset = new Vector3(0, 0, 0);

        Material mat;
        float defaultBrightness;

        Coroutine hitRoutine;
        Coroutine flipRoutine;
        bool flipping;

        Vector3 originalPosition;

        void Start()
        {
            mat = GetComponent<MeshRenderer>().material;
            defaultBrightness = mat.GetFloat("_Brightness");

            flipping = false;

            originalPosition = transform.position;
        }

        void Update()
        {
            //Move around.
            transform.position = originalPosition + movementOffset * Mathf.Sin(Time.time);
        }

        public void Hit()
        {
            if(hitRoutine != null)
            {
                StopCoroutine(hitRoutine);
            }

            hitRoutine = StartCoroutine(HitCoroutine());

            if (!flipping)
            {
                if (flipRoutine != null)
                {
                    StopCoroutine(flipRoutine);
                }

                flipRoutine = StartCoroutine(FlipCoroutine());
            }
        }

        IEnumerator HitCoroutine()
        {
            float brightness = 1f;

            while( brightness < 3f)
            {
                //Glow up.
                brightness = Mathf.Min(3, Mathf.Lerp(brightness, 3 + 0.1f, Time.deltaTime * 20f));
                mat.SetFloat("_Brightness", brightness);

                yield return null;
            }

            while(brightness > defaultBrightness)
            {
                //Glow down.
                brightness = Mathf.Max(defaultBrightness, Mathf.Lerp(brightness, defaultBrightness - 0.1f, Time.deltaTime * 10f));
                mat.SetFloat("_Brightness", brightness);

                yield return null;
            }
        }

        IEnumerator FlipCoroutine()
        {
            flipping = true;

            float angle = 0f;

            while(angle < 180f)
            {
                angle = Mathf.Min(180, Mathf.Lerp(angle, 190f, Time.deltaTime * 7f));
                transform.eulerAngles = new Vector3(angle, 0, 0);
                yield return null;

                if(angle > 150f)
                {
                    flipping = false;
                }
            }
        }
    }
}
