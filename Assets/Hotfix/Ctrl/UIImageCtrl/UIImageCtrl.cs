using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public enum EmImageType
    {
        None,
        Icon,
    }

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(UISpriteCtrl))]
    public class UIImageCtrl : MonoBehaviour
    {
        public EmImageType imageType = EmImageType.Icon;
        public int imageID = -1;
        private Image _image;
        public Image Image
        {
            get
            {
                if (null == _image)
                    _image = gameObject.GetComponent<Image>();
                if (null == _image)
                    _image = gameObject.AddComponent<Image>();
                return _image;
            }
            set
            {
                _image = value;
            }
        }

        private UISpriteCtrl _spriteCtrl;
        public UISpriteCtrl SpriteCtrl
        {
            get
            {
                if (_spriteCtrl == null)
                {
                    if (Image == null) return null;
                    _spriteCtrl = Image.GetComponent<UISpriteCtrl>();
                }
                return _spriteCtrl;
            }
        }

        public void SetImage(int newImageID)
        {
            string imageName = string.Empty;
            switch (imageType)
            {
                case EmImageType.None:
                    break;
                case EmImageType.Icon:
                    imageName = IconUtility.GetIcon(newImageID);
                    break;
            }
            //这里加一个图集对比，图集不同，强行加载一次
            bool isSameSprite = SpriteCtrl.spriteName.Equals(imageName);
            if (imageID == newImageID && isSameSprite)
                return;
            if (!string.IsNullOrEmpty(imageName))
            {
                imageID = newImageID;
                gameObject.SetActive(true);
                Image.enabled = true;
                SetSprite(imageName);
            }
            else
            {
                HideImage();
            }
        }

        public void HideImage()
        {
            SetSprite("");
            imageID = -1;
        }

        private void SetSprite(string newSpriteName)
        {
            if (!string.IsNullOrEmpty(newSpriteName) && SpriteCtrl != null)
            {
                SpriteCtrl.SetSpriteName(newSpriteName);
            }
        }
    }
}