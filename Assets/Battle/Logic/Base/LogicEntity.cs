using Entitas;

public partial class LogicEntity : Entity
{
    private readonly EntityComponentChanged _addComponent;
    private readonly EntityComponentChanged _removeComponent;

    public LogicEntity()
    {
        _addComponent = OnAddComponent;
        _removeComponent = OnRemoveComponent;
    }
    public virtual void Enter()
    {
        OnComponentAdded += _addComponent;
        OnComponentRemoved += _removeComponent;
    }

    public virtual void Leave()
    {
        OnComponentAdded -= _addComponent;
        OnComponentRemoved -= _removeComponent;
    }

    private void OnAddComponent(Entity entity, int index, IComponent component)
    {
        if (component is LogicComponent logicComponent)
        {
            logicComponent.PostInitialize(this);
        }
    }
    private void OnRemoveComponent(Entity entity, int index, IComponent component)
    {
        if (component is IDispose dispose)
        {
            dispose.Dispose();
        }
    }
}