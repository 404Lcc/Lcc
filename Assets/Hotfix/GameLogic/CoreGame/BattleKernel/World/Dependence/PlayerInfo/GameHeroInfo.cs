using System.Collections.Generic;

namespace LccHotfix
{
    public class HeroInfo
    {
        public int HeroId; // 英雄卡牌表ID
        public int FighterId = 1993; //局内战斗实体表ID
        public int BattleUnitId = 16; //战斗单位属性表ID
        public int GameWeaponId = 2771; //局内武器表ID
        public int HeroLevel = 1;
        public int WeaponLevel = 1;

        ///
        /// <summary>进局覆盖攻击；表示不覆盖，走配置 Base。
        /// </summary>
        public float FixedAtk = -1f;

        /// <summary>
        /// 进局覆盖血量；表示不覆盖，走配置 Base。
        /// </summary>
        public double FixedHp = -1;
    }

    public class GameHeroInfo
    {
        public List<HeroInfo> HeroInfos { get; set; } = new();
    }
}