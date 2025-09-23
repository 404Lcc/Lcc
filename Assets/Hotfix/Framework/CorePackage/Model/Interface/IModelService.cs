using System;

namespace LccHotfix
{
    public interface IModelService : IService
    {
        void Init();
        
        T GetModel<T>() where T : ModelTemplate;

        object GetModel(Type type);
    }
}