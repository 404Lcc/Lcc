using LccModel;

namespace LccHotfix
{
    public class ProtobufObject
    {
        public virtual void AfterDeserialization()
        {
        }
        public T Clone<T>() where T : ProtobufObject
        {
            byte[] bytes = ProtobufUtil.Serialize(this);
            return (T)ProtobufUtil.Deserialize(typeof(T), bytes, 0, bytes.Length);
        }
    }
}