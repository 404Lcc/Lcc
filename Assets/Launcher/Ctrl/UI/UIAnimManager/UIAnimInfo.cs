using System;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class UIAnimInfo
{
    [LabelText("延迟时间（毫秒）")] public float delayTime;
    [LabelText("目标")] public Transform transTarget;
    [LabelText("效果类型")] public UIAnimType animType;

    [LabelText("动画片段")] [ShowIf("@animType == UIAnimType.Animation")]
    public AnimationClip anim;

    [LabelText("循环")] public bool isLoop;

    private UIAnimCtrl _animCtrl;

    public string AniName
    {
        get
        {
            if (anim == null)
                return string.Empty;
            return anim.name;
        }
    }

    public void PlayAnim(bool isForward, Action callBack)
    {
        switch (animType)
        {
            case UIAnimType.Animation:
                if (anim == null)
                    break;

                _animCtrl = transTarget.GetComponent<UIAnimCtrl>();
                if (_animCtrl == null)
                {
                    _animCtrl = transTarget.gameObject.AddComponent<UIAnimCtrl>();
                }

                _animCtrl.PlayAnim(this, isForward, callBack);
                break;
        }
    }

    public void StopAnim(bool isForward)
    {
        switch (animType)
        {
            case UIAnimType.Animation:

                if (_animCtrl == null)
                    break;

                _animCtrl.StopAnim(isForward);
                break;
        }
    }
}