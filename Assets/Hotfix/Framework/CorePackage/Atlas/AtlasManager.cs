using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace LccHotfix
{
    public class AtlasManager : Module, IAtlasService
    {
        private IAssetLoader _loader;
        private readonly IDictionary<string, Sprite> _loadedSprites = new Dictionary<string, Sprite>(256);

        public AtlasManager()
        {
            SpriteAtlasManager.atlasRequested += RequestAtlas;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
            SpriteAtlasManager.atlasRequested -= RequestAtlas;
            _loader?.Release();
        }

        private void RequestAtlas(string atlasName, Action<SpriteAtlas> callback)
        {
            _loader ??= new AssetLoader();
            _loader.LoadAssetAsync<SpriteAtlas>(atlasName, (loadHandle) =>
            {
                var atlas = loadHandle.AssetObject as SpriteAtlas;
                if (atlas is not null)
                {
                    var sprites = new Sprite[atlas.spriteCount];
                    atlas.GetSprites(sprites);
                    foreach (var sprite in sprites)
                    {
                        sprite.name = sprite.name.Replace("(Clone)", "");
                        var spriteName = $"{atlasName}_{sprite.name}";
                        if (!_loadedSprites.ContainsKey(spriteName))
                            _loadedSprites[spriteName] = sprite;
                    }
                }

                callback.Invoke(atlas);
            });
        }

        /// <summary>
        /// 根据精灵名获取一个精灵对象
        /// </summary>
        /// <param name="spriteName">精灵名</param>
        /// <returns></returns>
        public void GetSprite(string spriteName, Action<Sprite> callback)
        {
            _loader ??= new AssetLoader();
            _loader.LoadAssetAsync<Sprite>(spriteName, (loadHandle) => { callback?.Invoke(loadHandle.AssetObject as Sprite); });
        }

        /// <summary>
        /// 根据图集名和精灵名获取一个精灵对象
        /// </summary>
        /// <param name="atlasName">图集名</param>
        /// <param name="spriteName">精灵名</param>
        /// <returns></returns>
        public void GetSprite(string atlasName, string spriteName, Action<Sprite> callback)
        {
            var spriteKey = $"{atlasName}_{spriteName}";
            if (_loadedSprites.TryGetValue(spriteKey, out var sprite))
            {
                callback?.Invoke(sprite);
            }

            RequestAtlas(atlasName, (atlas) =>
            {
                if (_loadedSprites.TryGetValue(spriteKey, out var sprite))
                {
                    callback?.Invoke(sprite);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }
    }
}