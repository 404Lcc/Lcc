using System;

namespace LccHotfix
{
    public class ObjViewLoader : ViewLoaderBase
    {
        public string ObjName; // 资源路径
        public string BindPointName; // 挂载的挂点名称
        public bool BindParentTf;

        public override void Load(LogicEntity entity, Action<LogicEntity, int, IReceiveLoaded> callback)
        {
            if (IsPrepare)
                return;
            if (!string.IsNullOrEmpty(ObjName))
            {
                IsPrepare = true;
                Main.GameObjectPoolService.GetObjectAsync(ObjName, (operation) =>
                {
                    var loaded = new ObjReceiveLoaded();
                    loaded.Init(operation);
                    callback?.Invoke(entity, Category, loaded);
                });
            }
        }
    }
}