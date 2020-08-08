//using DG.Tweening;
using Model;
using UnityEngine;
using UnityEngine.UI;

public class Tips : MonoBehaviour
{
    public int id;
    public string information;

    public Text informationtext;
    public void InitTips(string information, Vector2 position, Vector2 offset, float duration, Transform parent = null)
    {
        this.information = information;

        informationtext.text = information;

        if (parent == null)
        {
            transform.SetParent(IO.gui.transform);
        }
        else
        {
            transform.SetParent(parent);
        }
        RectTransform rect = GameUtil.GetComponent<RectTransform>(gameObject);
        rect.localPosition = position;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        informationtext.CrossFadeAlpha(0, duration, true);
        Vector3 target = new Vector3(position.x + offset.x, position.y + offset.y, 0);
        //DOTween.To(() => rect.localPosition, x => rect.localPosition = x, target, duration).SetEase(Ease.OutCubic).OnComplete(() =>
        //{
        //    IO.tipsManager.DeleteTips(id);
        //});
    }
    void Update()
    {
        transform.SetAsLastSibling();
    }
}