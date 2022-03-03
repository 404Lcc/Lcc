using System;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class TipsWindow : AObjectBase
    {
        public GameObject gameObject;

        public string title;
        public string info;
        public string confirm;
        public string cancel;
        public event Action<bool> Callback;

        public Text titleText;
        public Text infoText;
        public Text confirmText;
        public Text cancelText;

        public Button confirmBtn;
        public Button cancelBtn;
        public Image ad;
        public override void Start()
        {
            gameObject = GetParent<GameObjectEntity>().gameObject;
            AutoReference(gameObject);
            ShowView(gameObject);

            confirmBtn.onClick.AddListener(OnConfirm);
            cancelBtn.onClick.AddListener(OnCancel);
        }
        public void InitTipsWindow(string title, string info, Action<bool> callback, string confirm, string cancel, Transform parent = null)
        {
            this.title = title;
            this.info = info;
            this.Callback = callback;
            this.confirm = confirm;
            this.cancel = cancel;

            titleText.text = title;
            infoText.text = info;
            confirmText.text = confirm;
            cancelText.text = cancel;

            if (parent == null)
            {
                gameObject.transform.SetParent(Objects.Canvas.transform);
            }
            else
            {
                gameObject.transform.SetParent(parent);
            }
            gameObject.transform.SetAsLastSibling();
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
        public void OnConfirm()
        {
            Callback?.Invoke(true);
            OnHideTipsWindow();
        }
        public void OnCancel()
        {
            Callback?.Invoke(false);
            OnHideTipsWindow();
        }
        public void OnHideTipsWindow()
        {
            Callback = null;
            TipsWindowManager.Instance.ClearTipsWindow(Parent.id);
        }
    }
}