using cfg;

namespace LccHotfix
{
    public interface IConfigService : IService
    {
        Tables Tables { get; set; }
        public bool Initialized { get; }
        void Init();
    }
}