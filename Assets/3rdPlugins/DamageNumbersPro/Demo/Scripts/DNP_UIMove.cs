using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNP_UIMove : MonoBehaviour
{
    public Vector2 fromPosition;
    public Vector2 toPosition;
    public float frequency = 4f;

    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, Mathf.Sin(Time.time * frequency) * 0.5f + 0.5f);
    }
}
