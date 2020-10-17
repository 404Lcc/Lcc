using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Model
{
    public static class WebUtil
    {
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
                FileUtil.SaveAsset(PathUtil.GetPath(PathType.PersistentDataPath, folders) + name, bytes);
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
    }
}