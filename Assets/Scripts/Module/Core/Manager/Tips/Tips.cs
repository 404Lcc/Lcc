//using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Model
{
    public class Tips : ObjectBase
    {
        public int id;
        public string info;

        public Text infoText;
        public override void Update()
        {
            transform.SetAsLastSibling();
        }
        public void InitTips(string info, Vector2 localPosition, Vector2 offset, float duration, Transform parent = null)
        {
            this.info = info;

            infoText.text = info;

            if (parent == null)
            {
                transform.SetParent(Objects.gui.transform);
            }
            else
            {
                transform.SetParent(parent);
            }
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localPosition = localPosition;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            infoText.CrossFadeAlpha(0, duration, true);
            Vector3 target = new Vector3(localPosition.x + offset.x, localPosition.y + offset.y, 0);
            //DOTween.To(() => rect.localPosition, x => rect.localPosition = x, target, duration).SetEase(Ease.OutCubic).OnComplete(() =>
            //{
            //    TipsManager.Instance.ClearTips(id);
            //});
        }
    }
}