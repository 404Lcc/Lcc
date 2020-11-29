using LccModel;

namespace LccHotfix
{
    public class UserSetDataInstance
    {
        public int audio;
        public int voice;
        public CVType cvType;
        public LanguageType languageType;
        public DisplayModeType displayModeType;
        public ResolutionType resolutionType;
        public UserSetDataInstance()
        {
        }
        public UserSetDataInstance(int audio, int voice, CVType cvType, LanguageType languageType, DisplayModeType displayModeType, ResolutionType resolutionType)
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