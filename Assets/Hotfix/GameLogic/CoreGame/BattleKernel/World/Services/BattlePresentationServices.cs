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
}
