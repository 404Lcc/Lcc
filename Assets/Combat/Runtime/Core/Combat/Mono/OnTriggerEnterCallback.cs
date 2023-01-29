using UnityEngine;
using System;

namespace LccModel
{
    public class OnTriggerEnterCallback : MonoBehaviour
    {
        public Action<Collider> triggerEnterAction;


        private void OnTriggerEnter(Collider other)
        {
            triggerEnterAction?.Invoke(other);
        }
    }
}