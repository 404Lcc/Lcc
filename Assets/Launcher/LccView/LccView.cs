using UnityEngine;

namespace LccModel
{
    public class LccView : MonoBehaviour
    {
        [HideInInspector]
        public string className;
        public object type;
        public T GetType<T>()
        {
            return (T)type;
        }
    }
}