using System.Collections.Generic;

namespace LccModel
{
    public class ExecutionComponent : Component
    {
        public Combat Combat => GetParent<Combat>();

        public Dictionary<int, ExecutionConfigObject> executionDict = new Dictionary<int, ExecutionConfigObject>();


        public ExecutionConfigObject AttachExecution(int executionId)
        {
            if (GetExecution(executionId) != null)
            {
                return GetExecution(executionId);
            }
            ExecutionConfigObject executionConfigObject = AssetManager.Instance.LoadAsset<ExecutionConfigObject>(out var handle, $"Execution_{executionId}", AssetSuffix.Asset, AssetType.SkillConfig, AssetType.Execution);
            AssetManager.Instance.UnLoadAsset(handle);
            if (executionConfigObject == null)
            {
                return null;
            }
            if (!executionDict.ContainsKey(executionConfigObject.Id))
            {
                executionDict.Add(executionConfigObject.Id, executionConfigObject);
            }
            return executionConfigObject;
        }
        public ExecutionConfigObject GetExecution(int executionId)
        {
            if (executionDict.ContainsKey(executionId))
            {
                return executionDict[executionId];
            }
            return null;
        }
    }
}