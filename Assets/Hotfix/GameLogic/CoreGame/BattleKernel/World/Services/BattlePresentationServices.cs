using UnityEngine;

namespace LccHotfix
{
    public interface IBattleEffectService
    {
        void PlayEffect(string path, Vector3 position, float duration, float scale = 1f);
    }

    public interface IBattleAudioService
    {
        void PlayEntityAudio(LogicEntity entity, string eventName);
    }

    public partial class LogicWorld
    {
        public IBattleEffectService BattleEffectService { get; private set; }
        public IBattleAudioService BattleAudioService { get; private set; }

        public void SetBattleEffectService(IBattleEffectService service)
        {
            BattleEffectService = service;
        }

        public void SetBattleAudioService(IBattleAudioService service)
        {
            BattleAudioService = service;
        }
    }
}
