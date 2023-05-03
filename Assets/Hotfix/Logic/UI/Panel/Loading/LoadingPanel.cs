using ET;
using LccModel;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class UILoadingModel : ViewModelBase
    {
        public float currentPercent;
        public float targetPercent;
        public float updateRate;
    }
    public class UILoadingPanel : UIPanel<UILoadingModel>
    {
        public static UILoadingPanel Instance { get; set; }

        public Slider progress;
        public Text progressText;

        public override void OnInitComponent(Panel panel)
        {
            base.OnInitComponent(panel);

            Instance = (UILoadingPanel)panel.Logic;
        }

        public override void OnBeforeUnload(Panel panel)
        {
            base.OnBeforeUnload(panel);

            Instance = null;
        }

        public async ETTask UpdateLoadingPercent(int from, int to, float rate = 1)
        {
            ViewModel.updateRate = rate;
            ViewModel.targetPercent = to;
            ViewModel.currentPercent = Mathf.Clamp(ViewModel.currentPercent, from, to);

            progress.value = ViewModel.currentPercent * 0.01f;
            progressText.text = (int)ViewModel.currentPercent + "%";
            while (ViewModel.currentPercent < ViewModel.targetPercent)
            {
                ViewModel.currentPercent += ViewModel.updateRate;
                ViewModel.currentPercent = Mathf.Clamp(ViewModel.currentPercent, 0, 100);

                progress.value = ViewModel.currentPercent * 0.01f;
                progressText.text = (int)ViewModel.currentPercent + "%";
                await Timer.Instance.WaitFrameAsync();
            }
        }
    }
}