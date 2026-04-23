namespace LccHotfix
{
    // As: EViewCategory：View分类, 由项目定义维度和内容
    public static class EViewCategory
    {
        public const int MainGameObject = 0; // 主场景GameObject对象
        public const int MainUI = 1; // 主UI，Entity的主要表现是UI
        public const int MainFx = 2; // 主特效，主要表现是特效
        public const int Hp = 3; // 血条
        public const int AddedGameObject = 4; // 附加的GameObject
        public const int RecordSubObjrct = 5; // 只为了单独入池
        public const int RadarIcon = 6; // 雷达图标
        public const int DataUI = 7; // 数据层UI
        public const int MainInstance = 8; // Instance View

        // BodyParts
        public const int Part_WeaponR = 11;
        public const int Part_WeaponL = 12;
        public const int Part_Cockpit = 13;


        // 下面属于附属逻辑，生命周期不跟随Main，可以随时增加或者删除
        public const int Range = 100; // 范围
    }
}