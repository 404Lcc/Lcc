namespace LccHotfix
{
    public struct SubobjectSource
    {
        public uint SubobjectTid;
        public int SubobjectLogicID;
        
        public SubobjectSource(int subobjectLogicID, uint sbjTid)
        {
            SubobjectLogicID = subobjectLogicID;
            SubobjectTid = sbjTid;
        }
    }
}