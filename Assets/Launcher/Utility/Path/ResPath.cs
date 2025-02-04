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
        private static string pathStreamingWeb;
        private static string pathPersistentWeb;
        public static string pathPersistent;

        private static StringBuilder build = new StringBuilder();
        private static string platStr;
        public static string platformDirectory
        {
            get { return RuntimePlatformDirectory(); }
        }

        public static void InitPath()
        {
            platStr = RuntimePlatformDirectory();

#if UNITY_EDITOR
            string outPath = Application.dataPath.Replace("Assets", "OutPackage");
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            pathPersistent = outPath + Path.DirectorySeparatorChar;
#else
            pathPersistent = Application.persistentDataPath + Path.DirectorySeparatorChar;
#endif
            Debug.Log("persistent 地址：" + pathPersistent);

#if UNITY_IOS
            pathStreamingWeb = @"file://" + Application.streamingAssetsPath;
            pathPersistentWeb = @"file://" + pathPersistent;
#elif UNITY_ANDROID
		    pathStreamingWeb = Application.streamingAssetsPath;
            pathPersistentWeb = "file://" + pathPersistent;
#else
            pathStreamingWeb = "file://" + Application.streamingAssetsPath;
            pathPersistentWeb = pathPersistent;
#endif
            Debug.Log("streaming web 地址：" + pathStreamingWeb);
            Debug.Log("Persistent web 地址：" + pathPersistentWeb);
        }

        /// <summary>
        /// www webRequest类型加载的Streaming路径
        /// </summary>
        public static string ResStreamingPathWeb
        {
            get
            {
                return pathStreamingWeb;
            }
        }
        public static string ResPersistentPathWeb
        {
            get
            {
                return pathPersistentWeb;
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

        public static string StreamingPath(string bundleName)
        {
            lock (build)
            {
                build.Clear();
                build.Append(ResStreamingPathAB);
                build.Append(Path.DirectorySeparatorChar);
                build.Append(platformDirectory);
                build.Append(Path.DirectorySeparatorChar);
                build.Append(bundleName.ToLower());
                return build.ToString();
            }
        }

        public static string StreamingPathWeb(string bundleName)
        {
            lock (build)
            {
                build.Clear();
                build.Append(ResStreamingPathWeb);
                build.Append(Path.DirectorySeparatorChar);
                build.Append(platformDirectory);
                build.Append(Path.DirectorySeparatorChar);
                build.Append(bundleName.ToLower());
                return build.ToString();
            }
        }
        public static string PersistentPath(string bundleName)
        {
            lock (build)
            {
                build.Clear();
                build.Append(pathPersistent);
                build.Append(platformDirectory);
                build.Append(Path.DirectorySeparatorChar);
                build.Append(bundleName.ToLower());
                return build.ToString();
            }
        }
        public static string PersistentPathWeb(string bundleName)
        {
            lock (build)
            {
                build.Clear();
                build.Append(ResPersistentPathWeb);
                build.Append(platformDirectory);
                build.Append(Path.DirectorySeparatorChar);
                build.Append(bundleName.ToLower());
                return build.ToString();
            }
        }

        public static string PersistentDirectory()
        {
            lock (build)
            {
                build.Clear();
                build.Append(pathPersistent);
                build.Append(platformDirectory);
                return build.ToString();
            }
        }

        public static string RuntimePlatformDirectory()
        {
            if (!string.IsNullOrEmpty(platStr)) return platStr;
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