namespace LccHotfix
{
    public class UnitLogicGenInfo : ICustomLogicGenInfo
    {
        public UnitSource SumUnitSource;
        public UnitSource? SkillUnitSource;
        public SubobjectSource? SubobjSource;

        public override void Destroy()
        {
            SumUnitSource = default;
            SkillUnitSource = null;
            SubobjSource = null;
            base.Destroy();
        }
    }
}
