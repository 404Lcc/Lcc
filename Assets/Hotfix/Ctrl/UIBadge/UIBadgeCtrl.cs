using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class UIBadgeCtrl : UIBadgeBaseCtrl<BadgeComponentHandler>
    {
        protected override void Awake()
        {
            if (gameObject.transform.childCount > 0)
            {
                _badgeObject = gameObject.transform.GetChild(0).gameObject;
            }
            else
            {
                Main.AssetService.LoadAssetAsync<GameObject>("BadgeGo", OnEnd);
            }

            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void OnEnd(AssetHandle obj)
        {
            var go = obj.AssetObject as GameObject;
            _badgeObject = GameObject.Instantiate(go);
            ClientTools.ResetTransform(_badgeObject.transform, transform);
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