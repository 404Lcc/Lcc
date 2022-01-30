using DG.Tweening;
using UnityEngine;

namespace LccModel
{
    public class LaunchPanel : APanelView<LaunchModel>
    {
        public CanvasGroup BG;
        public override void Start()
        {
            base.Start();
            ShowLaunchAnim();
        }
        public void ShowLaunchAnim()
        {
            BG.DOFade(0, ViewModel.time).onComplete = OnComplete;
        }
        public void OnComplete()
        {
#if UNITY_EDITOR
#if ILRuntime
            ConfigManager.Instance.InitManager();
            ILRuntimeManager.Instance.InitManager();
#else
            ConfigManager.Instance.InitManager();
            MonoManager.Instance.InitManager();
#endif
#else
#if AssetBundle
            UIEventManager.Instance.Publish(UIEventType.Updater);
#else
#if ILRuntime
            ConfigManager.Instance.InitManager();
            ILRuntimeManager.Instance.InitManager();
#else
            ConfigManager.Instance.InitManager();
            MonoManager.Instance.InitManager();
#endif
#endif
#endif
            ClearPanel();
        }
    }
}