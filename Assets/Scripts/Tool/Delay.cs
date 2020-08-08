using UnityEngine;

public class Delay : MonoBehaviour
{
    public float time = 1.0f;
    void Start()
    {
        gameObject.SetActive(false);
        Invoke("DelayFunction", time);
    }
    private void DelayFunction()
    {
        gameObject.SetActive(true);
    }
}