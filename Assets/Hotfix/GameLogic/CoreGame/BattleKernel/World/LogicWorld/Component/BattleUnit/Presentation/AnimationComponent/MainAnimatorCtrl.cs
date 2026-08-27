using UnityEngine;

namespace LccHotfix
{
    public class MainAnimatorCtrl : IAnimatorCtrl
    {
        private static readonly int AttackAnimSpeedMult = Animator.StringToHash("AttackAnimSpeedMult");

        public void SyncData(ViewComponent view, ref AnimationData data)
        {
            if (view == null)
                return;

            var animator = FindMainAnimator(view);
            if (animator == null)
            {
                return;
            }

            bool isForceSpecAnim = data.isForceSpecAnim;
            if (!string.IsNullOrEmpty(data.SpecAnim_Layer0))
            {
                PlayAnim(animator, data.SpecAnim_Layer0, isForceSpecAnim, 0);
                data.SpecAnim_Layer0 = "";
            }

            if (!string.IsNullOrEmpty(data.SpecAnim_Layer1))
            {
                animator.SetFloat(AttackAnimSpeedMult,
                    data.SpecAnimSpeed_Layer1 > 0f ? data.SpecAnimSpeed_Layer1 : 1f);
                PlayAnim(animator, data.SpecAnim_Layer1, isForceSpecAnim, 1);
                data.SpecAnim_Layer1 = "";
            }

            if (!string.IsNullOrEmpty(data.SpecAnim_Layer2))
            {
                PlayAnim(animator, data.SpecAnim_Layer2, isForceSpecAnim, 2);
                data.SpecAnim_Layer2 = "";
            }

            data.isForceSpecAnim = false;
        }

        private static Animator FindMainAnimator(ViewComponent view)
        {
            foreach (var wrapper in view.ViewList)
            {
                if (wrapper is IMainAnimatorView mainAnimatorView)
                {
                    var anim = mainAnimatorView.GetMainAnimator();
                    if (anim != null)
                    {
                        return anim;
                    }
                }
            }
            return null;
        }

        public static void PlayAnim(Animator animator, string animName, bool isForce = false, int layer = 0)
        {
            if (animator == null)
            {
                return;
            }

            // AnimatorController 实际层数不足时 GetCurrentAnimatorStateInfo/Play 会刷 Invalid Layer Index
            if (layer < 0 || layer >= animator.layerCount)
            {
                if (KLogger.IsDev)
                    KLogger.LogWarning($"PlayAnim Invalid Layer Index: layer={layer}, animator.layerCount={animator.layerCount}");
                return;
            }

            if (!isForce)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                if (stateInfo.IsName(animName) && stateInfo.normalizedTime <= 1)
                {
                    return;
                }
            }
            else
            {
                animator.StopPlayback();
            }

            if (animator.gameObject.activeInHierarchy)
            {
                animator.Play(animName, layer, 0);
            }
        }

        /// <summary>
        /// 判断指定 Animator 当前 layer 上是否正好播放 animName 且已播放完毕（normalizedTime >= 1）。
        /// 用于业务方在 Update 中轮询动画播放完成的场景。
        /// </summary>
        public static bool IsAnimFinished(Animator animator, string animName, int layer = 0)
        {
            if (animator == null || layer < 0 || layer >= animator.layerCount)
            {
                return false;
            }
            var info = animator.GetCurrentAnimatorStateInfo(layer);
            return info.IsName(animName) && info.normalizedTime >= 1f;
        }
    }
}
