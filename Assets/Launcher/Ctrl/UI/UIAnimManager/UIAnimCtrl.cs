using UnityEngine;
using System;

public class UIAnimCtrl : MonoBehaviour
{
    private Action _callBack;
    private float _timer;
    private float _duration;
    private Animation _animation;
    private UIAnimInfo _animInfo;

    public bool IsPlayOver { get; private set; }

    void Update()
    {
        if (_duration <= 0)
            return;

        float deltaTime = Time.deltaTime > 0.04f ? 0.04f : Time.deltaTime;
        _timer += deltaTime;

        if (_timer >= _duration)
        {
            if (!_animInfo.isLoop)
            {
                IsPlayOver = true;
                _callBack?.Invoke();
                ClearAnim();
            }
            else
            {
                _timer = 0;
            }
        }
    }

    public void PlayAnim(UIAnimInfo animInfo, bool isForward, Action callBack = null)
    {
        _animInfo = animInfo;

        if (!TrySetupAnimation(animInfo.transTarget))
            return;

        IsPlayOver = false;
        _callBack = callBack;
        _timer = 0;
        _duration = 0;
        switch (_animInfo.animType)
        {
            case UIAnimType.Animation:
                var clip = TryGetClip();
                if (clip == null)
                    break;

                SetupClip(clip, isForward);
                _animation.Play(_animInfo.AniName);
                break;
        }
    }


    private bool TrySetupAnimation(Transform target)
    {
        _animation = target.GetComponent<Animation>();
        if (_animation == null)
        {
            _animation = target.gameObject.AddComponent<Animation>();
        }

        return _animation != null;
    }

    private AnimationClip TryGetClip()
    {
        var clip = _animInfo.anim;
        clip.legacy = true;
        if (_animation.GetClip(clip.name) == null)
        {
            _animation.AddClip(clip, clip.name);
        }

        return clip;
    }

    private void SetupClip(AnimationClip clip, bool isForward)
    {
        _duration = clip.length;

        var animationState = _animation[_animInfo.AniName];
        animationState.time = isForward ? 0f : _duration;
        animationState.speed = isForward ? 1f : -1f;
        _animation.wrapMode = _animInfo.isLoop ? WrapMode.Loop : WrapMode.Once;
    }

    public void ResetAnim(bool isForward)
    {
        IsPlayOver = false;

        if (_animation == null)
            return;

        var animationState = _animation[_animInfo.AniName];
        _animation.Stop(_animInfo.AniName);
        animationState.time = isForward ? 0f : animationState.clip.length;
        _animation.Sample();
    }

    public void ClearAnim()
    {
        IsPlayOver = false;
        _callBack = null;
        _timer = 0;
        _duration = 0;
        _animation.wrapMode = WrapMode.Once;
    }
}