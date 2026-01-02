using UnityEngine;

namespace LccHotfix
{
    public class FxOne : MonoBehaviour
    {
        public bool bHidding = true;

        public bool bPlaying = false;
        public bool bCanReplay = false;
        public bool bIsReleased = false;

        public FxCache fxCache = null;
        public EFxOneType fxType;

        float fxLifetime = -1;
        GameObject fxGameObject = null;
        ParticleSystem fxParticleSystem = null;
        FxInstance fxInstance = null;

        private Color? _cacheColor;

        public void SetFxGameObject(GameObject _fxGameObject)
        {
            fxGameObject = _fxGameObject;

            fxParticleSystem = _fxGameObject.GetComponentInChildren<ParticleSystem>();

            if (fxParticleSystem)
            {
                var main = fxParticleSystem.main;
                if (main.stopAction == ParticleSystemStopAction.Callback)
                {
                    fxInstance = fxParticleSystem.gameObject.AddComponent<FxInstance>();
                    fxInstance.SetFxStopCallback(OnParticleSystemStopped);
                }

                if (_cacheColor != null)
                {
                    SetColor(_cacheColor);
                }
            }
        }

        public void SetHiddenInGame(bool hidden)
        {
            bHidding = hidden;

            gameObject.SetActive(!bHidding);
        }

        public void Update()
        {
            if (bPlaying && fxLifetime > 0)
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

            fxLifetime = inLifetime;
            bPlaying = true;
            fxParticleSystem?.Play();
        }

        public void SetColor(Color? color)
        {
            if (fxParticleSystem == null)
            {
                _cacheColor = color;
                return;
            }

            if (color != null)
            {
                var main = fxParticleSystem.main;
                main.startColor = color.Value;
            }
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

            fxCache.ReleaseFx(this);
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

        /// <summary>
        /// 用来停止 Particle System 的内容，并附着延迟回收逻辑
        /// 目前用于处理特效 Stop 但是不会立马销毁的情况
        /// </summary>
        public void StopParticleSystem(float delayReleaseTime = 1.0f)
        {
            // 立即停止播放
            fxParticleSystem?.Stop();

            // 设置剩余生命周期
            fxLifetime = delayReleaseTime;
        }
    }
}