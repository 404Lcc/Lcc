using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class UIAnimInfo
{
    [LabelText("等待上一个动画结束")] public bool isWaitForLast;
    [LabelText("延迟时间（毫秒）")] public float delayTime;
    [LabelText("目标")] public Transform transTarget;
    [LabelText("效果类型")] public UIAnimType animType;

    [LabelText("动画片段")] [ShowIf("@animType == UIAnimType.Animation")]
    public AnimationClip anim;

    [LabelText("循环")] public bool isLoop;
    [LabelText("单独播放")] public bool isPlaySingle;

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

    public bool IsPlayOver
    {
        get
        {
            if (_animCtrl == null)
                return false;
            return _animCtrl.IsPlayOver;
        }
    }

    public void PlayAnim(bool isForward)
    {
        transTarget.gameObject.SetActive(true);
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

                _animCtrl.PlayAnim(this, isForward);
                break;
        }
    }

    public void ResetAnim(bool isForward)
    {
        switch (animType)
        {
            case UIAnimType.Animation:

                if (_animCtrl == null)
                    break;

                _animCtrl.ResetAnim(isForward);
                break;
        }
    }

    public void ClearAnim()
    {
        switch (animType)
        {
            case UIAnimType.Animation:
                if (_animCtrl == null)
                    break;

                _animCtrl.ClearAnim();
                break;
        }
    }
}