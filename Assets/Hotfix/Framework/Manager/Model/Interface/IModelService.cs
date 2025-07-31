using System;

namespace LccHotfix
{
    public interface IModelService : IService
    {
        T GetModel<T>() where T : ModelTemplate;

        object GetModel(Type type);
    }
}