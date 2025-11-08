using UnityEngine;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

public enum UIAnimType
{
    [LabelText("动画片段")] Animation = 0,
}

public class UIAnimManager : MonoBehaviour
{
    private bool _isForward = true;
    private float _timer;
    private int _curIndex;
    private bool _canUpdate;
    private UIAnimInfo _animInfo;
    private UIAnimInfo _lastAnimInfo;

    [LabelText("开启自动播放")] public bool isAutoPlay;

    [LabelText("自定义动画列表")] [ListDrawerSettings(ShowIndexLabels = true, OnBeginListElementGUI = "OnBeginListElementGUI", OnEndListElementGUI = "OnEndListElementGUI")] [OnValueChanged("OnChangeAnimList", true)]
    public List<UIAnimInfo> animSenquenceList = new List<UIAnimInfo>();

    public Action PlayOverCallBack { get; set; }


    private void OnBeginListElementGUI(int index)
    {
#if UNITY_EDITOR
        Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox("第" + index + "个动画");
#endif
    }

    private void OnEndListElementGUI(int index)
    {
#if UNITY_EDITOR
        Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
#endif
    }

    private void OnChangeAnimList()
    {
        for (int i = 0; i < animSenquenceList.Count; i++)
        {
            UIAnimInfo info = animSenquenceList[i];
            if (i > 0)
            {
                UIAnimInfo lastInfo = animSenquenceList[i - 1];
                if (lastInfo.isLoop && info.isWaitForLast)
                {
                    info.isWaitForLast = false;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.DisplayDialog("提醒", "上一个动画循环，当前动画无法选择等待结束", "确定");
#endif
                }
            }
            else if (i == 0 && info.isWaitForLast)
            {
                info.isWaitForLast = false;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("提醒", "第一个动画无法使用等待结束", "确定");
#endif
            }
        }
    }

    void OnEnable()
    {
        AutoPlay();
    }

    void OnDestroy()
    {
        for (int i = 0; i < animSenquenceList.Count; i++)
        {
            animSenquenceList[i].ClearAnim();
        }

        animSenquenceList.Clear();
    }

    void Update()
    {
        if (!_canUpdate)
            return;

        if (TryPlayAnim())
        {
            _animInfo.PlayAnim(_isForward);

            if (!_animInfo.isPlaySingle)
            {
                SetAnimInfo(_curIndex + 1);
            }
            else
            {
                _lastAnimInfo = _animInfo;
                _animInfo = null;
            }
        }
    }

    private void AutoPlay()
    {
        if (!isAutoPlay)
            return;

        if (animSenquenceList.Count == 0)
            return;

        PlayAnim();
    }

    public void PlayAnim(int index = 0, bool isForward = true)
    {
        gameObject.SetActive(true);

        _isForward = isForward;
        _timer = 0;

        _animInfo = null;
        _lastAnimInfo = null;

        SetAnimInfo(index);
        ResetAnim(index, isForward);
        _canUpdate = true;
    }

    private void SetAnimInfo(int index)
    {
        _curIndex = index;
        _lastAnimInfo = _animInfo;

        if (_curIndex < animSenquenceList.Count)
        {
            _animInfo = animSenquenceList[_curIndex];
        }
        else
        {
            _animInfo = null;
        }
    }

    public void ResetAnim(int index = 0, bool isForward = true)
    {
        if (animSenquenceList.Count == 0)
            return;

        if (index < 0 || index >= animSenquenceList.Count)
        {
            foreach (var item in animSenquenceList)
            {
                item.ResetAnim(isForward);
            }

            return;
        }

        animSenquenceList[index].ResetAnim(isForward);
    }

    public void StopAnim()
    {
        _canUpdate = false;

        if (_animInfo != null)
        {
            _animInfo.ClearAnim();
            _animInfo = null;
        }

        if (_lastAnimInfo != null)
        {
            _lastAnimInfo.ClearAnim();
            _lastAnimInfo = null;
        }
    }


    private bool TryPlayAnim()
    {
        float deltaTime = Time.deltaTime > 0.04f ? 0.04f : Time.deltaTime;
        _timer += deltaTime;

        if (_animInfo == null)
        {
            if (_lastAnimInfo != null && _lastAnimInfo.IsPlayOver)
            {
                PlayOverCallBack?.Invoke();
                StopAnim();
            }

            return false;
        }

        if (_animInfo.isWaitForLast)
        {
            if (_lastAnimInfo != null && !_lastAnimInfo.IsPlayOver)
            {
                _timer = 0;
                return false;
            }
        }


        if (_timer > _animInfo.delayTime)
        {
            _timer -= _animInfo.delayTime;
            return true;
        }

        return false;
    }
}