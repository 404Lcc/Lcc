using System;
using System.Reflection;
using UnityEngine;

namespace Hotfix
{
    public class PanelObjectBaseHandler : AObjectBaseHandler
    {
        public PanelObjectBaseHandler(bool isKeep, bool isAssetBundle, params string[] types) : base(isKeep, isAssetBundle, types)
        {
            this.isKeep = isKeep;
            this.isAssetBundle = isAssetBundle;
            this.types = types;
        }
        public override GameObject CreateGameObject(string name, Transform parent)
        {
            GameObject gameObject = base.CreateGameObject(name, parent);
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
        public Panel CreatePanel(PanelType type, object data)
        {
            Panel panel = new Panel();
            panel.State = PanelState.Close;
            GameObject gameObject = CreateGameObject(type.ToPanelString(), Model.Objects.GUI.transform);
            if (gameObject == null) return null;
            panel.Type = type;
            Assembly assembly = type.GetType().Assembly;
            Type classType = assembly.GetType(type.GetType().Namespace + "." + type.ToPanelString());
            if (classType != null)
            {
                panel.AObjectBase = LccViewFactory.CreateView(classType, gameObject, data);
            }
            panel.ClosePanel();
            return panel;
        }
    }
}