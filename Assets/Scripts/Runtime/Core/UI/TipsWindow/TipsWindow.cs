using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Model
{
    public class TipsWindow : ObjectBase, IEnumerator
    {
        public int id;
        public bool state;
        public string title;
        public string info;
        public string confirm;
        public string cancel;
        public Action<bool> complete;
        public object Current => null;

        public Text titleText;
        public Text infoText;
        public Text confirmText;
        public Text cancelText;
        public void InitTipsWindow(string title, string info, string confirm = "确定", string cancel = "取消", Transform parent = null)
        {
            state = true;

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
                transform.SetParent(Objects.gui.transform);
            }
            else
            {
                transform.SetParent(parent);
            }
            RectTransform rect = GameUtil.GetComponent<RectTransform>(gameObject);
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
            complete?.Invoke(true);
            OnHideTipsWindow();
        }
        public void OnCancel()
        {
            complete?.Invoke(false);
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
            complete = null;
            TipsWindowManager.Instance.ClearTipsWindow(id);
        }
    }
}