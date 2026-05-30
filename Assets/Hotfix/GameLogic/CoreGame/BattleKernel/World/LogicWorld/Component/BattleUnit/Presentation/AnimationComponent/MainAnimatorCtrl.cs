using UnityEngine;

namespace LccHotfix
{
    public class MainAnimatorCtrl : IAnimatorCtrl
    {
        public void SyncData(ViewComponent view, ref AnimationData data)
        {
            if (view == null)
                return;

            var theView = view.GetView<IMainAnimatorView>(EViewCategory.MainGameObject);
            if (theView == null)
            {
                return;
            }

            var animator = theView.GetMainAnimator();
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
                PlayAnim(animator, data.SpecAnim_Layer1, isForceSpecAnim, 1);
                data.SpecAnim_Layer1 = "";
            }

            data.isForceSpecAnim = false;
        }

        public void PlayAnim(Animator animator, string animName, bool isForce = false, int layer = 0)
        {
            if (!isForce)
            {
                var normalizedTime = animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
                if (animator.GetCurrentAnimatorStateInfo(layer).IsName(animName) && normalizedTime <= 1)
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
    }
}
