namespace Hotfix
{
    public class CommandData
    {
        public int id;
        public CommandDataType dataType;
        public bool isExcute;
        public bool isCondition;
        public bool isFinish;
        public virtual void Execute()
        {
        }
    }
}