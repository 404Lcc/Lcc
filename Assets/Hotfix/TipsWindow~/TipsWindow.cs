﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class TipsWindow : AObjectBase
    {
        public GameObject gameObject => GetParent<GameObjectEntity>().gameObject;

        public string title;
        public string info;
        public string confirm;
        public string cancel;
        public Action<bool> completed;

        public Text titleText;
        public Text infoText;
        public Text confirmText;
        public Text cancelText;

        public Button confirmBtn;
        public Button cancelBtn;
        public Image ad;
        public override void Start()
        {
            AutoReference(gameObject);
            ShowView(gameObject);

            confirmBtn.onClick.AddListener(OnConfirm);
            cancelBtn.onClick.AddListener(OnCancel);
        }
        public void InitTipsWindow(string title, string info, Action<bool> completed, string confirm, string cancel, Transform parent = null)
        {
            this.title = title;
            this.info = info;
            this.completed = completed;
            this.confirm = confirm;
            this.cancel = cancel;

            titleText.text = title;
            infoText.text = info;
            confirmText.text = confirm;
            cancelText.text = cancel;

            if (parent == null)
            {
                gameObject.transform.SetParent(GlobalManager.Instance.PopupRoot);
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
            completed?.Invoke(true);
            OnHideTipsWindow();
        }
        public void OnCancel()
        {
            completed?.Invoke(false);
            OnHideTipsWindow();
        }
        public void OnHideTipsWindow()
        {
            completed = null;
            TipsWindowManager.Instance.ClearTipsWindow(Parent.id);
        }
    }
}