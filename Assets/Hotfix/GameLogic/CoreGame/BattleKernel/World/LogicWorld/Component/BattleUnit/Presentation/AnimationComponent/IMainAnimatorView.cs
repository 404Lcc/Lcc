using UnityEngine;

namespace LccHotfix
{
    public interface IMainAnimatorView : IViewWrapper
    {
        Animator GetMainAnimator();
    }
}
