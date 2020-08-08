using Hotfix;
using LitJson;
using System.IO;
using System.Text;
using UnityEngine;

public class DataListInstance
{
    public string[] datalist;
    public DataListInstance()
    {
    }
    public DataListInstance(string[] datalist)
    {
        this.datalist = datalist;
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
    public CVType cvtype;
    public LanguageType languagetype;
    public DisplayModeType displaymodetype;
    public ResolutionType resolutiontype;
    public UserSetDataInstance()
    {
    }
    public UserSetDataInstance(int audio, int voice, CVType cvtype, LanguageType languagetype, DisplayModeType displaymodetype, ResolutionType resolutiontype)
    {
        this.audio = audio;
        this.voice = voice;
        this.cvtype = cvtype;
        this.languagetype = languagetype;
        this.displaymodetype = displaymodetype;
        this.resolutiontype = resolutiontype;
    }
}

public class JsonUtil : MonoBehaviour
{
    public static DataListInstance ToDataListInstance()
    {
        return new DataListInstance(DataList.datalist);
    }
    public static void ToDataList(DataListInstance value)
    {
        DataList.datalist = value.datalist;
    }
    public static string SetDataList(string path, string name, object obj)
    {
        string value = GameUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", JsonMapper.ToJson(obj));
        GameUtil.SaveAsset(path, name, Encoding.UTF8.GetBytes(value));
        return value;
    }
    public static DataListInstance GetDataList(string path, string name)
    {
        DataListInstance instance;
        Stream stream;
        FileInfo info = new FileInfo(path + name);
        if (info.Exists)
        {
            stream = info.OpenRead();
        }
        else
        {
            return null;
        }
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        string value = GameUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(bytes));
        stream.Close();
        stream.Dispose();
        instance = JsonMapper.ToObject<DataListInstance>(value);
        return instance;
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
        UserDataInstance instance;
        Stream stream;
        FileInfo info = new FileInfo(path + name);
        if (info.Exists)
        {
            stream = info.OpenRead();
        }
        else
        {
            return null;
        }
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        string value = GameUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(bytes));
        stream.Close();
        stream.Dispose();
        instance = JsonMapper.ToObject<UserDataInstance>(value);
        return instance;
    }

    public static UserSetDataInstance ToUserSetDataInstance()
    {
        return new UserSetDataInstance(UserSetData.audio, UserSetData.voice, UserSetData.cvtype, UserSetData.languagetype, UserSetData.displaymodetype, UserSetData.resolutiontype);
    }
    public static void ToUserSetData(UserSetDataInstance value)
    {
        UserSetData.audio = value.audio;
        UserSetData.voice = value.voice;
        UserSetData.cvtype = value.cvtype;
        UserSetData.languagetype = value.languagetype;
        UserSetData.displaymodetype = value.displaymodetype;
        UserSetData.resolutiontype = value.resolutiontype;
    }
    public static string SetUserSetData(string path, string name, object obj)
    {
        string value = GameUtil.RijndaelEncrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", JsonMapper.ToJson(obj));
        GameUtil.SaveAsset(path, name, Encoding.UTF8.GetBytes(value));
        return value;
    }
    public static UserSetDataInstance GetUserSetData(string path, string name)
    {
        UserSetDataInstance instance;
        Stream stream;
        FileInfo info = new FileInfo(path + name);
        if (info.Exists)
        {
            stream = info.OpenRead();
        }
        else
        {
            return null;
        }
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        string value = GameUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Encoding.UTF8.GetString(bytes));
        stream.Close();
        stream.Dispose();
        instance = JsonMapper.ToObject<UserSetDataInstance>(value);
        return instance;
    }
}