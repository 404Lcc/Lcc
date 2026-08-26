using System.Collections.Generic;

namespace LccHotfix
{
    public class HeroInfo
    {
        public int HeroTid;
        public int FighterTid = 1993;
        public int UintTid = 16;
        public int HeroLevel = 1;

        public float FixedAtk = -1f;
        public double FixedHp = -1;
        public float AtkInterval = -1f;
        public float ReloadTime = -1f;
        public float MoveSpeed = -1f;
        public int BulletCount = -1;
    }

    public class GameHeroInfo
    {
        public List<HeroInfo> HeroInfos { get; set; } = new();
    }
}
