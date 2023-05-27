using ET;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class MessageBox
    {
        public GameObject cloneObject;
        public Text infoText;
        public Button confirmBtn;
        public Action completed;

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
            infoText = cloneObject.transform.Find("BG/InfoText").GetComponent<Text>();
            confirmBtn = cloneObject.transform.Find("BG/ConfirmBtn").GetComponent<Button>();
            confirmBtn.onClick.AddListener(OnConfirm);
        }
        public void Show(string info, Action completed)
        {
            infoText.text = info;
            this.completed = completed;
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
            Hide();
        }
    }
    public class UpdatePanel : MonoBehaviour, IEventListener
    {
        public float currentPercent;
        public float targetPercent;
        public float updateRate;

        public static UpdatePanel Instance;

        public Slider progress;
        public Text progressText;
        public Text tipsText;
        public Text downloadText;
        public GameObject messageBox;

        public List<MessageBox> msgBoxList = new List<MessageBox>();


        public void Awake()
        {
            Instance = this;

        }
        public void OnDisable()
        {
            Event.Instance.RemoveListener(EventType.InitializeFailed, this);
            Event.Instance.RemoveListener(EventType.PatchStatesChange, this);
            Event.Instance.RemoveListener(EventType.FoundUpdateFiles, this);
            Event.Instance.RemoveListener(EventType.DownloadProgressUpdate, this);
            Event.Instance.RemoveListener(EventType.PackageVersionUpdateFailed, this);
            Event.Instance.RemoveListener(EventType.PatchManifestUpdateFailed, this);
            Event.Instance.RemoveListener(EventType.WebFileDownloadFailed, this);
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

        private void ShowMessageBox(string info, Action completed)
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
            msgBox.Show(info, completed);
        }

        public void HandleEvent(EventType eventType, IEventArgs args1 = null, IEventArgs args2 = null, IEventArgs args3 = null, IEventArgs args4 = null)
        {
            switch (eventType)
            {
                case EventType.InitializeFailed:
                    void InitializeFailed()
                    {
                        Action callback = () =>
                        {
                            UserEventDefine.UserTryInitialize.Publish();
                        };
                        ShowMessageBox($"Failed to initialize package !", callback);
                    }
                    InitializeFailed();

                    break;
                case EventType.PatchStatesChange:
                    void PatchStatesChange()
                    {
                        var patchStatesChange = args1.GetValue<UpdateEventDefine.PatchStatesChange>();
                        tipsText.text = patchStatesChange.Tips;
                    }
                    PatchStatesChange();
                    break;
                case EventType.FoundUpdateFiles:
                    void FoundUpdateFiles()
                    {
                        var foundUpdateFiles = args1.GetValue<UpdateEventDefine.FoundUpdateFiles>();
                        Action callback = () =>
                        {
                            UserEventDefine.UserBeginDownloadWebFiles.Publish();
                        };
                        float sizeMB = foundUpdateFiles.TotalSizeBytes / 1048576f;
                        sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
                        string totalSizeMB = sizeMB.ToString("f1");
                        ShowMessageBox($"Found update patch files, Total count {foundUpdateFiles.TotalCount} Total szie {totalSizeMB}MB", callback);
                    }
                    FoundUpdateFiles();

                    break;
                case EventType.DownloadProgressUpdate:
                    void DownloadProgressUpdate()
                    {
                        var downloadProgressUpdate = args1.GetValue<UpdateEventDefine.DownloadProgressUpdate>();
                        progress.value = (float)downloadProgressUpdate.CurrentDownloadCount / downloadProgressUpdate.TotalDownloadCount;
                        progressText.text = $"{downloadProgressUpdate.CurrentDownloadCount}/{downloadProgressUpdate.TotalDownloadCount}";

                        string currentSizeMB = (downloadProgressUpdate.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
                        string totalSizeMB = (downloadProgressUpdate.TotalDownloadSizeBytes / 1048576f).ToString("f1");
                        downloadText.text = $"{downloadProgressUpdate.CurrentDownloadCount}/{downloadProgressUpdate.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
                    }
                    DownloadProgressUpdate();
                    break;
                case EventType.PackageVersionUpdateFailed:
                    void PackageVersionUpdateFailed()
                    {
                        Action callback = () =>
                        {
                            UserEventDefine.UserTryUpdatePackageVersion.Publish();
                        };
                        ShowMessageBox($"Failed to update static version, please check the network status.", callback);
                    }
                    PackageVersionUpdateFailed();

                    break;
                case EventType.PatchManifestUpdateFailed:
                    void PatchManifestUpdateFailed()
                    {
                        Action callback = () =>
                        {
                            UserEventDefine.UserTryUpdatePatchManifest.Publish();
                        };
                        ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
                    }
                    PatchManifestUpdateFailed();
                    break;
                case EventType.WebFileDownloadFailed:
                    void WebFileDownloadFailed()
                    {
                        var webFileDownloadFailed = args1.GetValue<UpdateEventDefine.WebFileDownloadFailed>();
                        Action callback = () =>
                        {
                            UserEventDefine.UserTryDownloadWebFiles.Publish();
                        };
                        ShowMessageBox($"Failed to download file : {webFileDownloadFailed.FileName}", callback);
                    }
                    WebFileDownloadFailed();

                    break;
            }
        }
    }
}