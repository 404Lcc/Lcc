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
}