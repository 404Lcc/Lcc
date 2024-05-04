using Entitas;

public class LogicComponent : IComponent, IDispose
{
    private LogicEntity _owner;

    public LogicEntity Owner => _owner;

    public virtual void PostInitialize(LogicEntity owner)
    {
        _owner = owner;
    }

    public virtual void Dispose()
    {
        _owner = null;
    }
}