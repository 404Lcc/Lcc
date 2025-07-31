using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public enum IconType
    {
        None,
        ItemNormal, //item
        RewardNormal, //奖励（通用）
    }

    public enum IconSize
    {
        Size_00,
        Size_50,
        Size_100,
        Size_150,
    }

    public class IconAttribute : AttributeBase
    {
        public IconType iconType;

        public IconAttribute(IconType iconType)
        {
            this.iconType = iconType;
        }
    }

    internal class IconManager : Module, IIconService
    {

        private Dictionary<IconType, Type> iconTypeDict = new Dictionary<IconType, Type>();

        private GameObject root;


        public IconManager()
        {
            root = new GameObject("IconRoot");
            GameObject.DontDestroyOnLoad(root);

            foreach (Type item in Main.CodeTypesService.GetTypes(typeof(IconAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(IconAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    IconAttribute iconAttribute = (IconAttribute)atts[0];
                    iconTypeDict.Add(iconAttribute.iconType, item);
                }
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            foreach (var item in iconTypeDict.Values)
            {
                ReferencePool.RemoveAll(item);
            }

            iconTypeDict.Clear();

            GameObject.Destroy(root);
            root = null;
        }

        public IconBase GetIcon(IconType type, Transform parent, IconSize size = IconSize.Size_100)
        {
            if (parent == null)
                return null;

            IconBase icon = GetCacheIcon(type);

            if (icon == null)
                return null;

            ClientTools.ResetTransform(icon.GameObject.transform, parent);
            ClientTools.ResetRectTransfrom(icon.GameObject.transform as RectTransform);
            var scale = GetIconScale(size);
            icon.GameObject.transform.localScale = scale;
            icon.GameObject.SetActive(true);
            icon.Clear();
            return icon;
        }


        private IconBase GetCacheIcon(IconType iconType)
        {
            IconBase iconBase = ReferencePool.Acquire(iconTypeDict[iconType]) as IconBase;
            if (iconBase.GameObject == null)
            {
                var obj = ResGameObject.LoadGameObject(iconType.ToString(), true).ResGO;
                iconBase.InitIcon(obj, iconType);
            }

            return iconBase;
        }

        /// <summary>
        /// icon回收接口
        /// </summary>
        public void RecycleIcon<T>(T iconBase) where T : IconBase
        {
            if (iconBase == null)
                return;

            ReferencePool.Release(iconBase);

            var go = iconBase.GameObject;
            if (go != null)
            {
                go.SetActive(false);
                if (root != null)
                {
                    go.transform.SetParent(root.transform);
                }
            }
        }

        public Vector3 GetIconScale(IconSize size)
        {
            switch (size)
            {
                case IconSize.Size_00:
                    return Vector3.zero;
                case IconSize.Size_50:
                    return Vector3.one * 0.5f;
                case IconSize.Size_100:
                    return Vector3.one;
                case IconSize.Size_150:
                    return Vector3.one * 1.5f;
                default:
                    return Vector3.one;
            }
        }
    }
}