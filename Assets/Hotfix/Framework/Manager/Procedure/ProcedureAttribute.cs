namespace LccHotfix
{
    public class ProcedureAttribute : AttributeBase
    {
        public ProcedureType type;
        public ProcedureAttribute(ProcedureType type)
        {
            this.type = type;
        }
    }
}