using LccModel;
using System;
using UnityEngine;

namespace LccHotfix
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
            Vector2 anchorMin = Screen.safeArea.position;
            Vector2 anchorMax = Screen.safeArea.position + Screen.safeArea.size;
            anchorMin = new Vector2(anchorMin.x / Screen.width, anchorMin.y / Screen.height);
            anchorMax = new Vector2(anchorMax.x / Screen.width, anchorMax.y / Screen.height);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            return gameObject;
        }
        public Panel CreatePanel(PanelType type, object[] datas)
        {
            Panel panel = new Panel();
            panel.State = PanelState.Close;
            GameObject gameObject = CreateGameObject(type.ToPanelString(), Objects.Canvas.transform);
            if (gameObject == null) return null;
            panel.Type = type;
            Type classType = Manager.Instance.GetType(type.ToPanelString());
            if (classType != null)
            {
                GameObjectComponent gameObjectComponent = ObjectBaseFactory.Create<GameObjectComponent, GameObject>(PanelManager.Instance, gameObject);
                AObjectBase panelComponent = gameObjectComponent.AddComponent(classType, datas);
                panel.AObjectBase = panelComponent;
            }
            panel.ClosePanel();
            return panel;
        }
    }
}