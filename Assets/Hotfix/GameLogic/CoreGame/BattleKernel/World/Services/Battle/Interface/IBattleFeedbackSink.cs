using UnityEngine;

namespace LccHotfix
{
    public interface IBattleFeedbackSink
    {
        void ShowDamageMiss(Vector3 position);

        void ShowDamageBlock(Vector3 position);

        void ShowDamageImmune(Vector3 position);

        void ShowDamageNumber(int damage, Vector3 position, bool isCritical, bool isPoisonDot);

        void ShowHealNumber(int healing, Vector3 position);
    }
}
