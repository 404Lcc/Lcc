using UnityEngine;

namespace LccModel
{
    public class ModelBase : MonoBehaviour
    {
        public int modelId;
        private GameObject _modelObj;
        public void InitData(int modelId, GameObject modelObj)
        {
            this.modelId = modelId;
            this._modelObj = modelObj;
        }
        public void OnResycle()
        {
        }
        public GameObject GetModel()
        {
            return _modelObj;
        }
    }
}