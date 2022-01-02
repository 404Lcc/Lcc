using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace LccModel
{
    public class VideoManager : Singleton<VideoManager>
    {
        public Hashtable videos = new Hashtable();
        public bool VideoExist(string video)
        {
            if (videos.ContainsKey(video))
            {
                return true;
            }
            return false;
        }
        public VideoClip LoadVideo(string video)
        {
            VideoClip clip = AssetManager.Instance.LoadAsset<VideoClip>(video, ".mp4", false, true, AssetType.Video);
            videos.Add(video, clip);
            return clip;
        }
        public void RemoveVideo(string video, VideoPlayer player)
        {
            if (VideoExist(video))
            {
                player.source = VideoSource.VideoClip;
                player.clip = null;
                player.url = string.Empty;
                player.Stop();
                videos.Remove(video);
            }
        }
        public VideoClip PlayVideo(string video, bool isAsset, VideoPlayer player, RawImage image, int width = 1920, int height = 1080)
        {
            player.targetTexture = new RenderTexture(width, height, 0);
            if (VideoExist(video))
            {
                VideoClip clip = GetVideoClip(video);
                player.source = VideoSource.VideoClip;
                player.clip = clip;
                image.texture = player.targetTexture;
                player.Play();
                return clip;
            }
            if (isAsset)
            {
                VideoClip clip = LoadVideo(video);
                player.source = VideoSource.VideoClip;
                player.clip = clip;
                image.texture = player.targetTexture;
                player.Play();
                return clip;
            }
            else
            {
                player.source = VideoSource.Url;
                player.url = video;
                image.texture = player.targetTexture;
                player.Play();
                return null;
            }
        }
        public void PauseVideo(VideoPlayer player)
        {
            if (player.isPlaying)
            {
                player.Pause();
            }
        }
        public void ContinueVideo(VideoPlayer player)
        {
            if (player.isPaused)
            {
                player.Play();
            }
        }
        public VideoClip GetVideoClip(string video)
        {
            if (VideoExist(video))
            {
                VideoClip clip = videos[video] as VideoClip;
                return clip;
            }
            return null;
        }
        public bool IsPlayVideo(string video, bool isInside, VideoPlayer player)
        {
            if (isInside)
            {
                if (GetVideoClip(video))
                {
                    VideoClip clip = GetVideoClip(video);
                    if (player.clip == clip && player.isPlaying)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            else
            {
                if (player.url == video && player.isPlaying)
                {
                    return true;
                }
                return false;
            }
        }
    }
}