using System.Collections.Generic;

namespace LccHotfix
{
    public class HeroInfo
    {
        public int HeroTid; 
        public int FighterTid; //战斗实体模板（项目特化）
        public int UintTid; //战斗单位属性模板（项目特化）
        public int HeroLevel = 1;
    }

    public class GameHeroInfo
    {
        public List<HeroInfo> HeroInfos { get; set; } = new();
    }
}
