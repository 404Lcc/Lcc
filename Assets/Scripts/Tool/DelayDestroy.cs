using UnityEngine;

public class DelayDestroy : MonoBehaviour
{
    public float time = 1.0f;
    void Start()
    {
        Invoke("DelayFunction", time);
    }
    private void DelayFunction()
    {
        Destroy(gameObject);
    }
}