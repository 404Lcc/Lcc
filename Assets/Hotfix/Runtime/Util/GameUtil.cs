using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Hotfix
{
    public class GameUtil : MonoBehaviour
    {
        public static string GetDataPath
        {
            get
            {
                string path = Application.dataPath + "/";
                return path;
            }
        }
        public static string GetStreamingAssetsPath
        {
            get
            {
                string path = Application.streamingAssetsPath + "/";
                return path;
            }
        }
        public static string GetPersistentDataPath
        {
            get
            {
                string path = Application.persistentDataPath + "/";
                return path;
            }
        }
        /// <summary>
        /// 获取路径
        /// </summary>
        /// <param name="type"></param>
        /// <param name="folders"></param>
        /// <returns></returns>
        public static string GetPath(PathType type, params string[] folders)
        {
            string path = string.Empty;
            string subPath = string.Empty;
            switch (type)
            {
                case PathType.DataPath:
                    path = GetDataPath;
                    break;
                case PathType.StreamingAssetsPath:
                    path = GetStreamingAssetsPath;
                    break;
                case PathType.PersistentDataPath:
                    path = GetPersistentDataPath;
                    break;
            }
            for (int i = 0; i < folders.Length; i++)
            {
                subPath += folders[i] + "/";
                CreateDirectory(path + subPath);
            }
            if (string.IsNullOrEmpty(subPath)) return path;
            return path + subPath;
        }
        /// <summary>
        /// 获取物体
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject GetGameObjectConvertedToName(string name)
        {
            return GameObject.Find(name);
        }
        /// <summary>
        /// 获取物体
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static GameObject GetGameObjectConvertedToTag(string tag)
        {
            return GameObject.FindGameObjectWithTag(tag);
        }
        /// <summary>
        /// 获取子物体
        /// </summary>
        /// <param name="go"></param>
        /// <param name="subs"></param>
        /// <returns></returns>
        public static GameObject GetChildGameObject(GameObject go, params string[] subs)
        {
            if (go == null) return null;
            string sub = string.Empty;
            for (int i = 0; i < subs.Length - 1; i++)
            {
                sub += subs[i] + "/";
            }
            sub += subs[subs.Length - 1];
            Transform transform = go.transform.Find(sub);
            if (transform == null) return null;
            return transform.gameObject;
        }
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T GetComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            return go.GetComponent<T>();
        }
        /// <summary>
        /// 获取子物体组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="subs"></param>
        /// <returns></returns>
        public static T GetChildComponent<T>(GameObject go, params string[] subs) where T : Component
        {
            GameObject obj = GetChildGameObject(go, subs);
            if (obj == null) return null;
            return GetComponent<T>(obj);
        }
        /// <summary>
        /// 增加组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T AddComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            T component = go.GetComponent<T>();
            if (component != null) return null;
            return go.AddComponent<T>();
        }
        /// <summary>
        /// 增加子物体组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="subs"></param>
        /// <returns></returns>
        public static T AddChildComponent<T>(GameObject go, params string[] subs) where T : Component
        {
            GameObject obj = GetChildGameObject(go, subs);
            if (obj == null) return null;
            return AddComponent<T>(obj);
        }
        /// <summary>
        /// 删除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        //public static void SafeDestroy<T>(GameObject go) where T : Component
        //{
        //    if (go == null) return;
        //    T component = GetComponent<T>(go);
        //    Destroy(component);
        //}
        /// <summary>
        /// 删除物体
        /// </summary>
        /// <param name="go"></param>
        public static void SafeDestroy(GameObject go)
        {
            if (go == null) return;
            Destroy(go);
        }
        /// <summary>
        /// 卸载资源
        /// </summary>
        public static void UnloadAssets()
        {
            Model.AssetManager.Instance.UnloadAllAssetsData();
        }
        /// <summary>
        /// 转化成Panel类型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PanelType ConvertStringToPanelType(string name)
        {
            name = name.Substring(0, name.IndexOf("Panel"));
            return (PanelType)Enum.Parse(typeof(PanelType), name);
        }
        /// <summary>
        /// 转化成String类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ConvertPanelTypeToString(PanelType type)
        {
            return type.ToString() + "Panel";
        }
        /// <summary>
        /// 转化成Log类型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static LogType ConvertStringToLogType(string name)
        {
            name = name.Substring(0, name.IndexOf("Panel"));
            return (LogType)Enum.Parse(typeof(LogType), name);
        }
        /// <summary>
        /// 转化成String类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ConvertLogTypeToString(LogType type)
        {
            return type.ToString() + "Panel";
        }
        /// <summary>
        /// 设置图片资源
        /// </summary>
        /// <param name="name"></param>
        /// <param name="image"></param>
        /// <param name="types"></param>
        public static void SetImage(string name, Image image, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = Model.AssetManager.Instance.LoadAssetData<Sprite>(name, ".png", false, true, types);
            image.sprite = sprite;
        }
        /// <summary>
        /// 设置图片资源
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spriterenderer"></param>
        /// <param name="types"></param>
        public static void SetImage(string name, SpriteRenderer spriterenderer, params string[] types)
        {
            if (string.IsNullOrEmpty(name)) return;
            Sprite sprite = Model.AssetManager.Instance.LoadAssetData<Sprite>(name, ".png", false, true, types);
            spriterenderer.sprite = sprite;
        }
        /// <summary>
        /// 屏幕坐标转换UI坐标
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public static Vector2 ScreenToUgui(RectTransform rect, Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, null, out Vector2 localPosition);
            return localPosition;
        }
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path"></param>
        public static DirectoryInfo CreateDirectory(string path)
        {
            if (Directory.Exists(path)) return null;
            return Directory.CreateDirectory(path);
        }
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folders"></param>
        public static void CreateDirectory(string path, params string[] folders)
        {
            CreateDirectory(path);
            string subPath = string.Empty;
            for (int i = 0; i < folders.Length; i++)
            {
                subPath += folders[i];
                CreateDirectory(path + "/" + subPath);
                subPath += "/";
            }
        }
        /// <summary>
        /// 保存资源到本地
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="bytes"></param>
        public static void SaveAsset(string path, string name, byte[] bytes)
        {
            Stream stream;
            FileInfo info = new FileInfo(path + name);
            if (info.Exists)
            {
                info.Delete();
                stream = info.Create();
            }
            else
            {
                stream = info.Create();
            }
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
        }
        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static byte[] GetAsset(string path, string name)
        {
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
            stream.Close();
            stream.Dispose();
            return bytes;
        }
        /// <summary>
        /// 上传资源
        /// </summary>
        /// <param name="url"></param>
        /// <param name="field"></param>
        /// <param name="bytes"></param>
        /// <param name="name"></param>
        /// <param name="mime"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerator Upload(string url, string field, byte[] bytes, string name, string mime, Action<bool> action)
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData(field, bytes, name, mime);
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                action(true);
            }
        }
        /// <summary>
        /// 下载资源
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static IEnumerator Download(string url, string name, params string[] folders)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                byte[] bytes = webRequest.downloadHandler.data;
                SaveAsset(GetPath(PathType.PersistentDataPath, folders), name, bytes);
            }
        }
        public static IEnumerator Download(string url, Action<byte[]> action)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                byte[] bytes = webRequest.downloadHandler.data;
                action(bytes);
            }
        }
        public static IEnumerator Download(string url, Action<Texture2D> action)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerTexture download = new DownloadHandlerTexture(true);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                action(download.texture);
            }
        }
        public static IEnumerator Download(string url, Action<Texture2D, byte[]> action)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerTexture download = new DownloadHandlerTexture(true);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                action(download.texture, download.data);
            }
        }
        public static IEnumerator Download(string url, AudioType type, Action<AudioClip> action)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAudioClip download = new DownloadHandlerAudioClip(webRequest.url, type);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                action(download.audioClip);
            }
        }
        public static IEnumerator Download(string url, AudioType type, Action<AudioClip, byte[]> action)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAudioClip download = new DownloadHandlerAudioClip(webRequest.url, type);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                action(download.audioClip, download.data);
            }
        }
        public static IEnumerator Download(string url, Action<AssetBundle> action)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAssetBundle download = new DownloadHandlerAssetBundle(webRequest.url, uint.MaxValue);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                action(download.assetBundle);
            }
        }
        public static IEnumerator Download(string url, Action<AssetBundle, byte[]> action)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAssetBundle download = new DownloadHandlerAssetBundle(webRequest.url, uint.MaxValue);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                action(download.assetBundle, download.data);
            }
        }
        /// <summary>
        /// 生成MD5
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateMD5(string path, string name)
        {
            //加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = GetAsset(path, name);
            if (bytes == null)
            {
                bytes = md5.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
            return null;
        }
        /// <summary>
        /// 设置分辨率
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetResolution(bool resolution, int width = 0, int height = 0)
        {
            Resolution[] resolutions = Screen.resolutions;
            if (width == 0)
            {
                width = resolutions[resolutions.Length - 1].width;
            }
            if (height == 0)
            {
                height = resolutions[resolutions.Length - 1].width;
            }
            Screen.SetResolution(width, height, resolution);
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RijndaelEncrypt(string key, string value)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            //加密
            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.Key = keyBytes;
            rijndael.Mode = CipherMode.ECB;
            rijndael.Padding = PaddingMode.PKCS7;
            ICryptoTransform crypto = rijndael.CreateEncryptor();
            //加密后的数据
            byte[] bytes = crypto.TransformFinalBlock(valueBytes, 0, valueBytes.Length);
            return Convert.ToBase64String(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RijndaelDecrypt(string key, string value)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] valueBytes = Convert.FromBase64String(value);
            //解密
            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.Key = keyBytes;
            rijndael.Mode = CipherMode.ECB;
            rijndael.Padding = PaddingMode.PKCS7;
            ICryptoTransform crypto = rijndael.CreateDecryptor();
            //解密后的数据
            byte[] bytes = crypto.TransformFinalBlock(valueBytes, 0, valueBytes.Length);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }
}