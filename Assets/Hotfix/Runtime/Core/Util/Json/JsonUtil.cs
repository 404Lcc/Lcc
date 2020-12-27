using LitJson;

namespace LccHotfix
{
    public static class JsonUtil
    {
        public static string ToJson(object obj)
        {
            return JsonMapper.ToJson(obj);
        }
        public static T ToObject<T>(string json)
        {
            return JsonMapper.ToObject<T>(json);
        }
    }
}