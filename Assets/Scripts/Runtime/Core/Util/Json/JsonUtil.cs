using LitJson;
using System;
using System.Reflection;

namespace LccModel
{
    public static class JsonUtil
    {
        public static string ToJson(object obj)
        {
            return JsonMapper.ToJson(obj);
        }
        public static object ToObject(Type type, string json)
        {
            JsonReader jsonReader = new JsonReader(json);
            return typeof(JsonMapper).InvokeMember("ReadValue", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { type, jsonReader });
        }
        public static T ToObject<T>(string json)
        {
            return JsonMapper.ToObject<T>(json);
        }
    }
}