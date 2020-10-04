using LitJson;
using System.Text;

namespace Hotfix
{
    public static class UserUtil
    {
        public static DataListInstance ToDataListInstance()
        {
            return new DataListInstance(DataList.dataList);
        }
        public static void ToDataList(DataListInstance value)
        {
            DataList.dataList = value.dataList;
        }
        public static string SetDataList(string path, string name, object obj)
        {
            string value = Util.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", JsonMapper.ToJson(obj));
            Util.SaveAsset(path, name, Encoding.UTF8.GetBytes(value));
            return value;
        }
        public static DataListInstance GetDataList(string path, string name)
        {
            string value = Util.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(Util.GetAsset(path, name)));
            return JsonMapper.ToObject<DataListInstance>(value);
        }

        public static UserDataInstance ToUserDataInstance()
        {
            return new UserDataInstance();
        }
        public static void ToUserData(UserDataInstance value)
        {
        }
        public static string SetUserData(string path, string name, object obj)
        {
            string value = Util.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", JsonMapper.ToJson(obj));
            Util.SaveAsset(path, name, Encoding.UTF8.GetBytes(value));
            return value;
        }
        public static UserDataInstance GetUserData(string path, string name)
        {
            string value = Util.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(Util.GetAsset(path, name)));
            return JsonMapper.ToObject<UserDataInstance>(value);
        }

        public static UserSetDataInstance ToUserSetDataInstance()
        {
            return new UserSetDataInstance(UserSetData.audio, UserSetData.voice, UserSetData.cvType, UserSetData.languageType, UserSetData.displayModeType, UserSetData.resolutionType);
        }
        public static void ToUserSetData(UserSetDataInstance value)
        {
            UserSetData.audio = value.audio;
            UserSetData.voice = value.voice;
            UserSetData.cvType = value.cvType;
            UserSetData.languageType = value.languageType;
            UserSetData.displayModeType = value.displayModeType;
            UserSetData.resolutionType = value.resolutionType;
        }
        public static string SetUserSetData(string path, string name, object obj)
        {
            string value = Util.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", JsonMapper.ToJson(obj));
            Util.SaveAsset(path, name, Encoding.UTF8.GetBytes(value));
            return value;
        }
        public static UserSetDataInstance GetUserSetData(string path, string name)
        {
            string value = Util.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(Util.GetAsset(path, name)));
            return JsonMapper.ToObject<UserSetDataInstance>(value);
        }
    }
}