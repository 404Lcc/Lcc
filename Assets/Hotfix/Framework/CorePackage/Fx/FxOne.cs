using System;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class FxOne : MonoBehaviour
    {
        public bool bHidding = true;

        public bool bPlaying = false;
        public bool bCanReplay = false;
        public bool bIsReleased = false;
        public bool bIsLoop = false;

        public FxCache fxCache = null;
        public EFxOneType fxType;
        
        float fxLifetime = -1;
        GameObject fxGameObject = null;
        ParticleSystem fxParticleSystem = null;
        FxInstance fxInstance = null;

        public void SetFxGameObject(GameObject _fxGameObject)
        {
            if (_fxGameObject == null)
            {
                Log.Error("_fxGameObject == null");
                return;
            }
            fxGameObject = _fxGameObject;

            if (_fxGameObject.TryGetComponent(out fxParticleSystem))
            {
                fxParticleSystem = _fxGameObject.GetComponent<ParticleSystem>();
                fxInstance = _fxGameObject.AddComponent<FxInstance>();

                var main = fxParticleSystem.main;
                main.stopAction = ParticleSystemStopAction.Callback;
                fxInstance.SetFxStopCallback(OnParticleSystemStopped);
            }
        }

        public void SetHiddenInGame(bool hidden)
        {
            bHidding = hidden;

            gameObject.SetActive(!bHidding);
        }

        public void Update()
        {
            if (bPlaying && fxLifetime > 0 && !bIsLoop)
            {
                var dt = Time.deltaTime;
                fxLifetime -= dt;
                if (fxLifetime <= 0)
                {
                    Release();
                    return;
                }
            }
        }

        public void Play(float inLifetime = -1.0f)
        {
            if (!fxCache.isActiveAndEnabled || bIsReleased)
                return;

            if (inLifetime <= -999)
            {
                bIsLoop = true;
            }

            fxLifetime = inLifetime;
            bPlaying = true;
        }

        void OnParticleSystemStopped()
        {
            Release();
        }

        public void Release()
        {
            if (!fxCache.isActiveAndEnabled || bIsReleased)
                return;

            if (bPlaying)
            {
                Stop();
            }
            this.transform.SetParent(fxCache.transform);
            fxCache.ReleaseFx(this);
        }

        public void SetFxLifetime(float inLifetime)
        {
            fxLifetime = inLifetime;
            bIsLoop = false;
        }

        private void Stop()
        {
            if (!fxCache.isActiveAndEnabled || bIsReleased)
                return;

            bPlaying = false;
            fxLifetime = -1.0f;

            if (fxGameObject != null)
            {
                SetHiddenInGame(true);
            }
        }
    }
}