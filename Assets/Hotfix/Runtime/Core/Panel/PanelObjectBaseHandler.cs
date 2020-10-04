using System;
using System.Reflection;
using UnityEngine;

namespace Hotfix
{
    public class PanelObjectBaseHandler : ObjectBaseHandler
    {
        public PanelObjectBaseHandler(bool isKeep, bool isAssetBundle, params string[] types) : base(isKeep, isAssetBundle, types)
        {
            this.isKeep = isKeep;
            this.isAssetBundle = isAssetBundle;
            this.types = types;
        }
        public override GameObject CreateContainer(string name)
        {
            GameObject obj = base.CreateContainer(name);
            RectTransform rect = Util.GetComponent<RectTransform>(obj);
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            return obj;
        }
        public Panel CreatePanel(PanelType type, object data = null)
        {
            Panel info = new Panel();
            info.state = PanelState.Close;
            info.container = CreateContainer(Util.ConvertPanelTypeToString(type));
            if (info.container == null) return null;
            info.type = type;
            Assembly assembly = type.GetType().Assembly;
            Type classType = assembly.GetType(type.GetType().Namespace + "." + Util.ConvertPanelTypeToString(type));
            if (classType != null)
            {
                info.objectBase = LccViewFactory.CreateView(classType, info.container, data);
            }
            info.ClosePanel();
            return info;
        }
    }
}