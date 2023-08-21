using LccModel;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class LoopScrollItem : AObjectBase
    {
        public GroupBase groupBase;
        public int groupIndex;
        public int index = -1;
        public Vector2 sizeDelta;

        private GameObject _gameObject;
        public GameObject gameObject
        {
            get
            {
                return _gameObject;
            }
            set
            {
                if (_gameObject == null)
                {
                    _gameObject = value;
                    _gameObject.ConvertComponent(this);
                    OnInit();
                }
            }
        }

        public ILoopScrollSelect loopScrollSelect => (ILoopScrollSelect)Parent;

        public GameObject selectGo;
        public GameObject normalGo;

        public virtual void OnInit()
        {
            var rect = gameObject.transform as RectTransform;
            groupBase = gameObject.transform.parent.GetComponent<GroupBase>();
            SetSize(rect.sizeDelta(), out groupIndex);

            Button button = gameObject.GetComponent<Button>();
            if (button == null)
            {
                gameObject.AddComponent<Button>().onClick.AddListener(OnClick);
            }
            else
            {
                button.onClick.AddListener(OnClick);
            }
        }

        public virtual void OnShow()
        {

        }

        public virtual void OnHide()
        {

        }

        public virtual void UpdateData(object obj)
        {
            OnItemSelect(loopScrollSelect.CurSelect);
        }
        public virtual void OnClick()
        {
            loopScrollSelect.SetSelect(index);
        }

        public virtual void OnItemSelect(int index)
        {
            UpdateSelectSpriteVisible(this.index == index);
        }

        private void UpdateSelectSpriteVisible(bool visible)
        {
            if (selectGo != null && selectGo.activeSelf != visible)
            {
                selectGo.SetActive(visible);
            }
            if (normalGo != null && normalGo.activeSelf == visible)
            {
                normalGo.SetActive(!visible);
            }
        }

        public void SetSize(Vector2 sizeDelta, out int groupIndex)
        {
            this.sizeDelta = sizeDelta;
            groupBase.SetSize(sizeDelta);
            groupIndex = groupBase.startIndex;
        }
    }
}