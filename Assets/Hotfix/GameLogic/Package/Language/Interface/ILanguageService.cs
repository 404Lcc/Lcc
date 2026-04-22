namespace LccHotfix
{
    public interface ILanguageService : IService
    {
        void Init();
        string GetValue(string key, params object[] args);
        string GetValue(int id, params object[] args);
        string GetValue(uint id, params object[] args);
        string GetKey(int id);
        string GetKey(uint id);
    }
}