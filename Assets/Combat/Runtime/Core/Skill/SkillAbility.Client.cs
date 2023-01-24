namespace LccModel
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SkillAbility
    {
        public SkillExecutionData SkillExecutionData { get; set; }
        public ExecutionObject ExecutionObject { get; set; }



        public void LoadExecution()
        {
            ExecutionObject = null;
            //ExecutionObject = AssetUtils.Load<ExecutionObject>($"Execution_{SkillConfig.Id}");
            if (ExecutionObject == null)
            {
                return;
            }
        }
    }
}
