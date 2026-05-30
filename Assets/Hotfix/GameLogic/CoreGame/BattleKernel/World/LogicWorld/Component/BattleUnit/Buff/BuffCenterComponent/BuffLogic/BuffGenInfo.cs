namespace LccHotfix
{
    public delegate BuffGenInfo AdjustBuffGenInfo(BuffGenInfo genInfo);

    public class BuffGenInfo : UnitLogicGenInfo
    {
        public int BuffLogicID;
        public int BuffLevel;
        public int BuffMaxLevel;
        public float DurationAddRate; // 额外的持续时间加成比例，用于buff免疫逻辑
        public LogicEntity Sourcer;
        public LogicEntity Owner;

        public void AdjustBuffGenInfo_FromUnitEntity(LogicEntity fromEntity)
        {
            PreEnv.FillBuffSourceVarEnv(fromEntity);
        }

        public void Clear()
        {
            BuffLogicID = -1;
            BuffLevel = -1;
            BuffMaxLevel = -1;
            DurationAddRate = 0;
            Sourcer = null;
            Owner = null;
        }
    }
}
