namespace LccHotfix
{
    public enum ProcedureType : int
    {
        None,
        Login = 1 << 0,
        Main = 1 << 1,
        Robot = 1 << 2,
    }
}