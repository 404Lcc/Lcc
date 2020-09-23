using System;
using System.Reflection;
using UnityEngine;

namespace Model
{
    public class PanelObjectBaseHandler : ObjectBaseHandler
    {
        public PanelObjectBaseHandler(bool keep, bool assetBundleMode, params string[] types) : base(keep, assetBundleMode, types)
        {
            this.keep = keep;
            this.assetBundleMode = assetBundleMode;
            this.types = types;
        }
        public override GameObject CreateContainer(string name)
        {
            GameObject obj = base.CreateContainer(name);
            RectTransform rect = GameUtil.GetComponent<RectTransform>(obj);
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            return obj;
        }
        public PanelInfo CreatePanel(PanelType type, object data = null)
        {
            PanelInfo info = new PanelInfo();
            info.state = InfoState.Close;
            info.container = CreateContainer(GameUtil.ConvertPanelTypeToString(type));
            if (info.container == null) return null;
            info.type = type;
            Assembly assembly = type.GetType().Assembly;
            Type classType = assembly.GetType(type.GetType().Namespace + "." + GameUtil.ConvertPanelTypeToString(type));
            if (classType != null)
            {
                info.objectBase = LccViewFactory.CreateView(classType, info.container, data);
            }
            info.ClosePanel();
            return info;
        }
    }
}