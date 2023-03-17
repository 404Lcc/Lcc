namespace LccModel
{
    public enum TagType
    {
        Player,
        Friend,
        Enemy,
    }
    public class TagComponent : Component
    {
        public TagType tagType;
        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            tagType = (TagType)(object)p1;

        }
    }
}