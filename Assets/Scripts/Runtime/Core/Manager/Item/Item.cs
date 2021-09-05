using UnityEngine;

namespace LccModel
{
    public class Item : IItem
    {
        private GameObject _gameObject;
        public ItemType Type
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
                        GameObjectComponent gameObjectComponent = AObjectBase.GetComponent<GameObjectComponent>();
                        _gameObject = gameObjectComponent?.gameObject;
                    }
                }
                return _gameObject;
            }
        }
    }
}