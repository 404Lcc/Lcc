using Model;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TipsWindow : MonoBehaviour, IEnumerator
{
    public int id;
    public bool state;
    public string title;
    public string information;
    public string confirm;
    public string cancel;
    public Action<bool> complete;
    public object Current
    {
        get
        {
            return null;
        }
    }

    public Text titletext;
    public Text informationtext;
    public Text confirmtext;
    public Text canceltext;
    public Button confirmbtn;
    public Button cancelbtn;
    void Awake()
    {
        titletext = GameUtil.GetChildComponent<Text>(gameObject, "Title");
        informationtext = GameUtil.GetChildComponent<Text>(gameObject, "Information");
        confirmtext = GameUtil.GetChildComponent<Text>(gameObject, "ConfirmBtn", "Text");
        canceltext = GameUtil.GetChildComponent<Text>(gameObject, "CancelBtn", "Text");
        confirmbtn = GameUtil.GetChildComponent<Button>(gameObject, "ConfirmBtn");
        cancelbtn = GameUtil.GetChildComponent<Button>(gameObject, "CancelBtn");
        confirmbtn.onClick.AddListener(OnConfirm);
        cancelbtn.onClick.AddListener(OnCancel);
    }
    public void InitTipsWindow(string title, string information, string confirm = "确定", string cancel = "取消", Transform parent = null)
    {
        state = true;

        this.title = title;
        this.information = information;
        this.confirm = confirm;
        this.cancel = cancel;

        titletext.text = title;
        informationtext.text = information;
        confirmtext.text = confirm;
        canceltext.text = cancel;

        if (parent == null)
        {
            transform.SetParent(IO.gui.transform);
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
        IO.tipswindowManager.DeleteTipsWindow(id);
    }
}