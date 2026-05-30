namespace LccHotfix
{
    public struct SubobjectSource
    {
        public uint SubobjectTid;
        
        public SubobjectSource(IBattlePlayerInfo info, uint sbjTid)
        {
            SubobjectTid = sbjTid;
        }
    }
}
