namespace LccHotfix
{
    public interface IVibrationService : IService
    {
        bool IsVibrationEnabled { get; set; }
        void Init();
        void Vibrate(float duration, float intensity = 1.0f);


        void LightVibration();
        void MediumVibration();
        void StrongVibration();
    }
}