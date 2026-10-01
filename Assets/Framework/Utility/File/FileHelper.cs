using System.IO;
using UnityEngine;

namespace LccModel
{
    public static class FileHelper
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        private static readonly string RootPath = Path.Combine(Application.dataPath, "..");
#else
        private static readonly string RootPath = Application.persistentDataPath;
#endif

        public static string GetSavedPath()
        {
            return Path.Combine(RootPath, "Saved");
        }

        public static void CreateDirectoryByPath(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static string LoadString(string file)
        {
            var path = ResolvePath(file);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        public static void SaveString(string file, string str)
        {
            var path = ResolvePath(file);
            CreateDirectoryByPath(path);
            File.WriteAllText(path, str);
        }

        private static string ResolvePath(string file)
        {
            if (Path.IsPathRooted(file))
                return file;
            return Path.Combine(RootPath, file);
        }
    }
}
