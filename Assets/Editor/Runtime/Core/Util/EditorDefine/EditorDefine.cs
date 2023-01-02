using LccModel;
using UnityEditor;

public static class EditorDefine
{
    private static GlobalConfig _globalConfig;
    private static GlobalConfig GlobalConfig
    {
        get
        {
            if (_globalConfig == null)
            {
                _globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Resources/GlobalConfig.asset");
            }
            return _globalConfig;
        }
    }
    public static HotfixMode HotfixMode
    {
        get
        {
            return GlobalConfig.hotfixMode;
        }
        set
        {
            GlobalConfig.hotfixMode = value;
        }
    }
    public static bool IsRelease
    {
        get
        {
            return GlobalConfig.isRelease;
        }
        set
        {
            GlobalConfig.isRelease = value;
        }
    }
}