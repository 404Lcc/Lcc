namespace LccHotfix
{
    public struct HealContext
    {
        public LogicWorld World;
        public UnitSource Healer; // 发起治疗的人
        public UnitSource Target; // 被治疗的人
        public float Healing; // 治疗量，目前没有其他修饰治疗的逻辑，先用这个
    }
}
