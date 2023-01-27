using UnityEngine;
using System;

namespace LccModel
{
    public class OnTriggerEnterCallback : MonoBehaviour
    {
        public Action<Collider> OnTriggerEnterCallbackAction;


        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterCallbackAction?.Invoke(other);
        }
    }
}