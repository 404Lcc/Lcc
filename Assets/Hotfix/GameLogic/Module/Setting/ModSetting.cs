namespace LccHotfix
{
    public class SettingSaveData : ISave
    {
        public string TypeName => GetType().FullName;
        public DisplayModeType DisplayModeType { get; set; }
        public ResolutionType ResolutionType { get; set; }

        public void Init()
        {
            DisplayModeType = DisplayModeType.FullScreen;
            ResolutionType = ResolutionType.Resolution1920x1080;
        }
    }

    public class SettingData : ISaveConverter<SettingSaveData>
    {
        public SettingSaveData Save { get; set; }
        public DisplayModeType DisplayModeType { get; private set; }
        public ResolutionType ResolutionType { get; private set; }

        public ISave Flush()
        {
            Save.DisplayModeType = DisplayModeType;
            Save.ResolutionType = ResolutionType;
            return Save;
        }

        public void Init()
        {
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
            SettingData = gameSaveData.GetSaveConverterData<SettingData, SettingSaveData>();
        }
    }
}