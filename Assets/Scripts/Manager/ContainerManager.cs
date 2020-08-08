using System.Collections;
using UnityEngine;

namespace Model
{
    public class ContainerManager : MonoBehaviour
    {
        public Hashtable containers;
        void Awake()
        {
            InitManager();
        }
        public void InitManager()
        {
            containers = new Hashtable();
        }
        /// <summary>
        /// 创建容器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameObject CreateContainer(PanelType type, bool assetbundlemodel)
        {
            GameObject obj = IO.assetManager.LoadGameObject(GameUtil.ConvertPanelTypeToString(type), false, assetbundlemodel, AssetType.UI);
            if (obj == null) return null;
            obj.name = GameUtil.ConvertPanelTypeToString(type);
            obj.transform.SetParent(IO.gui.transform);
            RectTransform rect = GameUtil.GetComponent<RectTransform>(obj);
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            InitContainer(type, obj);
            return obj;
        }
        /// <summary>
        /// 创建容器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameObject CreateContainer(LogType type, bool assetbundlemodel)
        {
            GameObject obj = IO.assetManager.LoadGameObject(GameUtil.ConvertLogTypeToString(type), false, assetbundlemodel, AssetType.UI);
            if (obj == null) return null;
            obj.name = GameUtil.ConvertLogTypeToString(type);
            obj.transform.SetParent(IO.gui.transform);
            RectTransform rect = GameUtil.GetComponent<RectTransform>(obj);
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            InitContainer(type, obj);
            return obj;
        }
        /// <summary>
        /// 初始化容器
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        private void InitContainer(PanelType type, GameObject obj)
        {
            AddContainer(type, obj);
        }
        /// <summary>
        /// 初始化容器
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        private void InitContainer(LogType type, GameObject obj)
        {
            AddContainer(type, obj);
        }
        /// <summary>
        /// 增加容器
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        public void AddContainer(PanelType type, GameObject obj)
        {
            if (obj == null) return;
            containers.Add(type, obj);
        }
        /// <summary>
        /// 增加容器
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        public void AddContainer(LogType type, GameObject obj)
        {
            if (obj == null) return;
            containers.Add(type, obj);
        }
        /// <summary>
        /// 删除容器
        /// </summary>
        /// <param name="type"></param>
        public void RemoveContainer(PanelType type)
        {
            containers.Remove(type);
        }
        /// <summary>
        /// 删除容器
        /// </summary>
        /// <param name="type"></param>
        public void RemoveContainer(LogType type)
        {
            containers.Remove(type);
        }
    }
}