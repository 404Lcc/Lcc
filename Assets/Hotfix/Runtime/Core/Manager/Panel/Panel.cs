using UnityEngine;

namespace LccHotfix
{
    public class Panel
    {
        private GameObject _gameObject;
        public PanelType Type
        {
            get; set;
        }
        public PanelState State
        {
            get; set;
        }
        public AObjectBase AObjectBase
        {
            get; set;
        }
        public GameObject gameObject
        {
            get
            {
                if (_gameObject == null)
                {
                    if (AObjectBase != null)
                    {
                        GameObjectEntity gameObjectEntity = AObjectBase.GetParent<GameObjectEntity>();
                        _gameObject = gameObjectEntity?.gameObject;
                    }
                }
                return _gameObject;
            }
        }
        public void OpenPanel()
        {
            if (!(gameObject != null ? gameObject.activeSelf : false))
            {
                State = PanelState.Open;
                gameObject.SetActive(true);
            }
        }
        public void ClosePanel()
        {
            if (!(gameObject != null ? gameObject.activeSelf : false))
            {
                State = PanelState.Close;
                gameObject.SetActive(false);
            }
        }
        public void ClearPanel()
        {
            if (AObjectBase != null)
            {
                AObjectBase.Parent.SafeDestroy();
            }
        }
    }
}