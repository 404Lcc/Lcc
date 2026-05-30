namespace LccHotfix
{
    /// <summary>
    /// 修改速度（按比例修改，乘法叠加）
    /// </summary>
    public class ModifySpeedRatioBhvCfg : ModifyAttributeBhvCfg<float>
    {
        public ModifySpeedRatioBhvCfg(float value, int flag) : base(PropertyFloat.MoveSpeedRatio, value, flag)
        {
        }

        public ModifySpeedRatioBhvCfg(string value, int flag) : base(PropertyFloat.MoveSpeedRatio, value, flag)
        {

        }

        public override System.Type NodeType() { return typeof(ModifySpeedRatioBhv); }
    }

    //////////////////////////////////////////////////////////////////////////
    // 运行时节点 :
    //////////////////////////////////////////////////////////////////////////
    public class ModifySpeedRatioBhv : ModifyAttributeBhv<float>
    {

    }

}
