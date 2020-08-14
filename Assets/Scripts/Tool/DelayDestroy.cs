using UnityEngine;

public class DelayDestroy : MonoBehaviour
{
    public float time = 1;
    void Start()
    {
        Invoke("DelayFunction", time);
    }
    void DelayFunction()
    {
        Destroy(gameObject);
    }
}