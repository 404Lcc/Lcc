using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public static class WebUtil
    {
        public static IEnumerator Upload(string url, string field, byte[] bytes, string name, string mime, Action<bool> complete = null, Action<string> error = null)
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData(field, bytes, name, mime);
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LogUtil.Log(webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                complete?.Invoke(true);
            }
        }
        public static IEnumerator Download(string url, Action<byte[]> complete = null, Action<string> error = null)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LogUtil.Log(webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                byte[] bytes = webRequest.downloadHandler.data;
                complete?.Invoke(bytes);
            }
        }
        public static IEnumerator Download(string url, Action<Texture2D> complete = null, Action<string> error = null)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerTexture download = new DownloadHandlerTexture(true);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LogUtil.Log(webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                complete?.Invoke(download.texture);
            }
        }
        public static IEnumerator Download(string url, Action<Texture2D, byte[]> complete = null, Action<string> error = null)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerTexture download = new DownloadHandlerTexture(true);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LogUtil.Log(webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                complete?.Invoke(download.texture, download.data);
            }
        }
        public static IEnumerator Download(string url, AudioType type, Action<AudioClip> complete = null, Action<string> error = null)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAudioClip download = new DownloadHandlerAudioClip(webRequest.url, type);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LogUtil.Log(webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                complete?.Invoke(download.audioClip);
            }
        }
        public static IEnumerator Download(string url, AudioType type, Action<AudioClip, byte[]> complete = null, Action<string> error = null)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAudioClip download = new DownloadHandlerAudioClip(webRequest.url, type);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LogUtil.Log(webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                complete?.Invoke(download.audioClip, download.data);
            }
        }
        public static IEnumerator Download(string url, Action<AssetBundle> complete = null, Action<string> error = null)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAssetBundle download = new DownloadHandlerAssetBundle(webRequest.url, uint.MaxValue);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LogUtil.Log(webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                complete?.Invoke(download.assetBundle);
            }
        }
        public static IEnumerator Download(string url, Action<AssetBundle, byte[]> complete = null, Action<string> error = null)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAssetBundle download = new DownloadHandlerAssetBundle(webRequest.url, uint.MaxValue);
            webRequest.downloadHandler = download;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LogUtil.Log(webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                complete?.Invoke(download.assetBundle, download.data);
            }
        }
    }
}