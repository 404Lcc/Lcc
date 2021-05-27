namespace LccHotfix
{
    public class ProtobufObject
    {
        public ProtobufObject Clone()
        {
            byte[] bytes = ProtobufUtil.Serialize(this);
            return ProtobufUtil.Deserialize<ProtobufObject>(bytes, 0, bytes.Length);
        }
    }
}