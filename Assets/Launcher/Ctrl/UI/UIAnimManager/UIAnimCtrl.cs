using UnityEngine;
using System;

public class UIAnimCtrl : MonoBehaviour
{
    private Action _callBack;
    public float _timer;
    public float _duration;
    private UIAnimInfo _animInfo;

    private Animation _animation;

    private Animation Animation
    {
        get
        {
            if (_animation == null)
            {
                _animation = gameObject.GetComponent<Animation>();
                if (_animation == null)
                {
                    _animation = gameObject.gameObject.AddComponent<Animation>();
                }
            }

            return _animation;
        }
    }

    void Update()
    {
        if (_animInfo == null)
            return;

        if (_duration <= 0)
            return;

        float deltaTime = _animInfo.ignoreTimeScale ? (Time.unscaledDeltaTime > 0.04f ? 0.04f : Time.unscaledDeltaTime) : (Time.deltaTime > 0.04f ? 0.04f : Time.deltaTime);
        _timer += deltaTime;

        //需要忽略TimeScale的动画手动采样一下
        if (_animInfo.ignoreTimeScale)
        {
            Animation[_animInfo.AniName].normalizedTime = _timer / _duration;
            Animation.Sample();
        }

        if (_timer >= _duration)
        {
            if (!_animInfo.isLoop)
            {
                //缓存一下
                var temp = _callBack;
                Clear();
                temp?.Invoke();
            }
            else
            {
                _timer = 0;
            }
        }
    }

    public void PlayAnim(UIAnimInfo animInfo, bool isForward = true, Action callBack = null)
    {
        gameObject.SetActive(true);

        _callBack = callBack;
        _timer = 0;
        _duration = 0;
        _animInfo = animInfo;

        switch (_animInfo.animType)
        {
            case UIAnimType.Animation:
                SetupState(isForward);
                Animation.Play(_animInfo.AniName);
                break;
        }
    }

    private void SetupState(bool isForward)
    {
        var clip = _animInfo.anim;
        clip.legacy = true;
        _duration = clip.length;

        if (Animation.GetClip(_animInfo.AniName) == null)
        {
            Animation.AddClip(clip, _animInfo.AniName);
        }

        var animationState = Animation[_animInfo.AniName];
        animationState.time = isForward ? 0f : _duration;
        animationState.speed = isForward ? 1f : -1f;
        Animation.wrapMode = _animInfo.isLoop ? WrapMode.Loop : WrapMode.Once;
    }

    public void StopAnim(bool isForward = true)
    {
        if (_animInfo == null)
            return;

        if (_duration <= 0)
            return;

        SetupState(isForward);
        Animation.Play(_animInfo.AniName);
        Animation.Sample();
        Animation.Stop(_animInfo.AniName);

        Clear();
    }

    private void Clear()
    {
        _callBack = null;
        _timer = 0;
        _duration = 0;
        _animInfo = null;

        Animation.wrapMode = WrapMode.Once;
    }
}