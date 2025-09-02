using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public class GameNotice
    {
        ////下载公告内容，string类型，需要解析出来
        ////一条公告：标题&内容&索引&类型
        ////多条公告用|连接
        ////示例：000&0000&0000&000|000&0000&0000&000|000&0000&0000&000|
        public string announcementSave;
        public bool noticeSucc;

        public string noticeBoardSave;
        public bool noticeBoardSucc;

        public string noticeWhiteListSave;
        public bool noticeWhiteListSucc;

        public void OpenGameNotice(Action callback)
        {
            //Debug.Log("打开公告");
            //提审测试服不显示公告
            if (Launcher.Instance.IsAuditServer())
            {
                return;
            }
            Launcher.Instance.StartCoroutine(GetNotice(callback));
        }

        public void OpenGameNoticeBoard(Action callback)
        {
            //Debug.Log("打开公告");
            //提审测试服不显示公告
            if (Launcher.Instance.IsAuditServer())
            {
                return;
            }
            if (CheckNoticeBoard())
            {
                callback?.Invoke();
            }
        }

        private IEnumerator GetNotice(Action callback)
        {
            noticeSucc = false;
            announcementSave = "";
            string url = $"{Launcher.Instance.GameServerConfig.noticeUrl}/{Launcher.Instance.GameConfig.channel}/{Launcher.Instance.GameLanguage.curLanguage}/notice.txt";
            Debug.Log("GetNoticeBoard url=" + url);

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
#if UNITY_EDITOR
            web.timeout = 2;
#else
		    web.timeout = 20;
#endif

            yield return web.SendWebRequest();

            if (!string.IsNullOrEmpty(web.error))
            {
                web.Dispose();
                web = UnityWebRequest.Get(url);
                web.timeout = 20;
                yield return web.SendWebRequest();
            }

            if (!string.IsNullOrEmpty(web.error))
            {
            }
            else
            {
                string text = web.downloadHandler.text;
                announcementSave = text;
                noticeSucc = true;
                callback?.Invoke();
            }
        }

        public bool CheckNoticeBoard()
        {
            //白名单中不显示公告
            if (noticeWhiteListSucc && !string.IsNullOrEmpty(noticeWhiteListSave))
            {
                foreach (var item in noticeWhiteListSave.Split('\n'))
                {
                    if (item == SystemInfo.deviceUniqueIdentifier)
                    {
                        Debug.Log("设备id = " + SystemInfo.deviceUniqueIdentifier + " 在白名单中");
                        return false;
                    }
                    else
                    {
                        Debug.Log("设备id = " + SystemInfo.deviceUniqueIdentifier + " 不在白名单中");
                    }
                }
            }
            if (!string.IsNullOrEmpty(noticeBoardSave) && noticeBoardSucc)
            {
                //停服了
                return true;
            }
            return false;
        }

        public IEnumerator GetNoticeBoard()
        {
            yield return GetNoticeWhiteList();

            noticeBoardSucc = false;
            noticeBoardSave = "";
            string url = $"{Launcher.Instance.GameServerConfig.noticeUrl}/{Launcher.Instance.GameConfig.channel}/{Launcher.Instance.GameLanguage.curLanguage}/noticeBoard.txt";
            Debug.Log("GetNoticeBoard url=" + url);

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
#if UNITY_EDITOR
            web.timeout = 2;
#else
		    web.timeout = 20;
#endif

            yield return web.SendWebRequest();

            if (!string.IsNullOrEmpty(web.error))
            {
                web.Dispose();
                web = UnityWebRequest.Get(url);
                web.timeout = 20;
                yield return web.SendWebRequest();
            }

            if (!string.IsNullOrEmpty(web.error))
            {
            }
            else
            {
                string text = web.downloadHandler.text;
                noticeBoardSave = text;
                noticeBoardSucc = true;
            }
        }

        private IEnumerator GetNoticeWhiteList()
        {
            noticeWhiteListSucc = false;
            noticeWhiteListSave = "";
            string url = $"{Launcher.Instance.GameServerConfig.noticeUrl}/{Launcher.Instance.GameConfig.channel}/noticeWhiteList.txt";
            Debug.Log("GetNoticeWhiteList url=" + url);

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
#if UNITY_EDITOR
            web.timeout = 2;
#else
		    web.timeout = 20;
#endif

            yield return web.SendWebRequest();

            if (!string.IsNullOrEmpty(web.error))
            {
                web.Dispose();
                web = UnityWebRequest.Get(url);
                web.timeout = 20;
                yield return web.SendWebRequest();
            }

            if (!string.IsNullOrEmpty(web.error))
            {
            }
            else
            {
                string text = web.downloadHandler.text;
                noticeWhiteListSave = text;
                noticeWhiteListSucc = true;
            }
        }

    }
}