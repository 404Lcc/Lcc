using ET;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class ModelPoolManager : AObjectBase
    {
        public static ModelPoolManager Instance;
        public Transform parentRoot;
        public Dictionary<int, Queue<ModelBase>> cacheModelDict = new Dictionary<int, Queue<ModelBase>>();
        public Dictionary<string, GameObject> resourceDict = new Dictionary<string, GameObject>();

        public Dictionary<int, (string, string)> resourceNameDict = new Dictionary<int, (string, string)>();//resourceNameDict不要，这里换成读表 正式项目

        public override void Awake()
        {
            base.Awake();

            Instance = this;

            resourceNameDict.Add(1, ("name", "type"));
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;

            Object.Destroy(parentRoot.gameObject);
        }

        public void ResycleModel(ModelBase model)
        {
            if (cacheModelDict.TryGetValue(model.modelId, out Queue<ModelBase> queue))
            {
                queue.Enqueue(model);
            }
            else
            {
                queue = new Queue<ModelBase>();
                queue.Enqueue(model);
                cacheModelDict.Add(model.modelId, queue);
            }
            model.gameObject.SetActive(false);
            model.OnResycle();
        }

        public async ETTask<ModelBase> GetModelAsync(int modelId)
        {
            if (cacheModelDict.TryGetValue(modelId, out Queue<ModelBase> cache))
            {
                if (cache.Count > 0)
                {
                    var cacheModel = cache.Dequeue();
                    return cacheModel;
                }
            }
            GameObject modelRes = new GameObject();
            ModelBase model = modelRes.AddComponent<ModelBase>();
            SetParent(modelRes, parentRoot);
            GameObject modelObj = InstantiateModel(await LoadObjectAsync(modelRes, resourceNameDict[modelId].Item1, resourceNameDict[modelId].Item2));
            SetParent(modelObj, modelRes.transform);
            model.InitData(modelId, modelObj);
            model.gameObject.SetActive(true);
            return model;
        }
        private void SetParent(GameObject gameObject, Transform parent)
        {
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
        }
        private GameObject InstantiateModel(GameObject asset)
        {
            return GameObject.Instantiate(asset);
        }
        private async ETTask<GameObject> LoadObjectAsync(GameObject modelRes, string modelName, string type)
        {
            if (resourceDict.ContainsKey(modelName))
            {
                return resourceDict[modelName];
            }
            var asset = await AssetManager.Instance.AutoLoadAssetAsync<GameObject>(modelRes.transform, modelName, AssetSuffix.Prefab, type);
            if (asset != null)
            {
                resourceDict[modelName] = asset;
            }
            return asset;
        }
    }
}