
//////////////////////////////////////////////////////////////////////////
// Buff面向特殊游戏的 外部信息接口扩展
//////////////////////////////////////////////////////////////////////////
namespace LccHotfix
{
    public interface IEntityAddBuffNotify
    {
        void OnEntityAddBuff(LogicEntity e);
    }

    public interface IBuffPreviousRemove
    {
        void OnBuffPreviousRemove();
    }

    public interface IBuffStateChanged
    {
        void OnBuffStateChanged(LogicEntity e, BuffLogic buff);
    }

    public interface IBuffHandleBeforeDmg
    {
        void HandleBeforeDmg(DamageContext context, ref DamageResult result);
    }
}
