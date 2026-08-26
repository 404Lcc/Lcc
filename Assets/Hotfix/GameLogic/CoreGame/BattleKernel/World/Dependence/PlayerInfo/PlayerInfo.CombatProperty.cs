namespace LccHotfix
{
    public partial class InGamePlayerInfo : ICombatPropertyVolumeInfo
    {
        public void AddSubobjectVolume(uint subobjectTid, ref PropertySnapshot snapshot)
        {
            var volumeSubobjectTid = GetVolume_SubobjectTid((int)subobjectTid);
            if (volumeSubobjectTid != null)
                snapshot.Add(volumeSubobjectTid.Property);
        }
    }
}
