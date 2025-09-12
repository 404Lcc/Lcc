using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DamageNumbersPro.Demo
{
    public class DNP_SineFadeText : MonoBehaviour
    {
        public float fromAlpha = 0.5f;
        public float toAlpha = 0.8f;
        public float speed = 4f;
        public float startTimeBonus = 0f;

        Text text;

        void Awake()
        {
            text = GetComponent<Text>();
        }

        void FixedUpdate()
        {
            Color color = text.color;
            color.a = fromAlpha + (toAlpha - fromAlpha) * (Mathf.Sin(speed * Time.unscaledTime + startTimeBonus) * 0.5f + 0.5f);
            text.color = color;
        }
    }
}
