using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using cfg;
using LccModel;
using Luban;
using RVO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace LccHotfix
{
    public interface IVideoService : IService
    {
        public bool VideoExist(string video);

        public VideoClip LoadVideo(string video);
        public void RemoveVideo(string video, VideoPlayer player);

        public VideoClip PlayVideo(string video, bool isAsset, VideoPlayer player, RawImage image, int width = 1920, int height = 1080);

        public void PauseVideo(VideoPlayer player);

        public void ContinueVideo(VideoPlayer player);

        public VideoClip GetVideoClip(string video);

        public bool IsPlayVideo(string video, bool isInside, VideoPlayer player);
    }
}