namespace LccModel
{
    public partial class SkillAbility
    {
        public ExecutionObject ExecutionObject { get; set; }

        public void LoadExecution()
        {
            ExecutionObject = AssetManager.Instance.LoadAsset<ExecutionObject>(out var handler, $"Execution_{SkillConfig.Id}", AssetSuffix.Asset, AssetType.Execution);
            if (ExecutionObject == null)
            {
                return;
            }
        }
    }
}