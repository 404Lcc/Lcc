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

    void OnEnable()
    {
        _animation = gameObject.GetComponent<Animation>();
        if (_animation == null)
        {
            _animation = gameObject.gameObject.AddComponent<Animation>();
        }
    }

    void Update()
    {
        if (_animation == null)
            return;

        if (_animInfo == null)
            return;

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
                Clear();
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

        _animInfo = animInfo;

        IsPlayOver = false;
        _callBack = callBack;
        _timer = 0;
        _duration = 0;
        switch (_animInfo.animType)
        {
            case UIAnimType.Animation:
                SetupState(isForward);
                _animation.Play(_animInfo.AniName);
                break;
        }
    }

    private void SetupState(bool isForward)
    {
        var clip = _animInfo.anim;
        clip.legacy = true;
        if (_animation.GetClip(_animInfo.AniName) == null)
        {
            _animation.AddClip(clip, _animInfo.AniName);
        }

        _duration = clip.length;
        var animationState = _animation[_animInfo.AniName];
        animationState.time = isForward ? 0f : _duration;
        animationState.speed = isForward ? 1f : -1f;
        _animation.wrapMode = _animInfo.isLoop ? WrapMode.Loop : WrapMode.Once;
    }

    public void StopAnim(bool isForward = true)
    {
        if (_animation == null)
            return;

        if (_animInfo == null)
            return;

        if (_duration <= 0)
            return;

        var animationState = _animation[_animInfo.AniName];
        if (animationState == null)
            return;

        _animation.Stop(_animInfo.AniName);
        animationState.time = isForward ? 0f : animationState.clip.length;
        _animation.Sample();

        Clear();
    }

    private void Clear()
    {
        IsPlayOver = false;
        _callBack = null;
        _timer = 0;
        _duration = 0;
        if (_animation != null)
        {
            _animation.wrapMode = WrapMode.Once;
        }

        _animInfo = null;
    }
}