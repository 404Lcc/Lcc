namespace LccHotfix
{
    //属性、特性集合（按不确定的各种业务维度分组）
    public class CategoryVolumeInfo  
    {
        public PropertySnapshot Property = new ();   
    }   
    
    public class CategoryVolumeInfo_Subobject : CategoryVolumeInfo
    {
        //分类专属特殊的部分，或者和属性无关的部分，在这里扩展：
        public string AssetOverride_Main { get; set; } = null; //子弹皮肤 主体
    }
    
    
    public class CategoryVolumeInfo_Fighter : CategoryVolumeInfo
    {
        public int SkillLogicOverride_Main { get; set; } = -1;     //主技能篡改
    }
    
    public enum ECategoryVolume
    {
        FighterTid,     //按局内战斗实体表TID分类，常用于某个战斗实体模板的属性/主技能改写
        BattleUnit,     //按战斗单位属性表TID分类，作用于某类单位属性模板
        SubobjectTid,   //按局内子物体TID分类，常用于子弹/子物体属性与表现覆盖
        Camp,           //按阵营分类，如地球联邦、烈鹰军团、佣兵基地
        Element,        //按元素分类，如弹药、物理、能量、电、力场、暗
        AttackType,     //按攻击类型分类，如弹药系，能量系，机关系
        UnitType,       //按战斗单位类型分类，如敌人、防御者、城墙、召唤物、防御塔、宠物
        LevelType,      //按关卡类型分类，如主线、困难、挑战
        Skill,          //按技能TID分类，作用于具体技能模板
        Feature,        //预留的特性分类，当前未见实际接入
        EnemyStrength,  //按敌人强度分类，如普通、精英、Boss
        ElementType,    //按照伤害类型分类
    }
}
