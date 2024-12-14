using TMPro;
using UnityEngine;

namespace LccHotfix
{
    public class RedDot : MonoBehaviour
    {
        public bool isRedDotActive;
        private GameObjectPoolObject _redDotGameObject;
        private TextMeshProUGUI _redDotCountText;

        public Vector3 scale = Vector3.one;
        public Vector2 offset = Vector2.zero;

        private void Awake()
        {
            this.isRedDotActive = false;
        }

        public void Show(GameObjectPoolObject redDotGameObject)
        {
            this.isRedDotActive = true;
            this._redDotGameObject = redDotGameObject;
            redDotGameObject.GameObject.transform.SetParent(this.transform, false);
            redDotGameObject.GameObject.transform.localScale = scale;
            redDotGameObject.GameObject.transform.GetComponent<RectTransform>().anchoredPosition = this.offset;
            this._redDotCountText = redDotGameObject.GameObject.GetComponentInChildren<TextMeshProUGUI>();
            redDotGameObject.GameObject.SetActive(true);
        }

        public void RefreshRedDotCount(int count)
        {
            if (null == this._redDotGameObject)
            {
                return;
            }
            this._redDotGameObject.GameObject.transform.localScale = scale;
            this._redDotCountText.text = count <= 0 ? string.Empty : count.ToString();
        }

        public GameObjectPoolObject Recovery()
        {
            if (this._redDotCountText != null)
            {
                this._redDotCountText.text = "";
            }

            this.isRedDotActive = false;
            this._redDotCountText = null;
            this._redDotGameObject?.GameObject?.SetActive(false);
            GameObjectPoolObject go = this._redDotGameObject;
            this._redDotGameObject = null;
            return go;
        }
    }
}