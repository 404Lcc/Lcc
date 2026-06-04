using HotUpdate.Framework;

namespace LccHotfix
{
    public partial class LogicWorld
    {
        public LogicEntity AddEntity(string path)
        {
            var entity = CreateEntity();
            entity.AddComID(entity.creationIndex);

            if (string.IsNullOrEmpty(path))
            {
                return entity;
            }

            var mainObjectViewType = GetCreationInfo<BattleKernelCreationInfo>().MainObjectViewType;
            if (mainObjectViewType == null)
            {
                BattleLogger.LogError($"LogicWorld.AddEntity path={path}, MainObjectViewType == null");
                return entity;
            }

            var objViewLoader = ReferencePool.Acquire<ObjViewLoader>();
            objViewLoader.Category = EViewCategory.MainGameObject;
            objViewLoader.ObjName = path;
            objViewLoader.ViewClassType = mainObjectViewType;
            objViewLoader.IsAsync = true;
            if (entity.hasComViewLoader)
            {
                entity.ChangeViewLoad(objViewLoader);
            }
            else
            {
                entity.AddComViewLoader(objViewLoader);
            }

            return entity;
        }
    }
}
