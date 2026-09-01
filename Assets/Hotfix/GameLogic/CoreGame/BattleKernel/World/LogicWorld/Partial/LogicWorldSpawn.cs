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

            var objViewLoader = ReferencePool.Acquire<ObjViewLoader>();
            objViewLoader.Category = EViewCategory.MainGameObject;
            objViewLoader.ObjName = path;
            objViewLoader.ViewClassType = GetCreationInfo<BattleKernelCreationInfo>()?.DefaultMainGameObjectViewType;
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

        public LogicEntity AddEntity<T>(string path) where T : MainGameObjectView
        {
            var entity = CreateEntity();
            entity.AddComID(entity.creationIndex);

            if (string.IsNullOrEmpty(path))
            {
                return entity;
            }

            var objViewLoader = ReferencePool.Acquire<ObjViewLoader>();
            objViewLoader.Category = EViewCategory.MainGameObject;
            objViewLoader.ObjName = path;
            objViewLoader.ViewClassType = typeof(T);
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
