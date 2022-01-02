using LccModel;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LccHotfix
{
    public static class WebUtil
    {
        public static async ETTask<bool> Upload(string url, string field, byte[] bytes, string name, string mime)
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData(field, bytes, name, mime);
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
            await webRequest.SendWebRequest();
            if (webRequest.IsError())
            {
                LogUtil.Log(webRequest.error);
                return false;
            }
            else
            {
                return true;
            }
        }
        public static async ETTask<string> DownloadText(string url)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            await webRequest.SendWebRequest();
            if (webRequest.IsError())
            {
                LogUtil.Log(webRequest.error);
                return null;
            }
            else
            {
                string text = webRequest.downloadHandler.text;
                return text;
            }
        }
        public static async ETTask<byte[]> DownloadBytes(string url)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            await webRequest.SendWebRequest();
            if (webRequest.IsError())
            {
                LogUtil.Log(webRequest.error);
                return null;
            }
            else
            {
                byte[] bytes = webRequest.downloadHandler.data;
                return bytes;
            }
        }
        public static async ETTask<Texture2D> DownloadTexture2D(string url)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerTexture download = new DownloadHandlerTexture(true);
            webRequest.downloadHandler = download;
            await webRequest.SendWebRequest();
            if (webRequest.IsError())
            {
                LogUtil.Log(webRequest.error);
                return null;
            }
            else
            {
                return download.texture;
            }
        }
        public static async ETTask<AudioClip> DownloadAudioClip(string url, AudioType type)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAudioClip download = new DownloadHandlerAudioClip(webRequest.url, type);
            webRequest.downloadHandler = download;
            await webRequest.SendWebRequest();
            if (webRequest.IsError())
            {
                LogUtil.Log(webRequest.error);
                return null;
            }
            else
            {
                return download.audioClip;
            }
        }
        public static async ETTask<AssetBundle> DownloadAssetBundle(string url)
        {
            url = Uri.EscapeUriString(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            DownloadHandlerAssetBundle download = new DownloadHandlerAssetBundle(webRequest.url, uint.MaxValue);
            webRequest.downloadHandler = download;
            await webRequest.SendWebRequest();
            if (webRequest.IsError())
            {
                LogUtil.Log(webRequest.error);
                return null;
            }
            else
            {
                return download.assetBundle;
            }
        }
    }
}