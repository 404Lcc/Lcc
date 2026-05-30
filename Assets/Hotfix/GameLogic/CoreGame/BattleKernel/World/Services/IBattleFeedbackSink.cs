using UnityEngine;

namespace LccHotfix
{
    public interface IBattleFeedbackSink
    {
        void ShowDamageMiss(Vector3 position, bool usePrimaryStateFeedbackStyle);

        void ShowDamageBlock(Vector3 position, bool usePrimaryStateFeedbackStyle);

        void ShowDamageNumber(int damage, Vector3 position, bool useTaggedDefenderStyle, bool isCritical);

        void ShowHealNumber(int healing, Vector3 position, bool useTaggedTargetStyle);
    }

    public partial class LogicWorld
    {
        public IBattleFeedbackSink BattleFeedbackSink { get; private set; }

        public void SetBattleFeedbackSink(IBattleFeedbackSink feedbackSink)
        {
            BattleFeedbackSink = feedbackSink;
        }
    }
}
