using UnityEngine;

namespace Hotfix
{
    public static class LccUtil
    {
        /// <summary>
        /// 卸载资源
        /// </summary>
        public static void UnloadAssets()
        {
            Model.AssetManager.Instance.UnloadAllAssetsData();
        }
        /// <summary>
        /// 设置分辨率
        /// </summary>
        /// <param name="isFullscreen"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetResolution(bool isFullscreen, int width = 0, int height = 0)
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
            Screen.SetResolution(width, height, isFullscreen);
        }
    }
}