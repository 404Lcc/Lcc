using UnityEngine;

namespace LccHotfix
{
    public class VibrationSaveData : ISave
    {
        public bool IsVibrationEnabled { get; set; }

        public void Init()
        {
            IsVibrationEnabled = true;
        }
    }

    public class VibrationData : ISaveConverter<VibrationSaveData>
    {
        public VibrationSaveData Save { get; set; }
        public bool IsVibrationEnabled { get; set; }

        public void Flush()
        {
            Save.IsVibrationEnabled = IsVibrationEnabled;
        }

        public void Init()
        {
            this.IsVibrationEnabled = Save.IsVibrationEnabled;
        }
    }
}