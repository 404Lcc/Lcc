using TMPro;
using UnityEngine;

namespace LccHotfix
{
    [AddComponentMenu("UI/Extension/Localized Text")]
    public class LocalizedText : TextMeshProUGUI
    {
        public override string text
        {
            set
            {
                _localizedStrResult = value;
                key = "";
                base.text = value;
            }
        }

        public string LocalizedKey
        {
            get => key;
            set
            {
                if (key.Equals(value))
                    return;

                key = value;
                _localizedStrResult = null;
                _bIsKeyDirty = true;
            }
        }

        [SerializeField] private string key = "";

        private bool _bIsKeyDirty = true;
        private string _localizedStrResult;

        public virtual void ForceUpdateText(bool bRefreshKey = false)
        {
            if (bRefreshKey)
                _localizedStrResult = null;

            _bIsKeyDirty = true;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected virtual void LateUpdate()
        {
            if (!_bIsKeyDirty)
                return;
            var localize = key.Localize();
            var localizedStr = OnTextUpdate(_localizedStrResult ??= string.IsNullOrEmpty(localize) ? key : localize);
            base.text = localizedStr;
            _bIsKeyDirty = false;
        }

        protected virtual string OnTextUpdate(string localizedText) => localizedText;
    }
}
