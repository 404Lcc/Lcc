using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class TipsWindow : AObjectBase, IEnumerator
    {
        public int id;
        public bool state;
        public string title;
        public string info;
        public string confirm;
        public string cancel;
        public event Action<bool> Callback;
        public object Current
        {
            get; set;
        }

        public Text titleText;
        public Text infoText;
        public Text confirmText;
        public Text cancelText;
        public void InitTipsWindow(int id, bool state, string title, string info, string confirm = "确定", string cancel = "取消", Transform parent = null)
        {
            this.id = id;
            this.state = state;
            this.title = title;
            this.info = info;
            this.confirm = confirm;
            this.cancel = cancel;

            titleText.text = title;
            infoText.text = info;
            confirmText.text = confirm;
            cancelText.text = cancel;

            if (parent == null)
            {
                transform.SetParent(Objects.Canvas.transform);
            }
            else
            {
                transform.SetParent(parent);
            }
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
        public bool MoveNext()
        {
            return state;
        }
        public void Reset()
        {
        }
        public void OnHideTipsWindow()
        {
            state = false;
            Callback = null;
            TipsWindowManager.Instance.ClearTipsWindow(id);
        }
    }
}