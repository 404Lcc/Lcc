namespace LccHotfix
{
    public class UIBadgeCtrl : UIBadgeBaseCtrl<BadgeComponentHandler>
    {
        private GameObjectPoolAsyncOperation _operation;

        protected override void Awake()
        {
            if (gameObject.transform.childCount > 0)
            {
                _badgeObject = gameObject.transform.GetChild(0).gameObject;
            }
            else
            {
                _operation = Main.GameObjectPoolService.GetObjectAsync("Badge", OnEnd);
            }

            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_operation != null)
            {
                _operation.Release(ref _operation);
            }
        }

        private void OnEnd(GameObjectPoolAsyncOperation obj)
        {
            _badgeObject = obj.GameObject;
            RefreshDisplay();
        }

        public override void RefreshDisplay()
        {
            if (_badgeObject)
            {
                _badgeObject.SetActive(Count > 0);
            }
        }
    }
}