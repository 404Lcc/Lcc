using System;
using UnityEngine;

namespace LccHotfix
{
    public class FxOne : MonoBehaviour
    {
        public bool bHidding = true;
        public bool bFadingOut = false;

        public bool bPlaying = false;
        public bool bCanReplay = false;
        public bool bIsReleased = false;
        public bool bIsLoop = false;

        public FxCache fxCache = null;
        public EFxOneType fxType;
        // 入池前回调一次（如分级占位归还）；调用后清空
        public Action<FxOne> OnReleased;
        // 分级调度占位 path；Bind 写入，OnReleased 归还后由调度器清空
        public string VfxGradePath;

        float fxTime = -1;
        GameObject fxGameObject = null;
        ParticleSystem fxParticleSystem = null;
        FxInstance fxInstance = null;
        // 已绑定过特效内容；DestructionEffect 会 Destroy 内容 GO，用于检测并提前归还
        private bool _hasFxContent;

        public void SetFxGameObject(GameObject _fxGameObject)
        {
            if (_fxGameObject == null)
            {
                KLogger.LogError("_fxGameObject == null");
                return;
            }
            fxGameObject = _fxGameObject;
            _hasFxContent = true;

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
            // DestructionEffect 播完会 Destroy 内容；须立刻归还池与分级名额，避免 active 计数泄漏
            if (bPlaying && !bIsReleased && _hasFxContent && fxGameObject == null)
            {
                ClearDestroyedContentRefs();
                Release();
                return;
            }

            if (bPlaying && fxTime > 0 && !bIsLoop)
            {
                fxTime -= Time.deltaTime;
                if (fxTime <= 0)
                {
                    if (bFadingOut || fxParticleSystem == null)
                    {
                        Release();
                        return;
                    }
                    else
                    {
                        ParticleFadeOut(1f);
                        return;
                    }
                }
            }
        }

        public void Play(float inTime = -1.0f)
        {
            if (!fxCache.isActiveAndEnabled || bIsReleased)
                return;

            // 内容曾被 DestructionEffect Destroy：从缓存模板重新实例化后再播
            if (fxGameObject == null)
            {
                ClearDestroyedContentRefs();
                fxCache.RecreateFxGameObject(this);
            }

            if (inTime <= -999)
            {
                bIsLoop = true;
            }

            fxTime = inTime;
            bPlaying = true;
            bFadingOut = false;


        }

        void OnParticleSystemStopped()
        {
            if (!bFadingOut)
            {
                Release();
            }
        }

        public void Release()
        {
            if (fxCache == null || !fxCache.isActiveAndEnabled || bIsReleased)
                return;

            if (bPlaying)
            {
                Stop();
            }

            this.transform.SetParent(fxCache.transform);
            fxCache.ReleaseFx(this);

            var releasedCb = OnReleased;
            OnReleased = null;
            releasedCb?.Invoke(this);
        }

        // Unity 已 Destroy 的内容引用清成显式 null，并允许下次 Play 重建
        private void ClearDestroyedContentRefs()
        {
            fxGameObject = null;
            fxParticleSystem = null;
            fxInstance = null;
            _hasFxContent = false;
        }

        public void SetFxLifeTime(float lifeTime)
        {
            fxTime = lifeTime;
            bIsLoop = false;
        }

        private void Stop()
        {
            if (fxCache == null || !fxCache.isActiveAndEnabled || bIsReleased)
                return;

            bPlaying = false;
            fxTime = -1.0f;

            if (fxGameObject != null)
            {
                SetHiddenInGame(true);
            }
        }

        /// <summary>
        /// 停止新粒子的生成，然后等一会再停
        /// </summary>
        public void ParticleFadeOut(float duration)
        {
            if (fxParticleSystem != null)
            {
                fxParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                bFadingOut = true;
            }
            SetFxLifeTime(duration);
        }
    }
}
