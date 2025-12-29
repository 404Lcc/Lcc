using System.Reflection;
using LccModel;

namespace LccHotfix
{
    public interface IHotfixBridgeService : IService
    {
        void Init();

        object CrossDomainCallMethod(ClassInfo clsInfo, BindingFlags flag, object[] param, object instance = null);

        object CrossDomainCallProperty(ClassInfo clsInfo, BindingFlags flag, object instance = null);

        object CrossDomainCallField(ClassInfo clsInfo, BindingFlags flag, object instance = null);
    }
}