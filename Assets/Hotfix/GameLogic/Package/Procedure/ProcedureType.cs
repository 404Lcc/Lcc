namespace LccHotfix
{
    public enum ProcedureType : int
    {
        None,
        Login = 1 << 0,
        Main = 1 << 1,
        Battle = 1 << 2,
    }
}