namespace LccHotfix
{
    public partial class AudioManager
    {
        private AudioMetaConfig _audioMeta;

        public AudioMetaConfig Meta => _audioMeta;

        private void InitMeta()
        {
            _audioMeta = AudioRawFile<AudioMetaConfig>.Load(AudioMetaConfig.Path);
        }

        private void DisposeMeta()
        {
            _audioMeta = null;
        }
    }
}