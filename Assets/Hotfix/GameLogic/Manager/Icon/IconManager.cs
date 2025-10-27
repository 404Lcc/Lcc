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

        public IconManager()
        {
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
        }

        public IconBase GetIcon(IconType type, Transform parent, IconSize size = IconSize.Size_100)
        {
            if (parent == null)
                return null;

            IconBase iconBase = ReferencePool.Acquire(iconTypeDict[type]) as IconBase;
            iconBase.InitIcon(type, parent, size);
            return iconBase;
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