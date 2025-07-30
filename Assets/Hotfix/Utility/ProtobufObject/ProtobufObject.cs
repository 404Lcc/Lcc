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
            byte[] bytes = ProtobufUtility.Serialize(this);
            return (T)ProtobufUtility.Deserialize(typeof(T), bytes, 0, bytes.Length);
        }
    }
}