namespace LccHotfix
{
    public class UserSetData
    {
        public int musicVolume = 100;
        public int soundFXVolume = 100;
        public DisplayModeType displayModeType = DisplayModeType.FullScreen;
        public ResolutionType resolutionType = ResolutionType.Resolution1920x1080;
        public UserSetData()
        {
        }
        public UserSetData(int musicVolume, int soundFXVolume, DisplayModeType displayModeType, ResolutionType resolutionType)
        {
            this.musicVolume = musicVolume;
            this.soundFXVolume = soundFXVolume;
            this.displayModeType = displayModeType;
            this.resolutionType = resolutionType;
        }
    }
}