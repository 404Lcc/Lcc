namespace LccHotfix
{
    public struct DamageSource
    {
        public int skillId;
        public int subobjectId;
        public int buffId;

        public static DamageSource Skill(int skillId)
        {
            DamageSource source = new DamageSource();
            source.skillId = skillId;
            source.subobjectId = -1;
            source.buffId = -1;
            return source;
        }

        public static DamageSource Subobject(int skillId, int subobjectId)
        {
            DamageSource source = new DamageSource();
            source.skillId = skillId;
            source.subobjectId = subobjectId;
            source.buffId = -1;
            return source;
        }

        public static DamageSource Buff(int buffId)
        {
            DamageSource source = new DamageSource();
            source.skillId = -1;
            source.subobjectId = -1;
            source.buffId = buffId;
            return source;
        }
    }
}