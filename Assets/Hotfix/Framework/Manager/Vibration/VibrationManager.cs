using UnityEngine;

namespace LccHotfix
{
    internal class VibrationManager : Module, IVibrationService
    {
        private VibrationSaveData save;

        private SimpleVibrationHandler vibrationHandler;

        public bool IsVibrationEnabled
        {
            get => save.IsVibrationEnabled;
            set => save.IsVibrationEnabled = value;
        }

        public VibrationManager()
        {
            IsVibrationEnabled = true;
            vibrationHandler = new SimpleVibrationHandler();
        }

        public void Init()
        {
            save = Main.SaveService.GetGlobalGameSaveFileSave<VibrationSaveData>();
        }

        public void Vibrate(float duration, float intensity = 1.0f)
        {
            if (!IsVibrationEnabled)
                return;

            if (duration <= 0)
                return;

            vibrationHandler.Vibrate(duration, intensity);
        }

        public void LightVibration() => Vibrate(0.08f, 0.4f);
        public void MediumVibration() => Vibrate(0.1f, 0.6f);
        public void StrongVibration() => Vibrate(0.15f, 1f);

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }
    }
}