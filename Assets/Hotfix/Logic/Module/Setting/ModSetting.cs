namespace LccHotfix
{
    public class SettingSaveData : ISave
    {
        public int MusicVolume { get; set; }
        public int SoundFXVolume { get; set; }
        public DisplayModeType DisplayModeType { get; set; }
        public ResolutionType ResolutionType { get; set; }

        public void Init()
        {
            MusicVolume = 100;
            SoundFXVolume = 100;
            DisplayModeType = DisplayModeType.FullScreen;
            ResolutionType = ResolutionType.Resolution1920x1080;
        }
    }

    public class SettingData : ISaveConverter<SettingSaveData>
    {
        public SettingSaveData Save { get; set; }
        public int MusicVolume { get; private set; }
        public int SoundFXVolume { get; private set; }
        public DisplayModeType DisplayModeType { get; private set; }
        public ResolutionType ResolutionType { get; private set; }

        public void Flush()
        {
            Save.MusicVolume = MusicVolume;
            Save.SoundFXVolume = SoundFXVolume;
            Save.DisplayModeType = DisplayModeType;
            Save.ResolutionType = ResolutionType;
        }

        public void Init()
        {
            this.MusicVolume = Save.MusicVolume;
            this.SoundFXVolume = Save.SoundFXVolume;
            this.DisplayModeType = Save.DisplayModeType;
            this.ResolutionType = Save.ResolutionType;
        }
    }

    [Model]
    public class ModSetting : ModelTemplate, ISavePipeline
    {
        public SettingData SettingData { get; private set; }

        public void InitData(GameSaveData gameSaveData)
        {
            SettingData = gameSaveData.GetSaveData<SettingData, SettingSaveData>();
        }
    }
}