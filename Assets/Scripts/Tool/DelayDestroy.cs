using UnityEngine;

namespace Model
{
    public class DelayDestroy : MonoBehaviour
    {
        public float time = 1;
        void Awake()
        {
            Invoke("DelayFunction", time);
        }
        void DelayFunction()
        {
            Destroy(gameObject);
        }
    }
}