namespace LccHotfix
{
    public class SettingSaveData : SaveData
    {
        public int MusicVolume { get; set; }
        public int SoundFXVolume { get; set; }
        public DisplayModeType DisplayModeType { get; set; }
        public ResolutionType ResolutionType { get; set; }

        public override void CreateNewSaveData()
        {
            MusicVolume = 100;
            SoundFXVolume = 100;
            DisplayModeType = DisplayModeType.FullScreen;
            ResolutionType = ResolutionType.Resolution1920x1080;
        }
    }

    public class SettingData : ISaveDataConverter<SettingSaveData>
    {
        public int MusicVolume { get; private set; }
        public int SoundFXVolume { get; private set; }
        public DisplayModeType DisplayModeType { get; private set; }
        public ResolutionType ResolutionType { get; private set; }

        public SettingSaveData ToSaveData()
        {
            var save = new SettingSaveData();
            save.MusicVolume = MusicVolume;
            save.SoundFXVolume = SoundFXVolume;
            save.DisplayModeType = DisplayModeType;
            save.ResolutionType = ResolutionType;
            return save;
        }

        public void FromSaveData(SettingSaveData data)
        {
            this.MusicVolume = data.MusicVolume;
            this.SoundFXVolume = data.SoundFXVolume;
            this.DisplayModeType = data.DisplayModeType;
            this.ResolutionType = data.ResolutionType;
        }
    }

    [Model]
    public class ModSetting : ModelTemplate, ISavePipeline
    {
        public SettingData SettingData { get; private set; }

        public void InitData(GameSaveData gameSaveData)
        {
            var saveData = gameSaveData.GetModule<SettingSaveData>();
            SettingData = new SettingData();
            SettingData.FromSaveData(saveData);
        }

        public void SaveData(GameSaveData gameSaveData)
        {
            var module = SettingData.ToSaveData();
            gameSaveData.SetModule(module);
        }
    }
}