namespace LccHotfix
{
    public class SettingManager : Module, ISettingService
    {
        public SettingSaveData save;

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
        }

        public void Init()
        {
            save = Main.SaveService.GetGlobalGameSaveFileSave<SettingSaveData>();
        }
    }
}