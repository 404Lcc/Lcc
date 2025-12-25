using System;
using UnityEngine;

namespace LccHotfix
{
    public interface IAtlasService : IService
    {
        /// <summary>
        /// 根据精灵名获取一个精灵对象
        /// </summary>
        /// <param name="spriteName">精灵名</param>
        /// <returns></returns>
        public void GetSprite(string spriteName, Action<Sprite> callback);

        /// <summary>
        /// 根据图集名和精灵名获取一个精灵对象
        /// </summary>
        /// <param name="atlasName">图集名</param>
        /// <param name="spriteName">精灵名</param>
        /// <returns></returns>
        public void GetSprite(string atlasName, string spriteName, Action<Sprite> callback);
    }
}