namespace LccHotfix
{
    public class ProtobufObject
    {
        public T Clone<T>() where T : ProtobufObject
        {
            byte[] bytes = ProtobufUtil.Serialize(this);
            return ProtobufUtil.Deserialize<T>(bytes, 0, bytes.Length);
        }
    }
}