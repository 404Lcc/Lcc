using UnityEngine;

namespace Model
{
    public class Delay : MonoBehaviour
    {
        public float time = 1;
        void Awake()
        {
            gameObject.SetActive(false);
            Invoke("DelayFunction", time);
        }
        void DelayFunction()
        {
            gameObject.SetActive(true);
        }
    }
}