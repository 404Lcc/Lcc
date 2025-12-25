using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public class UIConstant
    {
        public const int LayerMaskUI = 5;
    }

    public class UIRoot : IUIRoot
    {
        private Dictionary<UILayerID, UILayer> _uiLayers = new Dictionary<UILayerID, UILayer>();
        private Dictionary<string, ElementNode> _elementNodes = new Dictionary<string, ElementNode>();

        public UIRoot(GameObject rootObject)
        {
            _root = rootObject;
        }

        private GameObject _eventSystem;
        private GameObject _root;
        private Transform _transform;
        private Canvas _canvas;
        private Camera _uiCamera;

        public Camera RenderCamera => _uiCamera ??= Transform.Find("UICamera").GetComponent<Camera>();
        public Canvas Canvas => _canvas ??= Transform.Find("Canvas").GetComponent<Canvas>();
        public Transform Transform => _transform ??= _root.transform;

        public void Initialize()
        {
            _root ??= CreateRootObject();
            _root.name = "UIRoot";
            Object.DontDestroyOnLoad(_root);

            var rootTrans = _root.transform;
            rootTrans.localScale = Vector3.one;
            rootTrans.localPosition = new Vector3(0, 10000, 0);
            rootTrans.localRotation = Quaternion.identity;

            var canvasTransform = Canvas.transform;
            for (UILayerID layerId = UILayerID.HUD; layerId <= UILayerID.Debug; layerId++)
            {
                var layer = new UILayer(this, layerId);
                layer.Create(canvasTransform);
                _uiLayers[layerId] = layer;
            }
        }

        public void Finalize()
        {
            foreach (var layerInfo in _uiLayers)
            {
                layerInfo.Value.Destroy();
            }

            _uiLayers.Clear();
            _elementNodes.Clear();

            _transform = null;
            _canvas = null;
            _uiCamera = null;

            _root.transform.SetParent(null);
            Object.Destroy(_root);
            _root = null;
        }

        public ElementNode Find(string name)
        {
            return _elementNodes.TryGetValue(name, out var node) ? node : null;
        }

        public bool Find(ElementNode elementNode, out string name)
        {
            foreach (var kv in _elementNodes)
            {
                if (kv.Value.Equals(elementNode))
                {
                    name = kv.Key;
                    return true;
                }
            }

            name = null;
            return false;
        }

        public void Attach(string name, ElementNode elementNode)
        {
            if (_elementNodes.ContainsKey(name))
            {
                return;
            }

            _elementNodes[name] = elementNode;
            elementNode.AttachedToRoot(this);
        }

        public void Detach(ElementNode elementNode)
        {
            if (elementNode is null)
                return;

            foreach (var kv in _elementNodes)
            {
                if (kv.Value.Equals(elementNode))
                {
                    _elementNodes.Remove(kv.Key);
                    break;
                }
            }

            elementNode.DetachedFromRoot();
        }

        private GameObject CreateRootObject()
        {
            var root = new GameObject("UIRoot")
            {
                layer = UIConstant.LayerMaskUI
            };
            // 创建UICamera
            var camera = new GameObject("UICamera", typeof(Camera))
            {
                layer = UIConstant.LayerMaskUI
            };
            var cameraComponent = camera.GetComponent<Camera>();
            // cameraComponent.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 5;
            _uiCamera = cameraComponent;

            // 创建UI画布
            var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster))
            {
                layer = UIConstant.LayerMaskUI
            };
            var canvasComponent = canvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponent.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            canvasComponent.worldCamera = cameraComponent;
            _canvas = canvasComponent;

            canvas.transform.SetParent(root.transform);
            camera.transform.SetParent(root.transform);

            // 创建EventSystem
            if (!EventSystem.current)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystem.transform.SetParent(root.transform);
                eventSystem.SetActive(!EventSystem.current);
                _eventSystem = eventSystem;
            }
            else
            {
                _eventSystem = EventSystem.current.gameObject;
            }

            return root;
        }

        public UILayer GetLayerByID(UILayerID uiLayerId) => _uiLayers[uiLayerId];
    }
}