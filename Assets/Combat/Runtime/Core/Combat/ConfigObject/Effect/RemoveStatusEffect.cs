using Sirenix.OdinInspector;

namespace LccModel
{
    [Effect("移除状态效果", 40)]
    public class RemoveStatusEffect : Effect
    {
        public override string Label
        {
            get
            {
                if (this.StatusConfigObject != null)
                {
                    return $"移除 [ {this.StatusConfigObject.Name} ] 状态效果";
                }
                return "移除状态效果";
            }
        }

        [ToggleGroup("Enabled")]
        [LabelText("状态配置")]
        public StatusConfigObject StatusConfigObject;
    }
}