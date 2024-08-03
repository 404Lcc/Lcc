using LccModel;

namespace LccModel
{
    public class ProtobufObject
    {
        public virtual void AfterDeserialization()
        {
        }
        public T Clone<T>() where T : ProtobufObject
        {
            byte[] bytes = ProtobufHelper.Serialize(this);
            return (T)ProtobufHelper.Deserialize(typeof(T), bytes, 0, bytes.Length);
        }
    }
}