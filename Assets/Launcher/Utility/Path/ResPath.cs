using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LccModel
{
    public class ResPath
    {
        private static string _pathStreamingWeb;
        private static string _pathPersistentWeb;
        public static string _pathPersistent;

        private static StringBuilder _build = new StringBuilder();
        private static string _platStr;
        public static string PlatformDirectory => RuntimePlatformDirectory();

        public static void InitPath()
        {
            _platStr = RuntimePlatformDirectory();

#if UNITY_EDITOR
            string outPath = Application.dataPath.Replace("Assets", "OutPackage");
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            _pathPersistent = outPath + Path.DirectorySeparatorChar;
#else
            _pathPersistent = Application.persistentDataPath + Path.DirectorySeparatorChar;
#endif
            Debug.Log("Persistent 地址：" + _pathPersistent);

#if UNITY_IOS
            _pathStreamingWeb = @"file://" + Application.streamingAssetsPath;
            _pathPersistentWeb = @"file://" + _pathPersistent;
#elif UNITY_ANDROID
		    _pathStreamingWeb = Application.streamingAssetsPath;
            _pathPersistentWeb = "file://" + _pathPersistent;
#else
            _pathStreamingWeb = "file://" + Application.streamingAssetsPath;
            _pathPersistentWeb = _pathPersistent;
#endif
            Debug.Log("Streaming web 地址：" + _pathStreamingWeb);
            Debug.Log("Persistent web 地址：" + _pathPersistentWeb);
        }

        /// <summary>
        /// www webRequest类型加载的Streaming路径
        /// </summary>
        public static string ResStreamingPathWeb
        {
            get
            {
                return _pathStreamingWeb;
            }
        }
        public static string ResPersistentPathWeb
        {
            get
            {
                return _pathPersistentWeb;
            }
        }

        /// <summary>
        /// 加载assetbundle时候的Streaming路径
        /// </summary>
        public static string ResStreamingPathAB
        {
            get
            {
                return Application.streamingAssetsPath;
            }
        }

        public static string StreamingPath(string name)
        {
            lock (_build)
            {
                _build.Clear();
                _build.Append(ResStreamingPathAB);
                _build.Append(Path.DirectorySeparatorChar);
                _build.Append(PlatformDirectory);
                _build.Append(Path.DirectorySeparatorChar);
                _build.Append(name.ToLower());
                return _build.ToString();
            }
        }

        public static string StreamingPathWeb(string name)
        {
            lock (_build)
            {
                _build.Clear();
                _build.Append(ResStreamingPathWeb);
                _build.Append(Path.DirectorySeparatorChar);
                _build.Append(PlatformDirectory);
                _build.Append(Path.DirectorySeparatorChar);
                _build.Append(name.ToLower());
                return _build.ToString();
            }
        }
        public static string PersistentPath(string name)
        {
            lock (_build)
            {
                _build.Clear();
                _build.Append(_pathPersistent);
                _build.Append(PlatformDirectory);
                _build.Append(Path.DirectorySeparatorChar);
                _build.Append(name.ToLower());
                return _build.ToString();
            }
        }
        public static string PersistentPathWeb(string name)
        {
            lock (_build)
            {
                _build.Clear();
                _build.Append(ResPersistentPathWeb);
                _build.Append(PlatformDirectory);
                _build.Append(Path.DirectorySeparatorChar);
                _build.Append(name.ToLower());
                return _build.ToString();
            }
        }

        public static string PersistentDirectory()
        {
            lock (_build)
            {
                _build.Clear();
                _build.Append(_pathPersistent);
                _build.Append(PlatformDirectory);
                return _build.ToString();
            }
        }

        public static string RuntimePlatformDirectory()
        {
            if (!string.IsNullOrEmpty(_platStr))
                return _platStr;
#if UNITY_EDITOR
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            switch (target)
            {
                case BuildTarget.Android:
                    return "android";
                case BuildTarget.iOS:
                    return "ios";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "windows";
                default:
                    return string.Empty;
            }
#elif UNITY_ANDROID
            return "android";
#elif UNITY_IOS
            return "ios";
#else
            return "windows";
#endif
        }
    }
}