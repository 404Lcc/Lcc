using LccModel;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    public class LoopScrollItem : IReference
    {
        public int index = -1;

        public GroupBase groupBase;
        public int GroupIndex => groupBase.groupIndex;
        public int GroupStart => groupBase.groupStart;

        public GameObject gameObject;

        public ILoopScroll loopScroll;

        public GameObject selectGo;
        public GameObject normalGo;


        public void Init(ILoopScroll loopScroll, GameObject gameObject)
        {
            this.loopScroll = loopScroll;
            this.gameObject = gameObject;

            ClientTools.AutoReference(gameObject, this);

            ClientTools.ForceGetComponent<Button>(this.gameObject).onClick.AddListener(OnItemClick);

            OnInit();

        }

        public virtual void OnInit()
        {
        }

        public virtual void OnShow()
        {
        }

        public virtual void UpdateData(object obj)
        {
            OnItemSelect(loopScroll.CurSelect);
        }

        public virtual void OnItemClick()
        {
            loopScroll.OnItemSelect(index);
            loopScroll.OnItemClick(index);
        }

        public virtual void OnItemSelect(int index)
        {
            UpdateSelectSpriteVisible(this.index == index);
        }

        public virtual void UpdateSelectSpriteVisible(bool visible)
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

        public void SetSize(Vector2 size)
        {
            loopScroll.SetSize(index, size);
        }

        public void SetSizeX(int x)
        {
            loopScroll.SetSizeX(index, x);
        }

        public void SetSizeY(int y)
        {
            loopScroll.SetSizeY(index, y);
        }

        public virtual void OnRecycle()
        {
        }
    }
}