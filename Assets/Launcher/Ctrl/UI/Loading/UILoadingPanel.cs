using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class MessageBox
    {
        public GameObject cloneObject;
        public TextMeshProUGUI infoText;
        public Button confirmBtn;
        public Action completed;
        public bool confirmHide;

        public bool ActiveSelf
        {
            get
            {
                return cloneObject.activeSelf;
            }
        }

        public void Create(GameObject cloneObject)
        {
            this.cloneObject = cloneObject;
            infoText = cloneObject.transform.Find("BG/InfoText").GetComponent<TextMeshProUGUI>();
            confirmBtn = cloneObject.transform.Find("BG/ConfirmBtn").GetComponent<Button>();
            confirmBtn.onClick.AddListener(OnConfirm);
        }
        public void Show(string info, Action completed, bool confirmHide)
        {
            infoText.text = info;
            this.completed = completed;
            this.confirmHide = confirmHide;
            cloneObject.SetActive(true);
            cloneObject.transform.SetAsLastSibling();
        }
        public void Hide()
        {
            infoText.text = string.Empty;
            completed = null;
            cloneObject.SetActive(false);
        }
        private void OnConfirm()
        {
            completed?.Invoke();
            if (confirmHide)
            {
                Hide();
            }
        }
    }
    public class UILoadingPanel : MonoBehaviour
    {
        public float currentPercent;
        public float targetPercent;
        public float updateRate;

        public static UILoadingPanel Instance;

        public RawImage backTex;

        public Slider progress;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI tipsText;

        public TextMeshProUGUI text;

        public GameObject messageBox;

        private readonly EventGroup _eventGroup = new EventGroup();

        public List<MessageBox> msgBoxList = new List<MessageBox>();


        public void Awake()
        {
            Instance = this;

            _eventGroup.AddListener<InitializeFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchStatesChange>(OnHandleEventMessage);
            _eventGroup.AddListener<FoundUpdateFiles>(OnHandleEventMessage);
            _eventGroup.AddListener<DownloadProgressUpdate>(OnHandleEventMessage);
            _eventGroup.AddListener<PackageVersionUpdateFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<PatchManifestUpdateFailed>(OnHandleEventMessage);
            _eventGroup.AddListener<WebFileDownloadFailed>(OnHandleEventMessage);

        }
        public void OnDestroy()
        {

            Instance = null;

            _eventGroup.RemoveAllListener();
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
        public void SetText(string text)
        {
            this.text.text = text;
        }
        public void ClearText()
        {
            progressText.text = "";
            tipsText.text = "";
        }

        public void Hide()
        {
            ClearText();
            gameObject.SetActive(false);
        }

        public void ShowMessageBox(string info, Action completed, bool confirmHide = true)
        {
            MessageBox msgBox = null;
            for (int i = 0; i < msgBoxList.Count; i++)
            {
                var item = msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = GameObject.Instantiate(messageBox, messageBox.transform.parent);
                msgBox.Create(cloneObject);
                msgBoxList.Add(msgBox);
            }
            msgBox.Show(info, completed, confirmHide);
        }

        public void OnHandleEventMessage(IEventMessage message)
        {
            if (message is InitializeFailed)
            {
                System.Action callback = () =>
                {
                    UserTryInitialize.SendEventMessage();
                };
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_init_failed"), callback);
            }
            else if (message is PatchStatesChange)
            {
                var msg = message as PatchStatesChange;
                tipsText.text = msg.Tips;
            }
            else if (message is FoundUpdateFiles)
            {
                var msg = message as FoundUpdateFiles;
                System.Action callback = () =>
                {
                    UserBeginDownloadWebFiles.SendEventMessage();
                };
                float sizeMB = msg.TotalSizeBytes / 1048576f;
                sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
                string totalSizeMB = sizeMB.ToString("f1");
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_found_update", msg.TotalCount.ToString(), totalSizeMB), callback);
            }
            else if (message is DownloadProgressUpdate)
            {
                var msg = message as DownloadProgressUpdate;
                progress.value = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
                progressText.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount}";
                string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
                string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
                tipsText.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
            }
            else if (message is PackageVersionUpdateFailed)
            {
                System.Action callback = () =>
                {
                    UserTryUpdatePackageVersion.SendEventMessage();
                };
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_update_failed_static"), callback);
            }
            else if (message is PatchManifestUpdateFailed)
            {
                System.Action callback = () =>
                {
                    UserTryUpdatePatchManifest.SendEventMessage();
                };
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_update_failed_patch"), callback);
            }
            else if (message is WebFileDownloadFailed)
            {
                var msg = message as WebFileDownloadFailed;
                System.Action callback = () =>
                {
                    UserTryDownloadWebFiles.SendEventMessage();
                };
                ShowMessageBox(Launcher.Instance.GetLanguage("msg_download_failed", msg.FileName), callback);
            }
            else
            {
                throw new System.NotImplementedException($"{message.GetType()}");
            }
        }
    }
}