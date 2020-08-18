namespace Hotfix
{
    public class CommandData
    {
        public int id;
        public CommandVarietyType variety;
        public bool excute;
        public bool condition;
        public bool finish;
        public virtual void Execute()
        {
        }
    }
}