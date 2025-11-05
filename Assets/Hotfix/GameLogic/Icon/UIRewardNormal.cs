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

        public UIImageCtrl iconImage;
        public UIImageCtrl frameImage;
        public GameObject newGO;
        public TextMeshProUGUI count;
        public TextMeshProUGUI name;
        public Button previewBtn;

        public override void OnInit()
        {
            base.OnInit();

            SetInfo(data, showName, showCount, showPreview, showQuality);
        }

        public void SetInfo(RewardItemData data, bool showName = true, bool showCount = true, bool showPreview = false)
        {
            if (data == null)
                return;

            this.data = data;
            this.showName = showName;
            this.showCount = showCount;
            this.showPreview = showPreview;
            showQuality = data.GetQuality();

            if (!IsDone)
                return;

            SetIcon();
        }

        public void SetInfo(RewardItemData data, bool showName, bool showCount, bool showPreview, QualityType quality)
        {
            if (data == null)
                return;

            this.data = data;
            this.showName = showName;
            this.showCount = showCount;
            this.showPreview = showPreview;
            this.showQuality = quality;

            if (!IsDone)
                return;

            SetIcon();
        }

        public void SetIcon()
        {
            iconImage.SetImage(data.GetIconId());
            frameImage.SetImage(GetFrame(showQuality));
            count.text = "x" + GameUtility.FormatCurrency(data.Count);
            name.text = data.GetName();
            newGO.SetActive(data.IsNew);
            count.gameObject.SetActive(data.IsShowCount() && showCount && data.Count > 0);
            name.gameObject.SetActive(data.IsShowName() && showName);
            previewBtn.gameObject.SetActive(data.IsShowPreview() && showPreview);
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