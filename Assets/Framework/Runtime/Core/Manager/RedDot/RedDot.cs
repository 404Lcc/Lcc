using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class RedDot : MonoBehaviour
    {
        public bool isRedDotActive;
        private GameObject redDot;
        private Text redDotCount;

        public Vector3 scale = Vector3.one;
        public Vector2 offset = Vector2.zero;

        void Start()
        {
            redDot = AssetManager.Instance.InstantiateAsset("RedDot", AssetType.Tool);
            redDotCount = redDot.GetComponentInChildren<Text>();


            redDot.transform.SetParent(transform, false);
            redDot.transform.localScale = scale;
            redDot.transform.GetComponent<RectTransform>().anchoredPosition = offset;

            Hide();

        }
        public void Show()
        {
            isRedDotActive = true;
            redDotCount.text = string.Empty;
            redDot.SetActive(isRedDotActive);
        }
        public void Hide()
        {
            isRedDotActive = false;
            redDotCount.text = string.Empty;
            redDot.SetActive(isRedDotActive);


        }

        public void RefreshRedDotCount(int count)
        {
            if (!isRedDotActive)
            {
                return;
            }
            redDotCount.text = count <= 0 ? string.Empty : count.ToString();
        }


        public void OnDestroy()
        {
            Hide();
            Destroy(redDot);


        }

    }

}