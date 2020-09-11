using System.Collections;
using UnityEngine;

namespace Model
{
    public class ContainerManager : Singleton<ContainerManager>
    {
        public Hashtable containers = new Hashtable();
        /// <summary>
        /// 创建容器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assetBundleMode"></param>
        /// <returns></returns>
        public GameObject CreateContainer(string name, bool assetBundleMode)
        {
            GameObject obj = AssetManager.Instance.LoadGameObject(name, false, assetBundleMode, AssetType.UI);
            if (obj == null) return null;
            obj.name = name;
            obj.transform.SetParent(Objects.gui.transform);
            RectTransform rect = GameUtil.GetComponent<RectTransform>(obj);
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            InitContainer(name, obj);
            return obj;
        }
        /// <summary>
        /// 初始化容器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        private void InitContainer(string name, GameObject obj)
        {
            AddContainer(name, obj);
        }
        /// <summary>
        /// 增加容器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public void AddContainer(string name, GameObject obj)
        {
            if (obj == null) return;
            containers.Add(name, obj);
        }
        /// <summary>
        /// 删除容器
        /// </summary>
        /// <param name="name"></param>
        public void RemoveContainer(string name)
        {
            containers.Remove(name);
        }
    }
}