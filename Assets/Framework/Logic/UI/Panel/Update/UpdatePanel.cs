using ET;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class UpdatePanel : MonoBehaviour
    {
        public float currentPercent;
        public float targetPercent;
        public float updateRate;

        public static UpdatePanel Instance;

        public Slider progress;
        public Text progressText;

        public void Awake()
        {
            Instance = this;

        }

        public void OnDestroy()
        {

            Instance = null;
        }

        public async ETTask UpdateLoadingPercent(int from, int to, float rate = 1)
        {
            gameObject.SetActive(true);
            updateRate = rate;
            targetPercent = to;
            currentPercent = Mathf.Clamp(currentPercent, from, to);

            progress.value = currentPercent * 0.01f;
            progressText.text = (int)currentPercent + "%";
            while (currentPercent < targetPercent)
            {
                currentPercent += updateRate;
                currentPercent = Mathf.Clamp(currentPercent, 0, 100);

                progress.value = currentPercent * 0.01f;
                progressText.text = (int)currentPercent + "%";
                await Timer.Instance.WaitFrameAsync();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}