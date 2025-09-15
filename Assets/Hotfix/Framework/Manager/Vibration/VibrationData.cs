using UnityEngine;

namespace LccHotfix
{
    public class VibrationSaveData : ISave
    {
        public string TypeName => GetType().FullName;
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

        public ISave Flush()
        {
            Save.IsVibrationEnabled = IsVibrationEnabled;
            return Save;
        }

        public void Init()
        {
            this.IsVibrationEnabled = Save.IsVibrationEnabled;
        }
    }
}