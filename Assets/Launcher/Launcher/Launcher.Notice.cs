using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public partial class Launcher
    {

        ////下载公告内容，string类型，需要解析出来
        ////一条公告：标题&内容&索引&类型
        ////多条公告用|连接
        ////示例：000&0000&0000&000|000&0000&0000&000|000&0000&0000&000|
        public string AnnouncementSave;
        public bool NoticeSucc;

        public string NoticeBoardSave;
        public bool NoticeBoardSucc;
        public IEnumerator GetNotice()
        {
            NoticeSucc = false;
            AnnouncementSave = "";
            string url = $"{Launcher.Instance.noticeUrl}/{Launcher.GameConfig.channel}/{Launcher.Instance.curLanguage}/notice.txt";
            Debug.Log("GetNotice url=" + url);
            UnityWebRequest uwr = UnityWebRequest.Get(url);

            var send = uwr.SendWebRequest();
            //避免unity Curl error 28 错误
            yield return send;

            if (uwr.isDone)
            {
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    string text = uwr.downloadHandler.text;
                    AnnouncementSave = text;
                    NoticeSucc = true;
                    uwr.Dispose();
                }
                else
                {
                    uwr.Dispose();
                }

            }
            else
            {
                uwr.Dispose();
            }
        }

        public bool CheckNoticeBoard()
        {
            if (!string.IsNullOrEmpty(Launcher.Instance.NoticeBoardSave) && Launcher.Instance.NoticeBoardSucc && !Launcher.Instance.IsAuditServer())
            {
                //停服了
                return true;
            }
            return false;
        }

        public IEnumerator GetNoticeBoard()
        {
            NoticeBoardSucc = false;
            NoticeBoardSave = "";
            string url = $"{Launcher.Instance.noticeUrl}/{Launcher.GameConfig.channel}/{Launcher.Instance.curLanguage}/noticeBoard.txt";
            Debug.Log("GetNoticeBoard url=" + url);
            UnityWebRequest uwr = UnityWebRequest.Get(url);

            var send = uwr.SendWebRequest();
            //避免unity Curl error 28 错误
            yield return send;

            if (uwr.isDone)
            {
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    string text = uwr.downloadHandler.text;
                    NoticeBoardSave = text;
                    NoticeBoardSucc = true;
                    uwr.Dispose();
                }
                else
                {
                    uwr.Dispose();
                }

            }
            else
            {
                uwr.Dispose();
            }
        }

    }
}