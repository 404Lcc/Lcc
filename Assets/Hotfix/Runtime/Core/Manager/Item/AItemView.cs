using UnityEngine;

namespace LccHotfix
{
    public abstract class AItemView<T> : AViewBase<T> where T : ViewModelBase
    {
        public GameObject gameObject;
        public override void Start()
        {
            gameObject = GetParent<GameObjectComponent>().gameObject;
            AutoReference(gameObject);
            ShowView(gameObject);
        }
    }
}