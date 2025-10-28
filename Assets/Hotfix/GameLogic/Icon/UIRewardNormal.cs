using cfg;
using LccModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LccHotfix
{
    /// <summary>
    /// 通用奖励item
    /// </summary>
    public class UIRewardNormal : IconBase
    {
        public bool showName;
        public bool showCount;
        public bool showPreview;
        public QualityType showQuality;

        public RewardItemData data;

        public UIImageCtrl frameImage;
        public GameObject newGO;
        public TextMeshProUGUI count;
        public TextMeshProUGUI name;
        public Button previewBtn;

        public void SetInfo(RewardItemData data, bool showName = true, bool showCount = true, bool showPreview = false)
        {
            this.data = data;
            this.showName = showName;
            this.showCount = showCount;
            this.showPreview = showPreview;
            showQuality = data.GetQuality();
            SetIcon(data.GetIconId());
        }

        public void SetInfo(RewardItemData data, bool showName, bool showCount, bool showPreview, QualityType quality)
        {
            this.data = data;
            this.showName = showName;
            this.showCount = showCount;
            this.showPreview = showPreview;
            this.showQuality = quality;
            SetIcon(data.GetIconId());
        }

        public override void SetIcon(int newImageID)
        {
            frameImage.SetImage(GetFrame(showQuality));
            count.text = "x" + GameUtility.FormatCurrency(data.Count);
            name.text = data.GetName();
            newGO.SetActive(data.IsNew);
            count.gameObject.SetActive(data.IsShowCount() && showCount && data.Count > 0);
            name.gameObject.SetActive(data.IsShowName() && showName);
            previewBtn.gameObject.SetActive(data.IsShowPreview() && showPreview);

            base.SetIcon(newImageID);
        }

        protected override void OnReset()
        {
            base.OnReset();
        }

        protected override void OnShowClickTips()
        {
            base.OnShowClickTips();

            if (data == null)
                return;
        }

        public int GetFrame(QualityType itemQuality)
        {
            switch (itemQuality)
            {
                case QualityType.White:
                    return 0;
                case QualityType.Green:
                    return 0;
                case QualityType.Blue:
                    return 0;
                case QualityType.Purple:
                    return 0;
                case QualityType.Yellow:
                    return 0;
                case QualityType.Red:
                    return 0;
                case QualityType.Platinum:
                    return 0;
            }

            return 0;
        }
    }
}