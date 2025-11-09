using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public enum UIAnimType
{
    [LabelText("动画片段")] Animation = 0,
}

public enum UIAnimMultiAutoPlayMode
{
    [LabelText("顺序播放")] Sequential = 0,
    [LabelText("并行播放")] Parallel = 1,
}

public class UIAnimManager : MonoBehaviour
{
    [LabelText("开启自动播放")] public bool isAutoPlay;

    [LabelText("多动画自动播放模式")] [ShowIf("@animList.Count > 1")]
    public UIAnimMultiAutoPlayMode multiAutoPlayMode;

    [LabelText("自定义动画列表")] [ListDrawerSettings(ShowIndexLabels = true, OnBeginListElementGUI = "OnBeginListElementGUI", OnEndListElementGUI = "OnEndListElementGUI")] [OnValueChanged("OnChangeAnimList", true)]
    public List<UIAnimInfo> animList = new List<UIAnimInfo>();

    private int _seqCurIndex;
    private Action _sequentialCallback;

    private Action _parallelCallback;
    private List<int> _parallelIndex = new List<int>();

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
    }

    void OnEnable()
    {
        AutoPlay();
    }

    void OnDestroy()
    {
        for (int i = 0; i < animList.Count; i++)
        {
            animList[i].ClearAnim();
        }

        animList.Clear();
    }



    private void AutoPlay()
    {
        if (!isAutoPlay)
            return;

        if (animList.Count == 0)
            return;

        if (animList.Count > 1)
        {
            switch (multiAutoPlayMode)
            {
                case UIAnimMultiAutoPlayMode.Sequential:
                    PlaySequential();
                    break;
                case UIAnimMultiAutoPlayMode.Parallel:
                    PlayParallel();
                    break;
            }
        }
        else
        {
            PlaySingle();
        }
    }

    public void PlaySingle(int index = 0, bool isForward = true, Action callBack = null)
    {
        if (animList.Count == 0)
            return;

        if (index < 0 || index >= animList.Count)
            return;

        gameObject.SetActive(true);

        var animInfo = animList[index];
        animInfo.ResetAnim(isForward);
        animInfo.PlayAnim(isForward, callBack);
    }

    public void PlaySequential(bool isForward = true, Action callBack = null)
    {
        if (animList.Count == 0)
            return;

        gameObject.SetActive(true);

        foreach (var item in animList)
        {
            item.isLoop = false;
            item.ResetAnim(isForward);
        }

        _seqCurIndex = 0;
        _sequentialCallback = callBack;

        PlaySingle(_seqCurIndex, isForward, () => OnSequentialComplete(isForward));
    }

    private void OnSequentialComplete(bool isForward)
    {
        _seqCurIndex++;
        if (_seqCurIndex >= animList.Count)
        {
            _seqCurIndex = 0;
            _sequentialCallback?.Invoke();
            _sequentialCallback = null;
            return;
        }

        PlaySingle(_seqCurIndex, isForward, () => OnSequentialComplete(isForward));
    }

    public void PlayParallel(bool isForward = true, Action callBack = null)
    {
        if (animList.Count == 0)
            return;

        gameObject.SetActive(true);

        _parallelCallback = callBack;
        _parallelIndex.Clear();

        for (int i = 0; i < animList.Count; i++)
        {
            int index = i;
            PlaySingle(index, isForward, () => OnParallelComplete(index));
            _parallelIndex.Add(index);
        }
    }

    private void OnParallelComplete(int index)
    {
        _parallelIndex.Remove(index);

        if (_parallelIndex.Count == 0)
        {
            _parallelCallback?.Invoke();
            _parallelCallback = null;
        }
    }

    public void ResetAnim(int index = 0, bool isForward = true)
    {
        if (animList.Count == 0)
            return;

        if (index < 0 || index >= animList.Count)
            return;

        var animInfo = animList[index];
        animInfo.ResetAnim(isForward);
    }

    public void StopAnim(int index = 0)
    {
        if (animList.Count == 0)
            return;

        if (index < 0 || index >= animList.Count)
            return;

        var animInfo = animList[index];
        animInfo.ClearAnim();
    }
}