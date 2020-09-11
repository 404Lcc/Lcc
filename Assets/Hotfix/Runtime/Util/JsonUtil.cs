using LitJson;
using System.Collections.Generic;
using System.Text;

namespace Hotfix
{
    public class DataListInstance
    {
        public List<string> dataList;
        public DataListInstance()
        {
        }
        public DataListInstance(List<string> dataList)
        {
            this.dataList = dataList;
        }
    }

    public class UserDataInstance
    {
        public UserDataInstance()
        {
        }
    }

    public class UserSetDataInstance
    {
        public int audio;
        public int voice;
        public CVType cvType;
        public LanguageType languageType;
        public DisplayModeType displayModeType;
        public ResolutionType resolutionType;
        public UserSetDataInstance()
        {
        }
        public UserSetDataInstance(int audio, int voice, CVType cvType, LanguageType languageType, DisplayModeType displayModeType, ResolutionType resolutionType)
        {
            this.audio = audio;
            this.voice = voice;
            this.cvType = cvType;
            this.languageType = languageType;
            this.displayModeType = displayModeType;
            this.resolutionType = resolutionType;
        }
    }

    public static class JsonUtil
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
            string value = GameUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", JsonMapper.ToJson(obj));
            GameUtil.SaveAsset(path, name, Encoding.UTF8.GetBytes(value));
            return value;
        }
        public static DataListInstance GetDataList(string path, string name)
        {
            string value = GameUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(GameUtil.GetAsset(path, name)));
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
            string value = GameUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", JsonMapper.ToJson(obj));
            GameUtil.SaveAsset(path, name, Encoding.UTF8.GetBytes(value));
            return value;
        }
        public static UserDataInstance GetUserData(string path, string name)
        {
            string value = GameUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(GameUtil.GetAsset(path, name)));
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
            string value = GameUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", JsonMapper.ToJson(obj));
            GameUtil.SaveAsset(path, name, Encoding.UTF8.GetBytes(value));
            return value;
        }
        public static UserSetDataInstance GetUserSetData(string path, string name)
        {
            string value = GameUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(GameUtil.GetAsset(path, name)));
            return JsonMapper.ToObject<UserSetDataInstance>(value);
        }
    }
}