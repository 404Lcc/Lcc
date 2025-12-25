using System;
using UnityEngine;

namespace LccHotfix
{
    public static class IconUtility
    {
        public static string GetIcon(int imageId)
        {
            var config = Main.ConfigService.Tables.TBIcon.Get(imageId);
            if (config == null)
            {
                Log.Error("Icon不存在 id = " + imageId);
                return string.Empty;
            }

            return config.ImageName;
        }

        public static void GetSprite(string spriteName, Action<Sprite> callback)
        {
            Main.AtlasService.GetSprite(spriteName, callback);
        }
    }
}