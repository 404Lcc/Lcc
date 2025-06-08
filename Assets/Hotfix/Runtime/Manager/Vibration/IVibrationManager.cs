namespace LccHotfix
{
    public interface IVibrationManager
    {
        void Vibrate(float duration, float intensity = 1.0f);

        bool IsVibrationEnabled { get; set; }

        void LightVibration();
        void MediumVibration();
        void StrongVibration();
    }
}