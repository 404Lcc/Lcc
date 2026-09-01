namespace LccHotfix
{
    public delegate bool NodeParamEntityCmdAction(CustomNode node, EntityCommand cmd);
    
    public static partial class EntityCmdType
    {
        public const int Op_None = 0;

        // 内核机制响应
        public const int Nt_ColliderHit = 101;
        public const int Nt_Death = 103;
        public const int Nt_OnHurt = 104;
        public const int Nt_Kill = 105;
        public const int Nt_Move = 107;
        public const int Nt_Skill = 108;
        public const int Nt_OnHit = 109; // 受击，不一定受伤
        public const int Nt_OnLocomotionEnd = 110;
    }

    //值类型Cmd，自定义扩展用ParamEx
    public struct EntityCommand
    {
        public int CmdType { get; set; }
        public long EntityID { get; set; }
        public HitInfo HitInfo;
        //命令数据:
        public AnyValve V0;
        public AnyValve V1;
        public AnyValve V2;
    }
}
