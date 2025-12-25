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
        [Tooltip("适应图片大小")] public bool perfect;

        private Image _image;
        private UISpriteCtrl _spriteCtrl;

        public Image Image
        {
            get
            {
                if (_image == null)
                {
                    _image = gameObject.GetComponent<Image>();
                }

                if (_image == null)
                {
                    _image = gameObject.AddComponent<Image>();
                }

                return _image;
            }
            set { _image = value; }
        }

        public UISpriteCtrl SpriteCtrl
        {
            get
            {
                if (_spriteCtrl == null)
                {
                    _spriteCtrl = Image.GetComponent<UISpriteCtrl>();
                }

                if (_spriteCtrl == null)
                {
                    _spriteCtrl = gameObject.AddComponent<UISpriteCtrl>();
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

            SetImage(imageName);
        }

        public void SetImage(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                gameObject.SetActive(true);
                Image.enabled = true;
                SetSprite(imageName);
            }
            else
            {
                ClearImage();
            }
        }

        private void ClearImage()
        {
            Image.sprite = null;
        }

        private void SetSprite(string imageName)
        {
            SpriteCtrl.GetSprite(imageName, (x) =>
            {
                Image.sprite = x;
                if (perfect)
                {
                    Image.SetNativeSize();
                }
            });
        }
    }
}