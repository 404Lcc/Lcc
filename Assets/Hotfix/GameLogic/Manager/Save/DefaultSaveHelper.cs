using LccHotfix;

public class DefaultSaveHelper : ISaveHelper
{
    private ES3Settings _settings;
    private readonly string SavePath = "../SaveDatas/";
    private readonly string FileName = "saveData.lcc";

    public DefaultSaveHelper()
    {
        _settings = new ES3Settings();
        SetEncryption(true);
        SetStorePath(StoreMode.Official);
    }

    public void SetEncryption(bool isAES)
    {
        if (isAES)
        {
            _settings.encryptionType = ES3.EncryptionType.AES;
            _settings.encryptionPassword = "xxxxxxxxxxxxxxxx";
        }
        else
        {
            _settings.encryptionType = ES3.EncryptionType.None;
        }
    }

    public void SetStorePath(StoreMode mode)
    {
        if (mode == StoreMode.Beta)
        {
            _settings.directory = ES3.Directory.DataPath;
            _settings.path = SavePath + FileName;
        }
        else if (mode == StoreMode.Official)
        {
            _settings.directory = ES3.Directory.PersistentDataPath;
            _settings.path = SavePath + FileName;
        }
    }

    public void Save<T>(string key, T value)
    {
        ES3.Save<T>(key, value, _settings);
    }

    public T Load<T>(string key)
    {
        return ES3.Load<T>(key, _settings);
    }

    public void DeleteKey(string key)
    {
        ES3.DeleteKey(key, _settings);
    }

    public bool KeyExists(string key)
    {
        return ES3.KeyExists(key, _settings);
    }

    public T Load<T>(string key, T defaultValue)
    {
        return ES3.Load<T>(key, defaultValue, _settings);
    }

    //加载到
    public void LoadInto<T>(string key, T obj) where T : class
    {
        ES3.LoadInto<T>(key, obj, _settings);
    }


    public void DeleteFile()
    {
        ES3.DeleteFile(_settings);
    }

    public void DeleteDirectory()
    {
        ES3.DeleteDirectory(_settings);
    }


    public bool FileExists()
    {
        return ES3.FileExists(_settings);
    }

    public bool DirectoryExists()
    {
        return ES3.DirectoryExists(_settings);
    }

    public string[] GetKeys()
    {
        return ES3.GetKeys(_settings);
    }

    public string[] GetFiles()
    {
        return ES3.GetFiles(_settings);
    }

    public string[] GetDirectories()
    {
        return ES3.GetDirectories(_settings);
    }
}