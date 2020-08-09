namespace Hotfix
{
    public class CommandData
    {
        public int id;
        public CommandVarietyType variety;
        public bool bexcute;
        public bool bcondition;
        public bool bfinish;
        public virtual void Execute()
        {
        }
    }
}