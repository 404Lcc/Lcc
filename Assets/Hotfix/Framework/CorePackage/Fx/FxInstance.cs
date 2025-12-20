using UnityEngine;

namespace LccHotfix
{
    public class FxInstance : MonoBehaviour
    {
        System.Action particleStopCallback;

        public void SetFxStopCallback(System.Action callback)
        {
            particleStopCallback = callback;
        }

        void OnParticleSystemStopped()
        {
            if (particleStopCallback != null)
            {
                particleStopCallback.Invoke();
            }
        }
    }
}