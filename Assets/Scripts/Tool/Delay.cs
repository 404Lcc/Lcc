using UnityEngine;

public class Delay : MonoBehaviour
{
    public float time = 1;
    void Start()
    {
        gameObject.SetActive(false);
        Invoke("DelayFunction", time);
    }
    void DelayFunction()
    {
        gameObject.SetActive(true);
    }
}