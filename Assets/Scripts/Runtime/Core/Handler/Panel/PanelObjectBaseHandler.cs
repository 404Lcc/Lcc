using System;
using System.Reflection;
using UnityEngine;

namespace Model
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
            GameObject gameObject = base.CreateContainer(name);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            return gameObject;
        }
        public PanelData CreatePanel(PanelType type, object data = null)
        {
            PanelData info = new PanelData();
            info.state = PanelState.Close;
            info.gameObject = CreateContainer(type.ToPanelString());
            if (info.gameObject == null) return null;
            info.type = type;
            Assembly assembly = type.GetType().Assembly;
            Type classType = assembly.GetType(type.GetType().Namespace + "." + type.ToPanelString());
            if (classType != null)
            {
                info.objectBase = LccViewFactory.CreateView(classType, info.gameObject, data);
            }
            info.ClosePanel();
            return info;
        }
    }
}