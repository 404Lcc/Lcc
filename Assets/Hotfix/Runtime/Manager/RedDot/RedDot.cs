using LccModel;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class RedDot : MonoBehaviour
    {
        public bool isRedDotActive;

        private GameObject _redDot;
        private Text _redDotCount;

        void Start()
        {
            _redDot = AssetManager.Instance.LoadGameObject("RedDot", true);
            _redDot.transform.localPosition = Vector3.zero;
            _redDot.transform.localRotation = Quaternion.identity;
            _redDot.transform.localScale = Vector3.one;

            _redDotCount = _redDot.GetComponentInChildren<Text>();


            _redDot.transform.SetParent(transform, false);
            _redDot.transform.localScale = Vector3.one;
            _redDot.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            Hide();

        }
        public void Show()
        {
            isRedDotActive = true;
            _redDotCount.text = string.Empty;
            _redDot.SetActive(isRedDotActive);
        }
        public void Hide()
        {
            isRedDotActive = false;
            _redDotCount.text = string.Empty;
            _redDot.SetActive(isRedDotActive);
        }

        public void RefreshRedDotCount(int count)
        {
            if (!isRedDotActive)
            {
                return;
            }
            _redDotCount.text = count <= 0 ? string.Empty : count.ToString();
        }
    }
}