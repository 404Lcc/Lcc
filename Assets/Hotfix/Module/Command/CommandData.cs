namespace Hotfix
{
    public class CommandData
    {
        public int id;
        public CommandVarietyType variety;
        public bool isExcute;
        public bool isCondition;
        public bool isFinish;
        public virtual void Execute()
        {
        }
    }
}