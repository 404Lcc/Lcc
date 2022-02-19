namespace LccModel
{
    public class ProtobufObject
    {
        public virtual void AfterDeserialization()
        {
        }
        public T Clone<T>() where T : ProtobufObject
        {
            byte[] bytes = ProtobufUtil.Serialize(this);
            return ProtobufUtil.Deserialize<T>(bytes, 0, bytes.Length);
        }
    }
}