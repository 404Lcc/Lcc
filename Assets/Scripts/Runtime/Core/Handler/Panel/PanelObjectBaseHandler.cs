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
        public override GameObject CreateGameObject(string name)
        {
            GameObject gameObject = base.CreateGameObject(name);
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
            PanelData panelData = new PanelData();
            panelData.state = PanelState.Close;
            panelData.gameObject = CreateGameObject(type.ToPanelString());
            if (panelData.gameObject == null) return null;
            panelData.type = type;
            Assembly assembly = type.GetType().Assembly;
            Type classType = assembly.GetType(type.GetType().Namespace + "." + type.ToPanelString());
            if (classType != null)
            {
                panelData.objectBase = LccViewFactory.CreateView(classType, panelData.gameObject, data);
            }
            panelData.ClosePanel();
            return panelData;
        }
    }
}