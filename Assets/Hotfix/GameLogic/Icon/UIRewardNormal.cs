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
        public RewardItemData data;

        public UIImageCtrl iconImage;
        public UIImageCtrl frameImage;
        public GameObject newGO;
        public TextMeshProUGUI count;
        public TextMeshProUGUI name;
        public Button previewBtn;

        protected override void UpdateData(object info)
        {
            base.UpdateData(info);

            data = (RewardItemData)info;

            iconImage.SetImage(data.GetIconId());
            frameImage.SetImage(GetFrame(data.GetQuality()));
            count.text = "x" + GameUtility.FormatCurrency(data.Count);
            name.text = data.GetName();
            newGO.SetActive(data.IsNew);
            count.gameObject.SetActive(data.IsShowCount() && data.Count > 0);
            name.gameObject.SetActive(data.IsShowName());
            previewBtn.gameObject.SetActive(data.IsShowPreview());
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