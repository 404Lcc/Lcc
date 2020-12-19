using LccModel;

namespace LccHotfix
{
    public class UserSetData
    {
        public int audio = 20;
        public int voice = 100;
        public CVType cvType = CVType.Chinese;
        public LanguageType languageType = LanguageType.Chinese;
        public DisplayModeType displayModeType = DisplayModeType.FullScreen;
        public ResolutionType resolutionType = ResolutionType.Resolution1920x1080;
        public UserSetData()
        {
        }
        public UserSetData(int audio, int voice, CVType cvType, LanguageType languageType, DisplayModeType displayModeType, ResolutionType resolutionType)
        {
            this.audio = audio;
            this.voice = voice;
            this.cvType = cvType;
            this.languageType = languageType;
            this.displayModeType = displayModeType;
            this.resolutionType = resolutionType;
        }
    }
}