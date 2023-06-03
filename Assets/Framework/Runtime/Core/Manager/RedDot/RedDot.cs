using NPOI.POIFS.Properties;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace LccModel
{
    public class RedDot : MonoBehaviour
    {
        public bool isRedDotActive;
        private GameObject redDot;
        private Text redDotCount;
        public AssetOperationHandle handle;

        public Vector3 scale = Vector3.one;
        public Vector2 offset = Vector2.zero;

        void Start()
        {
            var asset = AssetManager.Instance.AutoLoadAsset<GameObject>(gameObject.transform, "RedDot", AssetSuffix.Prefab, AssetType.Tool);

            GameObject redDot = Object.Instantiate(asset);
            redDot.name = name;
            redDot.transform.localPosition = Vector3.zero;
            redDot.transform.localRotation = Quaternion.identity;
            redDot.transform.localScale = Vector3.one;

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
            AssetManager.Instance.UnLoadAsset(handle);
            Destroy(redDot);


        }

    }

}