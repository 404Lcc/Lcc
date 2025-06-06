namespace LccHotfix
{
    public interface ISkillProcess : IDispose
    {
        public LogicEntity Owner { get; set; }
        public SkillAbility SkillAbility { get; set; }
        public BTAgent Agent { get; set; }

        void Start();
        public void Update();
        public void LateUpdate();
        public bool IsFinished();
    }
}