using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class UILoadingPanel : MonoBehaviour
    {
        public static UILoadingPanel Instance;

        public float currentPercent;
        public float targetPercent;
        public float updateRate;

        public RawImage backTex;

        public Slider progress;
        public TextMeshProUGUI progressText;

        public TextMeshProUGUI tipsText;

        public TextMeshProUGUI versionText;
        public GameObject messageBox;

        public PatchWindow patchWindow;

        public void Awake()
        {
            Instance = this;
            patchWindow = new PatchWindow();
            patchWindow.Init(progress, tipsText, messageBox);
        }

        public void OnDestroy()
        {
            Instance = null;
            patchWindow.Dispose();
            patchWindow = null;
        }

        void Update()
        {
            if (currentPercent < targetPercent)
            {
                currentPercent += updateRate;
                currentPercent = Mathf.Clamp(currentPercent, 0, 100);
                progress.value = currentPercent * 0.01f;
                progressText.text = (int)currentPercent + "%";
            }
        }

        #region 启动时loading背景

        public void SetStartLoadingBg()
        {
            //backTex.texture = Resources.Load<Texture2D>("");
        }

        #endregion

        public void UpdateLoadingPercent(int from, int to, float rate = 1)
        {
            gameObject.SetActive(true);


            updateRate = rate;
            targetPercent = to;
            currentPercent = Mathf.Clamp(currentPercent, from, to);

            progress.value = currentPercent * 0.01f;
            progressText.text = (int)currentPercent + "%";
        }

        public void Show(string text)
        {
            if (!string.IsNullOrEmpty(text))
                this.tipsText.text = text;
            else
                this.tipsText.text = "";
            currentPercent = targetPercent = 0;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            progressText.text = "";
            tipsText.text = "";
            gameObject.SetActive(false);
        }

        public void SetVersion(string version)
        {
            this.versionText.text = version;
        }

        public void ShowMessageBox(string content, Action confirm, bool needHide = true)
        {
            patchWindow.ShowMessageBox(content, confirm, needHide);
        }
    }
}